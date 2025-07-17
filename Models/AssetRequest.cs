using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AssetManagement.Models
{
    [Table("tb_AssetRequests")]
    public class AssetRequest
    {
        [Key]
        public int RequestID { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeID { get; set; }

        [ForeignKey("Asset")]
        public int? AssetID { get; set; } // Nullable for pending requests

        [Required]
        public string AssetName { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        public DateTime RequestedDate { get; set; } = DateTime.Now;
        public DateTime? ApprovalDate { get; set; } // Nullable

        // New foreign key to reference the Company
        public int CompanyID { get; set; }

        // Navigation Properties
        [JsonIgnore]
        public virtual Employee? Employee { get; set; }

        [JsonIgnore]
        public virtual PhysicalAsset? Asset { get; set; } // Add this

        // Navigation property for Company
        [JsonIgnore]
        public virtual Company? Company { get; set; } // Add this for navigation
    }
}
