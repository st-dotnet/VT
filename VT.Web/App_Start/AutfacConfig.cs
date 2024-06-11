﻿using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using VT.Data.Context;
using VT.Services.Interfaces;
using VT.Services.Services;
using VT.Web.Components;
using VT.Web.Interfaces;
using VT.QuickBooks.Interfaces;
using VT.QuickBooks.Services;

namespace VT.Web
{
    public class AutfacConfig
    {
        public static void Setup()
        {
            //container builder init
            var builder = new ContainerBuilder();

            //register controllers
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            //register database context
            builder.RegisterType<VerifyTechContext>().As<IVerifyTechContext>();

            //register all services in VT.Services
            builder.RegisterType<CompanyWorkerService>().As<ICompanyWorkerService>();
            builder.RegisterType<CompanyService>().As<Services.Interfaces.ICompanyService>();
            builder.RegisterType<CompanyServiceService>().As<ICompanyServiceService>();
            builder.RegisterType<CustomerService>().As<ICustomerService>();
            builder.RegisterType<CustomerServiceService>().As<ICustomerServiceService>();
            builder.RegisterType<ServiceRecordService>().As<IServiceRecordService>();
            builder.RegisterType<ServiceRecordItemService>().As<IServiceRecordItemService>();
            builder.RegisterType<PaymentGatewayService>().As<IPaymentGatewayService>();
            builder.RegisterType<EmailService>().As<IEmailService>();
            builder.RegisterType<CommissionService>().As<ICommissionService>();
            builder.RegisterType<BraintreePaymentService>().As<IPaymentService>();
            builder.RegisterType<SplashPaymentService>().As<ISplashPaymentService>();
            builder.RegisterType<InvoiceService>().As<IInvoices>();
            builder.RegisterType<CustomerServices>().As<ICustomer>();
            builder.RegisterType<EmployeeServices>().As<IEmployee>();
            builder.RegisterType<QuickbookSettingsServices>().As<IQuickbookSettings>();
            builder.RegisterType<WebhookService>().As<IWebhookService>();
            builder.RegisterType<CompanyServices>().As<IQBCompanyService>();

            //register Web components
            builder.RegisterType<UserAuthenticator>().As<IUserAuthenticator>();

            builder.RegisterModule(new AutofacWebTypesModule());
            builder.RegisterFilterProvider();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}