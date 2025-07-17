using System.Text.Json.Serialization;

namespace AssetManagement.Models
{
    public class EmployeeSoftwareAsset
    {
        public int EmployeeId { get; set; }
        public int SoftwareID { get; set; }
        public string EmployeeName { get; set; }
        public string SoftwareName { get; set; }
        public DateTime AssignedDate { get; set; }
        public int CompanyID { get; set; }

        [JsonIgnore]
        public virtual Employee Employee { get; set; }

        [JsonIgnore]
        public virtual SoftwareAsset SoftwareAsset { get; set; }
        [JsonIgnore]
        public virtual Company? Company { get; set; }

    }
}
