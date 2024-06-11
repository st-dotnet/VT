(function () {

    'use strict';

    //Save Customer
    var saveCompanyCreditCard = $("#saveCompanyCreditCard").validate({
        rules: {
            CustomerFirstName: {
                required: true
            },
            CustomerLastName: {
                required: true
            },
            CustomerEmail: {
                required: true
            },
            PaymentNumber: {
                required: true
            },
            PaymentCvv: {
                required: true
            },
            PaymentMethod: {
                required: true
            },
            Month: {
                required: true
            },
            Year: {
                required: true
            }
        },
        highlight: function (label) {
            $(label).closest('.form-group').removeClass('has-success').addClass('has-error');
        },
        success: function (label) {
            $(label).closest('.form-group').removeClass('has-error');
            label.remove();
        },
        errorPlacement: function (error, element) {
            var placement = element.closest('.input-group');
            if (!placement.get(0)) {
                placement = element;
            }
            if (error.text() !== '') {
                placement.after(error);
            }
        },
        submitHandler: function (form) {
            var buttonText = $("#btnSaveCompanyCreditCard").html();
            $("#btnSaveCompanyCreditCard").attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

            $(form).ajaxSubmit({
                success: function (data) {
                    if (data && data.success) {
                        VT.Util.Notification(true, "Customer has been successfully saved.");
                        setTimeout(function () {
                            window.location.href = "/";
                        }, 3000);
                    } else {
                        VT.Util.HandleLogout(data.message);
                        VT.Util.Notification(false, data.message);
                    }

                    $("#btnSaveCompanyCreditCard").attr('disabled', null).html(buttonText);
                },
                error: function (xhr, status, error) {
                    $("#btnSaveCompanyCreditCard").attr('disabled', null).html(buttonText);
                }
            });
            return false;
        }
    });
}).apply(this, [jQuery]);