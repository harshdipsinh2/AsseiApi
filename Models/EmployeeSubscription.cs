using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AssetManagement.Models
{
    [Table("tb_EmployeeSubscriptions")]
    public class EmployeeSubscription
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [ForeignKey("User")]
        public string Email { get; set; }  // This replaces EmployeeId

        [Required]
        [StringLength(255)]
        public string SubscriptionId { get; set; }

        [Required]
        [StringLength(255)]
        public string ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int CompanyID { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }  // Navigation property to User model

        [JsonIgnore]
        public virtual Company? Company { get; set; }
    }
}