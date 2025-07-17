using System.Text.Json.Serialization;

namespace AssetManagement.Models
{ 
    public class UserDTO
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public int? RoleID { get; set; }
        public int? EmployeeID { get; set; }  // Make nullable
        public string Name { get; set; }      // Employee Name
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        [JsonIgnore]
        public int CompanyID { get; set; }
    }
}
