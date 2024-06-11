namespace VT.Services.DTOs.SplashPayments
{
    public class SplashAccountResponse : BaseResponse
    {
        public string ReferenceNumber { get; set; }
        public bool InternalError { get; set; }
    } 
}
