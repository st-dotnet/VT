using System;
using System.Collections.ObjectModel;
using System.Linq;
using VT.Common;
using VT.Common.Utils;
using VT.Data.Context;
using VT.Data.Entities;
using VT.QuickBooks.DTOs.Customers;
using VT.QuickBooks.Interfaces;
using VT.Services.DTOs;
using VT.Services.Interfaces;

namespace VT.Services.Services
{
    public class WebhookService : IWebhookService
    {
        #region Field(s)

        private readonly IVerifyTechContext _context;
        private readonly ICustomer _qbCustomerServices;

        #endregion

        #region Constructor

        public WebhookService(IVerifyTechContext context, ICustomer qbCustomerServices)
        {
            _context = context;
            _qbCustomerServices = qbCustomerServices;
        }

        #endregion

        #region Public Methods

        public BaseResponse WebhooksOperations(WebhookNotificationRequest request)
        {
            var response = new BaseResponse();
            var @event = request.EventNotifications.FirstOrDefault();

            var qbsettings = _context.QuickbookSettings.FirstOrDefault(x => x.RealmId == @event.RealmId);
            var authorizationTokenHeader = (qbsettings.ClientId + ":" + qbsettings.ClientSecret).ToBase64Encode();
            var qbCustomers = _qbCustomerServices.GetAllCustomers(authorizationTokenHeader);
            var dbCustomers = _context.Customers.ToList();

            try
            {
                foreach (var eventNotification in request.EventNotifications)
                {
                    foreach (var entities in eventNotification.DataEvents.Entities.GroupBy(x => x.Name))
                    {
                        var customerEntities = entities.Where(x => x.Name == "Customer").ToList();

                        if (customerEntities != null)
                        {
                            var model = new WebhookRequest
                            {
                                Customers = customerEntities,
                                DbCustomers = dbCustomers,
                                QbSettings = qbsettings,
                                QbCustomers = qbCustomers
                            };
                            response = EntityOperations(model);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        #endregion

        #region Private Methods

        private BaseResponse EntityOperations(WebhookRequest request)
        {
            var response = new BaseResponse();
            try
            {
                foreach (var customer in request.Customers)
                {
                    var @case = customer.Operation;

                    switch (@case)
                    {
                        case "Update":
                            response = UpdateCustomer(customer, request);
                            break;
                        case "Delete":
                            response = DeleteCustomer(customer, request);
                            break;
                        case "Create":
                            response = CreateCustomer(customer, request);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        private BaseResponse UpdateCustomer(EntitiesRequest requestCustomer, WebhookRequest request)
        {
            var response = new BaseResponse();
            try
            {
                var dbCustomer = request.DbCustomers.FirstOrDefault(x => x.QuickbookCustomerId == requestCustomer.Id);
                if (dbCustomer != null)
                {
                    var qbCustomerDetails = request.QbCustomers.QueryResponse.Customer.FirstOrDefault(x => x.Id == requestCustomer.Id);
                    var defaultAddress = dbCustomer.Addresses.FirstOrDefault();
                    var defaultContactInfo = dbCustomer.ContactPersons.FirstOrDefault();

                    // update address realted information
                    dbCustomer.Name = qbCustomerDetails.GivenName + " " + qbCustomerDetails.FamilyName;
                    defaultAddress.City = qbCustomerDetails.BillAddr.City;
                    defaultAddress.StreetAddress = qbCustomerDetails.BillAddr.Line1;
                    defaultAddress.PostalCode = qbCustomerDetails.BillAddr.PostalCode;
                    defaultAddress.Territory = qbCustomerDetails.BillAddr.CountrySubDivisionCode;

                    // update contact information
                    defaultContactInfo.FirstName = qbCustomerDetails.GivenName;
                    defaultContactInfo.LastName = qbCustomerDetails.FamilyName;
                    defaultContactInfo.Email = qbCustomerDetails.PrimaryEmailAddr.Address;
                    defaultContactInfo.Mobile = qbCustomerDetails.Mobile.FreeFormNumber;
                    defaultContactInfo.Telephone = qbCustomerDetails.Mobile.FreeFormNumber.ToString();

                    _context.SaveChanges();
                    response.Success = true;
                    response.Message = "Customer has been updated successfully.";
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        private BaseResponse CreateCustomer(EntitiesRequest requestCustomer, WebhookRequest request)
        {
            var response = new BaseResponse();
            try
            {
                var newQbCustomer = request.QbCustomers.QueryResponse.Customer.FirstOrDefault(x => x.Id == requestCustomer.Id);
                var customer = new Data.Entities.Customer
                {
                    Name = newQbCustomer.GivenName + " " + newQbCustomer.FamilyName,
                    CompanyId = request.QbSettings.CompanyId,
                    QuickbookCustomerId = requestCustomer.Id
                };
                AddAddress(customer, newQbCustomer);
                AddContactPerson(customer, newQbCustomer);
                _context.Customers.Add(customer);
                _context.SaveChanges();

                response.Success = true;
                response.Message = "Customer has been created successfully";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        private BaseResponse DeleteCustomer(EntitiesRequest requestCustomer, WebhookRequest request)
        {
            var response = new BaseResponse();
            try
            {

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        private void AddAddress(Data.Entities.Customer customer, CustomerResponse request)
        {
            customer.Addresses = new Collection<Address>
            {
                new Address
                {
                    StreetAddress = request.BillAddr.Line1,
                    City = request.BillAddr.City,
                    Territory = request.BillAddr.CountrySubDivisionCode,
                    Country = request.BillAddr.Country,
                    PostalCode = request.BillAddr.PostalCode
                }
            };
        }

        private void AddContactPerson(Data.Entities.Customer customer, CustomerResponse request)
        {
            customer.ContactPersons = new Collection<ContactPerson>
            {
                new ContactPerson
                {
                    Email = request.PrimaryEmailAddr.Address,
                    FirstName = request.GivenName,
                    LastName = request.FamilyName,
                    Telephone = request.Mobile.FreeFormNumber,
                    Mobile = request.Mobile.FreeFormNumber,
                    MiddleName = request.MiddleName,
                    ContactType = ContactTypes.Office.ToString() //TODO : 
                }
            };
        }

        #endregion
    }
}
