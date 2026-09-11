using System.ComponentModel.DataAnnotations;

namespace ApparelMiddlewareGateway.Models
{
    /// <summary>
    /// A fabric roll assigned to a <see cref="CuttingJob"/>. Keyed by
    /// (CuttingJobNo, RollID) so the same physical roll number can recur across jobs.
    /// </summary>
    public class Roll
    {
        [Required]
        [MaxLength(64)]
        public string CuttingJobNo { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string RollID { get; set; } = string.Empty;

        /// <summary>Pending until spreader telemetry has been recorded, then Processed.</summary>
        [Required]
        [MaxLength(32)]
        public string Status { get; set; } = "Pending";

        public CuttingJob? CuttingJob { get; set; }
    }
}
