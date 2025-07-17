using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AssetManagement.Models;

[Table("tb_Assets")]
public class PhysicalAsset
{
    [Key]
    public int AssetID { get; set; } // Primary Key
    public string AssetName { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public int PurchaseCost { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string Department { get; set; }
    public string Location { get; set; }
    public string Status { get; set; }
    public string Condition { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    public int Quantity { get; set; }
    public int CompanyID { get; set; }

    // Navigation property for the company
    [JsonIgnore]
    public virtual Company? Company { get; set; }
}