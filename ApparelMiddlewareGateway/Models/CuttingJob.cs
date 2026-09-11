using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApparelMiddlewareGateway.Models
{
    /// <summary>
    /// A cutting job as it would be pushed from the ERP. Owns the set of fabric rolls
    /// that the operator is expected to spread on <see cref="TargetLine"/>.
    /// </summary>
    public class CuttingJob
    {
        [Key]
        [MaxLength(64)]
        public string CuttingJobNo { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string TargetLine { get; set; } = string.Empty;

        [Required]
        [MaxLength(32)]
        public string Status { get; set; } = "Active";

        public ICollection<Roll> Rolls { get; set; } = new List<Roll>();
    }
}
