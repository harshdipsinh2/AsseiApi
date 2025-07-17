using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Models
{
    [Table("tb_RoleMaster")]
    public class RoleMaster
    {

        [Key]
        public int RoleID { get; set; } // Primary Key
        public string RoleName { get; set; }   
        public string RoleDescription { get; set; }
    }
}
