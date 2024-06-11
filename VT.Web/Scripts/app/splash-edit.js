(function () {

    'use strict';

    //Save Merchant info
    var saveMerchantForm = $("#updateMerchantForm").validate({
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
            EntityEIN: {
                required: false,
                number: true,
                minlength: 9,
                maxlength: 9
            },
            MemberDriverLicenseState: {
                required: true
            },
            MemberEmail: {
                required: true,
                email: true
            },
            MemberFirstName: {
                required: true
            },
            MemberLastName: {
                required: true
            },
            MemberOwnerShip: {
                required: true,
                number: true,
                minlength: 1,
                maxlength: 100
            },
            MemberSocialSecurityNumber: {
                required: true
            },
            EntityName: {
                required: true
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
            EntityCountry: {
                required: true
            },
            EntityState: {
                required: true
            },
            EntityEmail: {
                required: true,
                email: true
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
                required: 'Entity website is required.',
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
            var buttonText = $("#btnUpdateMerchant").html();
            $("#btnUpdateMerchant").attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

            $(form).ajaxSubmit({
                success: function (data) {
                    if (data.success) {
                        VT.Util.Notification(true, data.message);
                        setTimeout(function () {
                            window.location.reload();
                        }, 3000);
                    } else {
                        VT.Util.HandleLogout(data.message);
                        VT.Util.Notification(false, data.message);
                    }
                    $("#btnUpdateMerchant").attr('disabled', null).html(buttonText);
                },
                error: function (xhr, status, error) {
                    $("#btnUpdateMerchant").attr('disabled', null).html(buttonText);
                }
            });
            return false;
        }
    });
}).apply(this, [jQuery]);