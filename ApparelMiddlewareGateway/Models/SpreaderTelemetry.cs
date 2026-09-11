using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ApparelMiddlewareGateway.Models
{
    public class SpreaderTelemetry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string CuttingJobNo { get; set; } = string.Empty; // Links to the specific job

        [Required]
        [MaxLength(64)]
        public string RollID { get; set; } = string.Empty;

        public int ActualPlyHeight { get; set; }

        [Precision(10, 2)]
        public decimal EndBit { get; set; }

        [Precision(10, 2)]
        public decimal OverlapStart { get; set; }

        [Precision(10, 2)]
        public decimal OverlapEnd { get; set; }

        [Precision(10, 2)]
        public decimal Damage { get; set; }

        [Required]
        [MaxLength(64)]
        public string OperatorId { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
