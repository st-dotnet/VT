(function () {

    'use strict';

    //Save Merchant
    var saveMerchantForm = $("#saveMerchantForm").validate({
        rules: {
            AnnualCCSales: {
                required: true
            },
            DBA: {
                required: true
            },
            Established: {
                required: true
            },
            MerchantCategoryCode: {
                required: true,
            },
            MemberTitle: {
                required: true
            },
            MemberDateOfBirth: {
                required: true
            },
            MemberDriverLicense: {
                required: true
            },
            MemberDriverLicenseState: {
                required: true
            },
            MemberEmail: {
                required: true,
                email: true
            },
            EstimatedSales: {
                required: true
            },
            MemberFirstName: {
                required: true
            },
            MemberLastName: {
                required: true
            },
            EntityEIN: {
                required: false,
                number: true,
                minlength: 9,
                maxlength: 9
            },
            MemberOwnerShip: {
                required: true,
            },
            MemberSocialSecurityNumber: {
                required: true
            },
            EntityName: {
                required: true
            },
            EntityEmail: {
                required: true,
                email: true
            },
            EntityAddress1: {
                required: true
            },
            EntityPhone: {
                required: true
            },
            EntityCity: {
                required: true
            },
            EntityState: {
                required: true
            },
            EntityType: {
                required: true
            },
            EntityWebsite: {
                required: true,
                url: true
            },
            EntityZip: {
                required: true
            },
            CardOrAccountNumber: {
                required: true,
            },
            AccountsPaymentMethod: {
                required: true,
            },
            AccountsRoutingCode: {
                required: true,
            }
        },
        messages: {
            EntityWebsite: {
                required: 'Website is required.',
                url: "Please enter a valid URL that starts with 'http://' or 'https://'"
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
            var buttonText = $("#btnAddMerchant").html();
            $("#btnAddMerchant").attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

            $(form).ajaxSubmit({
                success: function (data) {
                    if (data && data.success) {
                        VT.Util.Notification(true, "Merchant has been successfully saved.");
                        setTimeout(function () {
                            //window.location.reload();
                            window.location.href = "/";
                        }, 3000);
                    } else {
                        VT.Util.HandleLogout(data.message);
                        VT.Util.Notification(false, data.message);
                    }

                    $("#btnAddMerchant").attr('disabled', null).html(buttonText);
                    //$(form).resetForm();
                    //$("#CompanyId").val('');
                    //$("#CompanyId").val(0);
                },
                error: function (xhr, status, error) {
                    $("#btnAddMerchant").attr('disabled', null).html(buttonText);
                }
            });
            return false;
        }
    });

}).apply(this, [jQuery]);