namespace VT.Services.DTOs
{
    public class BaseResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public bool IsMerchantChangedSuccessfully { get; set; }
        public string QBEntityId { get; set; }
    }
}
