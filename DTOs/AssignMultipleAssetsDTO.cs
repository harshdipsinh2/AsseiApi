namespace AssetManagement.Models
{
    public class AssignMultipleAssetsDto
    {
        public int EmployeeId { get; set; }
        public List<int> AssetIds { get; set; }
        public DateTime AssignedDate { get; set; }
    }

}
