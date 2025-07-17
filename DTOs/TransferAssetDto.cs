namespace AssetManagement.Models
{
    public class TransferAssetDto
    {
        public int OldEmployeeId { get; set; }
        public int NewEmployeeId { get; set; }
        public int AssetId { get; set; }
    }
}
