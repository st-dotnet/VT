﻿using Newtonsoft.Json;

namespace VT.Services.DTOs.SplashPayments
{ 
    public class SplashTransactionRequest
    {
        public decimal Amount { get; set; }
        public string MerchantId { get; set; }
        public string CustomerJson { get; set; } // customer table 
        public string Descriptor { get; set; }
        public string TxnId { get; set; }
    }

    public class SplashTransactionResult : BaseResponse
    {
        public string TransactionId { get; set; }
    }

    public class SplashTransaction 
    {
        [JsonProperty(PropertyName = "merchant")]
        public string MerchantId { get; set; }

        [JsonProperty(PropertyName = "login")]
        public string LoginId { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string BankAccount { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "mcc")]
        public string MCC { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "origin")]
        public string Origin { get; set; }

        [JsonProperty(PropertyName = "total")]
        public string Total { get; set; }

        [JsonProperty(PropertyName = "mid")]
        public string Mid { get; set; } 
    }

    public class UpdateSplashTransaction
    {
        [JsonProperty(PropertyName = "allowPartial")]
        public string AllowPartial { get; set; }

        [JsonProperty(PropertyName = "reserved")]
        public string Reserved { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "inactive")]
        public string Inactive { get; set; } 

        [JsonProperty(PropertyName = "frozen")]
        public string Frozen { get; set; } 
    }

    public class SetVoidSplashTransaction
    { 
        [JsonProperty(PropertyName = "batch")]
        public string Batch { get; set; }
    }

    public class SetRefundSplashTransaction
    {
        [JsonProperty(PropertyName = "fortxn")]
        public string ForTxn { get; set; }  

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        //[JsonProperty(PropertyName = "merchant")]
        //public string Merchant { get; set; } 
    }
}
