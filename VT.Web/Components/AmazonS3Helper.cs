﻿using System;
using System.Security.Cryptography;

namespace VT.Web.Components
{
    public static class AmazonS3Helper
    {
        public static string ConstructPolicy(string bucket, DateTime expirationDate, string acl, string key)
        {
            // See about policy construction
            // http://docs.amazonwebservices.com/Amazo … ructPolicy   
            string policy = string.Format(@"{{ 'expiration': '{0}.000Z',
    'conditions': [
        {{ 'bucket': '{2}' }},
        {{ 'acl': '{1}' }},
        {{ 'success_action_status': '201' }},
        ['content-length-range', 0, 100000000],
        [ 'starts-with', '$key', '' ],
        [ 'starts-with', '$Content-Type', '' ],
        ['starts-with', '$name', ''],
        ['starts-with', '$Filename', '']
    ]
}}", expirationDate.ToString("s"), acl, bucket);
            // Encode the policy using UTF-8.
            byte[] pb = System.Text.Encoding.UTF8.GetBytes(policy);
            // Encode those UTF-8 bytes using Base64 and return.
            return Convert.ToBase64String(pb);
        }
        
        public static string CreateSignature(string unsignedPolicy, string awsSecretKey)
        {
            var sigHash = new HMACSHA1(System.Text.Encoding.UTF8.GetBytes(awsSecretKey));
            var bytPolicy = System.Text.Encoding.UTF8.GetBytes(unsignedPolicy);
            var bytSignature = sigHash.ComputeHash(bytPolicy);
            var retVal = Convert.ToBase64String(bytSignature);
            return retVal;
        }
    }
}
