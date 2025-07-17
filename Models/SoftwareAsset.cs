using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AssetManagement.Models;

[Table("tb_SoftwareAssets")]
public class SoftwareAsset
{
    [Key]
    public int SoftwareId { get; set; } // Primary Key
    public string SoftwareName { get; set; }
    public int SubscriptionCost { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Vendor { get; set; }            
    public string LicenseType { get; set; }
    public string Apps { get; set; }
    public int CompanyID { get; set; }

    //  Optional navigation property for Company
    [JsonIgnore]
    public virtual Company? Company { get; set; }
}
