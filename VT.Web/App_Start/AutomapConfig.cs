using AutoMapper;
using VT.QuickBooks.DTOs;
using VT.QuickBooks.DTOs.Customers;
using VT.Services.DTOs;
using VT.Services.DTOs.QBEntitiesRequestResponse;
using VT.Services.DTOs.SplashPayments;
using VT.Web.Models;
using VT.Web.Models.QBCustomers;
using Webhooks.Models.DTO;

namespace VT.Web
{
    public class AutomapConfig
    {
        public static void Setup()
        {
            Mapper.CreateMap<OrganizationDetailResponse, SaveOrganizationViewModel>();
            Mapper.CreateMap<SaveOrganizationViewModel, CompanySaveRequest>();
            Mapper.CreateMap<PasswordResetModel,ResetPasswordRequest>();
            Mapper.CreateMap<CompanyWorkerResponse, SaveUserModel>();
            Mapper.CreateMap<CompanyWorkerResponse,UserDetailViewModel>();
            Mapper.CreateMap<MerchantAccountViewModel, OrganizationAccountRequest>();
            Mapper.CreateMap<GatewayCustomerViewModel, GatewayCustomerRequest>();
            Mapper.CreateMap<ChargeCustomerViewModel, ChargeCustomerRequest>();
            Mapper.CreateMap<SetServiceRecordItemCostModel, SetServiceRecordItemRequest>();
            Mapper.CreateMap<ChargeCustomerAccountViewModel, ChargeCustomerCcRequest>();
            Mapper.CreateMap<CustomerDetailResponse, CustomerDetailViewModel>();
            Mapper.CreateMap<GatewayAccountDetailResponse, MerchantAccountViewModel>();
            Mapper.CreateMap<ImportCustomerValidation, ImportCustomerRequest>();
            Mapper.CreateMap<SplashCustomerDetailResponse, SplashCustomerModel>();  
            Mapper.CreateMap<GetInvoiceDetailItem, InvoicesViewModel>();
            Mapper.CreateMap<GetVoidInvoiceDetailItem, VoidInvoicesViewModel>();
            Mapper.CreateMap<GetCommissionExpenseDetailItem, CommissionExpenseViewModel>();
            Mapper.CreateMap<SplashGetMerchantResponse, SplashMerchantModel>();
            Mapper.CreateMap<SplashMerchantModel, UpdateMerchantInfoRequest>();
            Mapper.CreateMap<ImageDetails, ImageDetailsRequest>();
            Mapper.CreateMap<CreateQBCustomerModel, QuickBooks.DTOs.Customers.CreateCustomerRequest>();
            Mapper.CreateMap<QuickbooksSettingsModel, QuickbooksSettingsRequest>();
            Mapper.CreateMap<SyncCustomerListModel, SyncCustomerRequest>();
            Mapper.CreateMap<WebhooksDataModel, WebhookNotificationRequest>();
            Mapper.CreateMap<EventNotificationsModel, EventNotificationsRequest>();
            Mapper.CreateMap<DataChangeEventsModel, DataChangeEventsRequest>();
            Mapper.CreateMap<EntitiesModel,EntitiesRequest>();
            Mapper.CreateMap<SyncEmployeeListModel, SyncEmployeeRequest>();
            Mapper.CreateMap<SyncServiceListModel, SyncServicesRequest>();
        }
    }
}