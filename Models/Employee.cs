using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AssetManagement.Models
{
    [Table("tb_Employees")]
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; }
        public string Dept { get; set; }

        // Role as a string 
        public string Role { get; set; }

        public DateTime JoinDate { get; set; }
        public double? Salary { get; set; }
        public string? PhoneNumber { get; set; }
        public string EmailId { get; set; }

        // RoleID as a foreign key to RoleMaster table
        public int RoleID { get; set; }

        // ForeignKey to RoleMaster table
        [ForeignKey("RoleID")]
        [JsonIgnore]
        public virtual RoleMaster? RoleMaster { get; set; }

        // Add CompanyID property
        public int CompanyID { get; set; }

        // ForeignKey to Companies table
        [ForeignKey("CompanyID")]
        [JsonIgnore]
        public virtual Company? Company { get; set; }
    }

    // Email update class (unchanged)
    public class EmailUpdate
    {
        public string EmailId { get; set; }
    }
}