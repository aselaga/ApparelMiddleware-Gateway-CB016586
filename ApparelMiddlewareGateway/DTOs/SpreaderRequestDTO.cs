using System.ComponentModel.DataAnnotations;

namespace ApparelMiddlewareGateway.DTOs
{
    public class SpreaderRequestDTO
    {
        [Required]
        [StringLength(64, MinimumLength = 1)]
        public string CuttingJobNo { get; set; } = string.Empty; // Required for validation

        [Required]
        [StringLength(64, MinimumLength = 1)]
        public string RollID { get; set; } = string.Empty;

        [Required]
        [Range(1, 100, ErrorMessage = "Ply height must be between 1 and 100.")]
        public int ActualPlyHeight { get; set; }

        [Range(0, 9999.99, ErrorMessage = "End bit must be between 0 and 9999.99.")]
        public decimal EndBit { get; set; }

        [Range(0, 9999.99, ErrorMessage = "Overlap start must be between 0 and 9999.99.")]
        public decimal OverlapStart { get; set; }

        [Range(0, 9999.99, ErrorMessage = "Overlap end must be between 0 and 9999.99.")]
        public decimal OverlapEnd { get; set; }

        [Range(0, 9999.99, ErrorMessage = "Damage must be between 0 and 9999.99.")]
        public decimal Damage { get; set; }
    }
}
