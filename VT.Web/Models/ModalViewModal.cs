namespace VT.Web.Models
{
    public class ModalViewModal
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Id { get; set; }
        public bool IsAlertModel { get; set; }
        public string QBModalPopupEntityId { get; set; }
        public string SubmitButtonId { get; set; }
        public string BtnId { get; set; } //optional
        public string HiddenElementId { get; set; } //optional
        public string NoButtonTitle { get; set; }
        public string YesButtonTitle { get; set; }
        public bool ShowHeader { get; set; }

        public ModalViewModal()
        {
            ShowHeader = true;
        }
    }

    public class EmployeeViewModal
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Id { get; set; }
        public bool IsAlertModel { get; set; }
        public string QBModalPopupEntityId { get; set; }
        public string SubmitButtonId { get; set; }
        public string BtnId { get; set; } //optional
        public string HiddenElementId { get; set; } //optional
        public string NoButtonTitle { get; set; }
        public string YesButtonTitle { get; set; }
        public string QBEmpId { get; set; }
        public bool ShowHeader { get; set; }

        public EmployeeViewModal()
        {
            ShowHeader = true;
        }
    }

    public class ServiceViewModal
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Id { get; set; }
        public bool IsAlertModel { get; set; }
        public string QBModalPopupEntityId { get; set; }
        public string SubmitButtonId { get; set; }
        public string BtnId { get; set; } //optional
        public string HiddenElementId { get; set; } //optional
        public string NoButtonTitle { get; set; }
        public string YesButtonTitle { get; set; }
        public string QBServId { get; set; }
        public bool ShowHeader { get; set; }

        public ServiceViewModal()
        {
            ShowHeader = true;
        }
    }
}