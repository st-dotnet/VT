using System;
using System.Configuration;

namespace VT.Common
{
    public static class ApplicationSettings
    {

        public static string AdminPassword
        {
            get { return ConfigurationManager.AppSettings["AdminPassword"]; }
        }

        public static bool EnableUserImport
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["EnableUserImport"]); }
        }

        public static bool EnableCustomerImport
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["EnableCustomerImport"]); }
        }
        public static Double ServiceFeePercentage
        {
            get { return Convert.ToDouble(ConfigurationManager.AppSettings["ServiceFeePercentage"]); }
        }
        public static string FromEmail
        {
            get { return ConfigurationManager.AppSettings["FromEmail"]; }
        }

        public static string MerchantId
        {
            get { return ConfigurationManager.AppSettings["MerchantId"]; }
        }

        public static string PublicKey
        {
            get { return ConfigurationManager.AppSettings["PublicKey"]; }
        }

        public static string PrivateKey
        {
            get { return ConfigurationManager.AppSettings["PrivateKey"]; }
        }

        public static string MerchantAccountId
        {
            get { return ConfigurationManager.AppSettings["MerchantAccountId"]; }
        }

        public static string SecureBaseUrl
        {
            get { return ConfigurationManager.AppSettings["SecureBaseUrl"]; }
        }

        public static string AwsS3Url
        {
            get { return ConfigurationManager.AppSettings["AwsS3Url"]; }
        }

        public static string Host
        {
            get { return ConfigurationManager.AppSettings["Host"]; }
        }

        public static string Username
        {
            get { return ConfigurationManager.AppSettings["Username"]; }
        }

        public static string Password
        {
            get { return ConfigurationManager.AppSettings["Password"]; }
        }

        public static int Port
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["Port"]); }
        }

        public static bool IsSsl
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["IsSsl"]); }
        }
        public static bool IsGatewayLive
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["IsGatewayLive"]); }
        }

        public static string Descriptor
        {
            get { return ConfigurationManager.AppSettings["Descriptor"]; }
        }

        public static string SuperAdminDescriptorName
        {
            get { return ConfigurationManager.AppSettings["SuperAdminDescriptorName"]; }
        }

        public static string SuperAdminDescriptorTelephone
        {
            get { return ConfigurationManager.AppSettings["SuperAdminDescriptorTelephone"]; }
        }

        public static string SuperAdminDescriptorUrl
        {
            get { return ConfigurationManager.AppSettings["SuperAdminDescriptorUrl"]; }
        }

        #region FileImport Keys

        public static string SampleFileEmployeeImportURL
        {
            get { return ConfigurationManager.AppSettings["SampleFileEmployeeImportURL"]; }
        }

        public static string SampleFileCustomerImportURL
        {
            get { return ConfigurationManager.AppSettings["SampleFileCustomerImportURL"]; }
        }

        #endregion

        #region PhotoInfo Keys

        public static int PhotoQuality
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["PhotoQuality"]); }
        }

        public static int PhotoWidth
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["PhotoWidth"]); }
        }

        public static int PhotoHeight
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["PhotoHeight"]); }
        }

        public static bool PhotoCrop
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["PhotoCrop"]); }
        }

        #endregion

        #region Splash Keys

        public static string SplashApiKey
        {
            get { return ConfigurationManager.AppSettings["SplashApiKey"]; }
        }

        public static string SplashSuperMerchantLoginId
        {
            get { return ConfigurationManager.AppSettings["SplashSuperMerchantLoginId"]; }
        }

        public static string SplashReferrerEntityId
        {
            get { return ConfigurationManager.AppSettings["SplashReferrerEntityId"]; }
        }

        public static string SplashApiUrl
        {
            get { return ConfigurationManager.AppSettings["SplashApiUrl"]; }
        }

        public static string SplashMerchantId
        {
            get { return ConfigurationManager.AppSettings["SplashMerchantId"]; }
        }

        public static int SplashTransactionFee
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["SplashTransactionFee"]); }
        }

        #endregion

        #region Quickbooks Keys

        public static string QBBaseUrl
        {
            get { return ConfigurationManager.AppSettings["qboBaseUrl"]; }
        }

        public static string QBClientKey
        {
            get { return ConfigurationManager.AppSettings["clientId"]; }
        }

        public static string QBClientSecret
        {
            get { return ConfigurationManager.AppSettings["clientSecret"]; }
        }

        public static string QBRealmId
        {
            get { return ConfigurationManager.AppSettings["realmId"]; }
        }

        public static string AccessToken
        {
            get { return ConfigurationManager.AppSettings["accessToken"]; }
        }

        public static string GetAccessToken { get; set; }

        public static string AccessTokenSecret
        {
            get { return ConfigurationManager.AppSettings["accessTokenSecret"]; }
        }

        public static string QBTokenAccessURI
        {
            get { return ConfigurationManager.AppSettings["tokenAccessURI"]; }
        }

        public static string QBTokenContentType
        {
            get { return ConfigurationManager.AppSettings["tokenContentType"]; }
        }

        public static string QBRefreshToken
        {
            get { return ConfigurationManager.AppSettings["refreshToken"]; }
        }

        #endregion

        #region  MailGun Keys

        public static string BaseUrlMailGun
        {
            get { return ConfigurationManager.AppSettings["BaseUrl"]; }
        }

        public static string ApiKeyMailGun
        {
            get { return ConfigurationManager.AppSettings["ApiKey"]; }
        }

        public static string DomainMailGun
        {
            get { return ConfigurationManager.AppSettings["Domain"]; }
        }

        public static string ResourceMailGun
        {
            get { return ConfigurationManager.AppSettings["Resource"]; }
        }

        public static string EmailFromMailGun
        {
            get { return ConfigurationManager.AppSettings["EmailFrom"]; }
        }

        public static string DefaultMCC
        {
            get
            {
                return ConfigurationManager.AppSettings["SplashDefaultMCC"];
            }
        }
        #endregion
    }
}