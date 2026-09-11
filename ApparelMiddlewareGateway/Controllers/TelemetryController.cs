using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApparelMiddlewareGateway.Data;
using ApparelMiddlewareGateway.Models;
using ApparelMiddlewareGateway.DTOs;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ApparelMiddlewareGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class TelemetryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TelemetryController> _logger;

        public TelemetryController(AppDbContext context, ILogger<TelemetryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>The authenticated operator's id, taken from the token.</summary>
        private string OperatorId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";

        /// <summary>The production line the operator is authorised for, taken from the token.</summary>
        private string? OperatorAssignedLine => User.FindFirst("AssignedLine")?.Value;

        // Endpoint to fetch the Cutting Job and its associated Rolls.
        // The LCNC app calls this first to populate the operator's tablet screen.
        [HttpGet("cutting-job/{jobNo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCuttingJobDetails(string jobNo)
        {
            if (string.IsNullOrWhiteSpace(jobNo))
            {
                return BadRequest(new { Message = "A cutting job number is required." });
            }

            var job = await _context.CuttingJobs
                .AsNoTracking()
                .Where(j => j.CuttingJobNo == jobNo)
                .Select(j => new
                {
                    CuttingJobNo = j.CuttingJobNo,
                    TargetLine = j.TargetLine,
                    Status = j.Status,
                    AssignedRolls = j.Rolls
                        .OrderBy(r => r.RollID)
                        .Select(r => new { r.RollID, r.Status })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (job is null)
            {
                _logger.LogInformation("Cutting job {JobNo} was requested but not found", jobNo);
                return NotFound(new { Message = $"Cutting job '{jobNo}' was not found." });
            }

            // Authorization: an operator may only see jobs routed to their assigned line.
            // The decision uses the job's TargetLine from the database, never a caller-supplied value.
            if (!string.Equals(OperatorAssignedLine, job.TargetLine, StringComparison.Ordinal))
            {
                _logger.LogWarning(
                    "RBAC denial: operator {OperatorId} (line {AssignedLine}) requested job {JobNo} routed to {TargetLine}",
                    OperatorId, OperatorAssignedLine ?? "none", jobNo, job.TargetLine);
                return Forbid();
            }

            return Ok(job);
        }

        // The endpoint that saves the telemetry.
        [HttpPost("spreader-telemetry")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SubmitTelemetry(
            [FromBody] SpreaderRequestDTO payload,
            [FromQuery] string lineID)

        {
            if (string.IsNullOrWhiteSpace(lineID))
            {
                return BadRequest(new { Message = "The 'lineID' query parameter is required." });
            }

            // 1. Load the cutting job (and its rolls). The job is the source of truth for
            //    which line the work belongs to.
            var job = await _context.CuttingJobs
                .Include(j => j.Rolls)
                .FirstOrDefaultAsync(j => j.CuttingJobNo == payload.CuttingJobNo);

            if (job is null)
            {
                return NotFound(new { Message = $"Cutting job '{payload.CuttingJobNo}' was not found." });
            }

            // 2. Authorization (RBAC). The decision compares the operator's AssignedLine
            //    claim against the job's TargetLine from the database - never the
            //    caller-supplied 'lineID'. A caller cannot widen their own access by
            //    changing a query parameter.
            if (!string.Equals(OperatorAssignedLine, job.TargetLine, StringComparison.Ordinal))
            {
                _logger.LogWarning(
                    "RBAC denial: operator {OperatorId} (line {AssignedLine}) attempted to submit for job {JobNo} routed to {TargetLine}",
                    OperatorId, OperatorAssignedLine ?? "none", job.CuttingJobNo, job.TargetLine);
                return Forbid();
            }

            // 3. Consistency check on the client-supplied line. Now that authorization has
            //    passed, a mismatch here means the tablet's context is stale/incorrect,
            //    which is a client error rather than an authorization failure.
            if (!string.Equals(job.TargetLine, lineID, StringComparison.Ordinal))
            {
                _logger.LogWarning(
                    "Line mismatch: job {JobNo} targets {TargetLine} but was submitted with lineID {LineID}",
                    job.CuttingJobNo, job.TargetLine, lineID);
                return BadRequest(new { Message = $"Cutting job '{payload.CuttingJobNo}' is routed to line '{job.TargetLine}', not '{lineID}'." });
            }

            // 4. The roll must be one that belongs to this job.
            var roll = job.Rolls.FirstOrDefault(r => r.RollID == payload.RollID);
            if (roll is null)
            {
                return BadRequest(new { Message = $"Roll '{payload.RollID}' is not assigned to cutting job '{payload.CuttingJobNo}'." });
            }

            // 5. Business Logic Validation (Duplicate Roll Check)
            if (await RollAlreadyRecorded(payload))
            {
                _logger.LogInformation(
                    "Duplicate telemetry rejected: roll {RollID} already recorded for job {JobNo} by operator {OperatorId}",
                    payload.RollID, payload.CuttingJobNo, OperatorId);

                // Return 409 Conflict if the roll was already entered for this job
                return Conflict(new { Message = $"Duplicate Entry: Roll '{payload.RollID}' has already been processed for Job '{payload.CuttingJobNo}'." });
            }

            // 6. Payload Sanitization & Execution
            var telemetryRecord = new SpreaderTelemetry
            {
                CuttingJobNo = payload.CuttingJobNo,
                RollID = payload.RollID,
                ActualPlyHeight = payload.ActualPlyHeight,
                EndBit = payload.EndBit,
                OverlapStart = payload.OverlapStart,
                OverlapEnd = payload.OverlapEnd,
                Damage = payload.Damage,
                OperatorId = OperatorId,
                Timestamp = DateTime.UtcNow
            };

            _context.SpreaderTelemetryRecords.Add(telemetryRecord);
            roll.Status = "Processed";

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // A concurrent request may have inserted the same roll between the check
                // above and this save; the unique index would then reject it. If that is
                // what happened, surface it as a normal 409 rather than a 500.
                if (await RollAlreadyRecorded(payload))
                {
                    _logger.LogInformation(
                        ex,
                        "Concurrent duplicate telemetry rejected by unique index: roll {RollID}, job {JobNo}",
                        payload.RollID, payload.CuttingJobNo);
                    return Conflict(new { Message = $"Duplicate Entry: Roll '{payload.RollID}' has already been processed for Job '{payload.CuttingJobNo}'." });
                }

                throw;
            }

            _logger.LogInformation(
                "Telemetry stored: record {RecordId}, roll {RollID}, job {JobNo}, line {LineID}, operator {OperatorId}",
                telemetryRecord.Id, payload.RollID, payload.CuttingJobNo, lineID, OperatorId);

            return Ok(new { Message = $"Telemetry for Roll {payload.RollID} on Job {payload.CuttingJobNo} securely saved." });
        }

        private Task<bool> RollAlreadyRecorded(SpreaderRequestDTO payload) =>
            _context.SpreaderTelemetryRecords
                .AsNoTracking()
                .AnyAsync(r => r.CuttingJobNo == payload.CuttingJobNo && r.RollID == payload.RollID);
    }
}
