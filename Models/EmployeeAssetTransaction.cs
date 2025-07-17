using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AssetManagement.Models
{
    [Table("tb_EmployeeAssetTransactions")]
    public class EmployeeAssetTransaction
    {
        [Key]
        [JsonIgnore]
        public int TransactionID { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int AssetID { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PurchasePrice { get; set; }

        public DateTime? TransactionDate { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        //  New CompanyID column
        public int CompanyID { get; set; }

        // Navigation Properties
        [JsonIgnore]
        public virtual Employee Employee { get; set; }

        [JsonIgnore]
        public virtual PhysicalAsset Asset { get; set; }

        //  Optional navigation property for Company
        [JsonIgnore]
        public virtual Company? Company { get; set; }
    }
}
