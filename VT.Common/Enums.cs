using System.ComponentModel;

namespace VT.Common
{
    public enum UserRoles
    {
        [Description("Super Admin")]
        SuperAdmin,

        [Description("Company Admin")]
        CompanyAdmin,

        [Description("Company User")]
        CompanyUser
    }

    public enum ContactTypes
    {
        Office,
        Resident,
    }

    public enum ExportStatus
    {
        Valid,
        Invalid,
    }

    public enum GatewayAccount
    {
        Merchant,
        Customer
    }
    public enum AttachmentType
    {
        Before,
        After
    }

    public enum EmailType
    {
        Invoice,
        WorkOrder
    }
    public enum EntityType
    {
        [Description("Sole Proprietor")]
        SoleProprietor = 0,

        [Description("Corporation")]
        Corporation = 1,

        [Description("Limited Liability Company")]
        LimitedLiabilityCompany = 2,

        [Description("Partnership")]
        Partnership = 3,

        [Description("Association")]
        Association = 4,

        [Description("Non-Profit Organization")]
        NonProfitOrganization = 5,

        [Description("Government Organization")]
        GovernmentOrganization = 6
    }
    public enum AccountType
    {
        [Description("Checking")]
        Checking = 8,

        [Description("Savings")]
        Savings = 9,

        [Description("Corporate Checking")]
        CorporateChecking = 10,

        [Description("Corporate Savings")]
        CorporateSavings = 11
    }
}
