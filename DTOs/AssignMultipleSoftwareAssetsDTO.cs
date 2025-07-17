namespace AssetManagement.Models
{

    public class AssignMultipleSoftwareAssetsDTO
    {
        public int EmployeeId { get; set; }
        public List<int> SoftwareIds { get; set; }
    }

}
