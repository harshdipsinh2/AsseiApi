using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AssetManagement.Models
{
    [Table("tb_EmployeePhysicalAssets")]
    public class EmployeePhysicalAsset
    {
        [JsonIgnore]
        public int Id { get; set; }  // Primary Key
        public int EmployeeId { get; set; }

        [Column("AssetID")] // Explicit mapping for case-sensitive databases
        public int AssetId { get; set; }
        public string EmployeeName { get; set; }
        public string AssetName { get; set; }
        public DateTime AssignedDate { get; set; }
        public int CompanyID { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Employee Employee { get; set; }
        [JsonIgnore]
        public virtual PhysicalAsset Asset { get; set; }
        [JsonIgnore]
        public virtual Company? Company { get; set; }
    }
}
