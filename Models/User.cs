using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AssetManagement.Models
{
    [Table("tb_Users")]
    public class User
    {
        [Key]
        [JsonIgnore]
        public int UserID { get; set; }

        // Make EmployeeID nullable and not required
        public int? EmployeeID { get; set; }

        [ForeignKey(nameof(EmployeeID))]
        [JsonIgnore]
        public Employee? Employee { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public int? RoleID { get; set; }

        [ForeignKey(nameof(RoleID))]
        [JsonIgnore]
        public RoleMaster? Role { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public int CompanyID { get; set; }

        // Optional navigation property for Company
        [JsonIgnore]
        public virtual Company? Company { get; set; }
    }

}
