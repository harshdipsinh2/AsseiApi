using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AssetManagement.Models
{
    [Table("tb_Companies")]
    public class Company
    {
        [Key]
        public int CompanyID { get; set; } // Primary Key for Companies table

        public string CompanyName { get; set; } // Company name

        // Additional fields for the company can be added here, for example:
        public string? Address { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property for employees in this company
        [JsonIgnore] // To avoid circular references when serializing
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>(); // Employees of the company
    }
} 