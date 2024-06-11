(function () {

    'use strict';

    var sysEmpArray = [];
    var qbEmpArray = [];

    var elem1 = document.querySelector('.js-switch1');
    var isIntegrateQuikcbooks = new Switchery(elem1);

    var elem2 = document.querySelector('.js-switch2');
    var servicesSettings = new Switchery(elem2);

    var elem3 = document.querySelector('.js-switch3');
    var customersSettings = new Switchery(elem3);

    var elem4 = document.querySelector('.js-switch4');
    var employeesSettings = new Switchery(elem4);

    var elem5 = document.querySelector('.js-switch5');
    var IsCopyWFInvoicesToQBSwitch = new Switchery(elem5);

    $(document).on('change', '.js-switch1', function () {
        var isIntegrate = $("#IsCustomersIntegrated").is(":checked");
        if (isIntegrate) {
            $(".quikcbook-settings-div").show();
        }
        else {
            $(".quikcbook-settings-div").hide();
        }
    });

    $(document).on('change', '.js-switch3', function (ev) {
        //console.log(ev);
        //console.log(ev.currentTarget.checked);
        var isSettingsOn = $(this).data('issettingson');
        var ischecked = $(this).data('switchery');

        if (isSettingsOn == "True") {
            if (ischecked == false) {
                $(this).attr('switchery', true);
                $(this).data('switchery', true);

                $.ajax({
                    url: '/Customers/GetModal',
                    type: "POST",
                    success: function (result) {
                        $("#mainDiv").html(result);
                        $("#modal-sync-cutomers-form").modal({
                            backdrop: 'static',
                            keyboard: false,
                            show: true
                        });
                        $("#mainDiv").html('<b>Please wait quickbooks list is being prepared...</b> <br/><br/><br/><br/><i class="fa fa-spinner fa-spin fa-lg" style="font-size:100px;"></i>');
                        // fetch data 
                        $.ajax({
                            url: '/Customers/GetSynchronizationListData',
                            type: "POST",
                            success: function (result) {
                                $("#mainDiv").html(result);
                            },
                            error: function (xhr, status, error) {
                                VT.Util.Notification(false, error);
                            }
                        });
                    },
                    error: function (xhr, status, error) {
                        VT.Util.Notification(false, error);
                    }
                });
            }
            else {
                $(this).attr('switchery', false);
                $(this).data('switchery', false);
            }
        }
        else {
            if (ischecked == true) {
                $(this).attr('switchery', false);
                $(this).data('switchery', false);

                $.ajax({
                    url: '/Customers/GetModal',
                    type: "POST",
                    success: function (result) {
                        $("#mainDiv").html(result);
                        $("#modal-sync-cutomers-form").modal({
                            backdrop: 'static',
                            keyboard: false,
                            show: true
                        });

                        $("#mainDiv").html('<b>Please wait quickbooks list is being prepared...</b> <br/><br/><br/><br/><i class="fa fa-spinner fa-spin fa-lg" style="font-size:100px;"></i>');
                        // fetch data 
                        $.ajax({
                            url: '/Customers/GetSynchronizationListData',
                            type: "POST",
                            success: function (result) {
                                $("#mainDiv").html(result);
                            },
                            error: function (xhr, status, error) {
                                VT.Util.Notification(false, error);
                            }
                        });
                    },
                    error: function (xhr, status, error) {
                        VT.Util.Notification(false, error);
                    }
                });
            }
            else {
                $(this).attr('switchery', true);
                $(this).data('switchery', true);
            }
        }
    });

    $(document).on("click", "#save-update-list", function () {

        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

        $.ajax({
            url: '/Customers/SaveUpdatedList',
            type: "POST",
            success: function (result) {
                if (result.success) {
                    VT.Util.Notification(true, result.message);
                    $("#modal-sync-cutomers-form").modal("hide");
                    setTimeout(function () {
                        window.location.href = "/Customers";
                    }, 3000);
                }
                else {
                    VT.Util.Notification(false, result.message);
                }
                $("#save-update-list").attr('disabled', null).html(buttonText);
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
                $("#save-update-list").attr('disabled', null).html(buttonText);
            }
        });
    });

    $(document).on("click", ".unlinkCustomer", function () {
        var customerId = $(this).data("unlinkcustomerid");
        var qbCustomerId = $(this).data("unlinkqbcustomerid");

        $("#hdnunlinkCustomerId").val(customerId);
        $("#QBModalPopupEntityId").val(qbCustomerId);

        $("#modal-unlink-customer-form").modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });
    });
    // basic
    var saveLinkedCustomer = $("#saveEditCustomerPopup").validate({
        rules: {
            SCName: { required: true },
            SCAddress: { required: true },
            SState: { required: true },
            SCPostalCode: { required: true },
            SCPhone: { required: true },
            SCEmail: { required: true, email: true },
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
            var buttonText = $("#btneditSaveCustomer").html();
            $("#btneditSaveCustomer").attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

            $(form).ajaxSubmit({
                success: function (data) {
                    $("#btneditSaveCustomer").attr('disabled', null).html('Submit');
                    $("#modal-edit-customer-form").modal("hide");
                    $("#mainDiv").html(data);
                    $("#linkedCustomerPanel").show();
                },
                error: function (xhr, status, error) {
                    $("#btneditSaveCustomer").attr('disabled', null).html(buttonText);
                }
            });
            return false;
        }
    });

    // edit linked customer
    $(document).on("click", ".editLinkedCustomer", function () {

        var customerId = $(this).data("editcustomerid");
        var qbId = $(this).data("editqbcustomerid");

        $.ajax({
            url: '/Customers/EditLinkedCustomer',
            type: "POST",
            data: {
                CustomerId: customerId,
                QBCustomerId: qbId
            },
            success: function (result) {
                if (result.success) {

                    $("#CompanyId").val(result.customer.CompanyId);
                    $("#SCCustomerId").val(result.customer.SCCustomerId);
                    $("#QbCustomerId").val(result.customer.QbCustomerId);

                    $("#SCName").val(result.customer.SCName);
                    $("#SCAddress").val(result.customer.SCAddress);
                    $("#SState").val(result.customer.SState);
                    $("#SCPostalCode").val(result.customer.SCPostalCode);
                    $("#SCPhone").val(result.customer.SCPhone);
                    $("#SCEmail").val(result.customer.SCEmail);
                    $("#matchStatus").text(result.customer.IsMatch ? "Set Customer Data" : "Set Customer Data");
                    $("#tblSysstemCustomer tbody").append(result.systemcustmer);
                    $("#tblQBCustomer tbody").append(result.qbCustomer);

                    if (result.customer.IsMatch) {
                        $("#tblRow1").addClass("bg-success");
                        $("#tblC1").addClass("bg-success");
                        $("#tblC2").addClass("bg-success");
                    }
                    else {
                        $("#tblRow1").addClass("bg-danger");
                        $("#tblC1").addClass("bg-danger");
                        $("#tblC2").addClass("bg-danger");
                    }
                    $("#modal-edit-customer-form").modal({
                        backdrop: 'static',
                        keyboard: false,
                        show: true
                    });
                }
                else {
                    VT.Util.HandleLogout(result.message);
                    VT.Util.Notification(false, result.message);
                }
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
            }
        });
        return false;
    });

    $(document).on("click", ".panel-collapse", function () {

        var panelId = $(this).data("panelid");
    });

    // save settings
    $("#btnunlinkCustomer").click(function () {
        debugger;

        var customerId = $("#hdnunlinkCustomerId").val();
        var qbCustomerId = $("#QBModalPopupEntityId").val();

        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

        $.ajax({
            url: '/Customers/UnlinkCustomer',
            data: {
                EntityId: customerId,
                QBEntityId: qbCustomerId
            },
            type: "POST",
            success: function (result) {

                $("#btnunlinkCustomer").attr('disabled', null).html(buttonText);
                $("#modal-unlink-customer-form").modal("hide");
                $("#mainDiv").html(result);
                $("#linkedCustomerPanel").show();
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
                $("#btnunlinkCustomer").attr('disabled', null).html(buttonText);
            }
        });
    });

    // save unlinked customer
    $("#savelinkedCustomer").click(function () {

        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

        $.aja({
            url: '/Customers/SaveUnlinkedCustomer',
            type: "POST",
            succes: function (result) {
                if (result.success) {
                    VT.Util.HandleLogout(result.message);
                    VT.Util.Notification(true, result.message);
                } $("#savelinkedCustomer").attr('disabled', null).html(buttonText);
            },
            err: function (xhr, status, error) {
                VT.Util.Notification(false, error);
                $("#btnunlinkCustomer").attr('disabled', null).html(buttonText);
            }
        });
    });

    // save settings
    $("#saveqbsettings").click(function () {

        var customersSettings = $("#CustomersSettings").is(":checked");
        var servicesSettings = $("#ServicesSettings").is(":checked");
        var employeesSettings = $("#EmployeesSettings").is(":checked");
        var isCopyWFInvoicesToQB = $("#IsCopyWFInvoicesToQB").is(":checked");

        var clientId = $("#ClientId").val();
        var clientSecret = $("#ClientSecret").val();
        var invoicePrefix = $("#InvoicePrefix").val();
        var defaultPassword = $("#DefaultPassword").val();
        var qbSettingsId = $("#QbSettingsId").val();

        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

        $.ajax({
            url: '/Customers/SaveSettings/',
            type: "POST",
            data: {
                CustomersSettings: customersSettings,
                ServicesSettings: servicesSettings,
                EmployeesSettings: employeesSettings,
                IsCopyWFInvoicesToQB: isCopyWFInvoicesToQB,
                ClientId: clientId,
                ClientSecret: clientSecret,
                InvoicePrefix: invoicePrefix,
                DefaultPassword: defaultPassword,
                QbSettingsId: qbSettingsId
            },
            success: function (result) {
                if (result.success) {
                    $("#modal-quikckbooks-settings-form").modal("hide");
                    setTimeout(function () {
                        window.location.reload();
                    }, 2000);
                    VT.Util.Notification(true, result.message);
                }
                else {
                    VT.Util.HandleLogout(result.message);
                    VT.Util.Notification(false, result.message);
                }
                $("#saveqbsettings").attr('disabled', null).html(buttonText);
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
                $("#btnSaveQbSettings").attr('disabled', null).html(buttonText);
            }
        });
    });

    var saveLinkedEmployee = $("#saveLinkedEmployeeForm").validate({
        rules: {
            GivenName: { required: true },
            FamilyName: { required: true },
            Email: { required: true, email: true },
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
            var buttonText = $("#btnSaveLinkedEmployee").html();
            $("#btnSaveLinkedEmployee").attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);
            $(form).ajaxSubmit({
                success: function (data) {

                    $("#btnSaveLinkedEmployee").attr('disabled', null).html('Submit');
                    $("#modal-edit-linkedemployee-form").modal("hide");
                    $("#mainEmployeeDiv").html(data);
                },
                error: function (xhr, status, error) {
                    $("#btnSaveLinkedEmployee").attr('disabled', null).html(buttonText);
                }
            });
            return false;
        }
    });

    // save linked employees in session
    $(document).on("click", "#btnLinkEmployees", function () {

        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

        $.ajax({
            url: '/Customers/SaveLinkedEmployees',
            type: "POST",
            data: {
                SystemEmployees: sysEmpArray,
                QbEmployees: qbEmpArray
            },
            success: function (result) {

                setTimeout(function () {
                    window.location.reload();
                }, 3000);

                $("#openSyncEmployeesModal").trigger("click");
                //$("#mainEmployeeDiv").html(result);
                //
                //$("#modal-sync-employees-form").modal({
                //    backdrop: 'static',
                //    keyboard: false,
                //    show: true
                //});
                $("#linkedemployeespanel").addClass('active');
                $("#btnLinkEmployees").attr('disabled', null).html(buttonText);
            },
            error: function (xhr, status, error) {
                $("#btnLinkEmployees").attr('disabled', null).html(buttonText);
            }
        });
    });

    $(document).on("click", ".unlinkEmployee", function () {

        var employeeId = $(this).data("unlinkemployeeid");
        var qbEmployeeId = $(this).data("unlinkqbemployeeid");

        $("#hdnunlinkEmployeeId").val(customerId);
        $("#QBModalPopupEntityId").val(qbCustomerId);

        $("#modal-unlink-customer-form").modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });
    });

    // edit linked customer
    $(document).on("click", "#sync-linked-customers", function () {
        debugger;
        $.ajax({
            url: '/Customers/GetModal/',
            type: "POST",
            success: function (result) {
                $("#mainDiv").html(result);
                $("#modal-sync-cutomers-form").modal({
                    backdrop: 'static',
                    keyboard: false,
                    show: true
                });
                $("#mainDiv").html('<b>Please wait quickbooks list is being prepared...</b> <br/><br/><br/><br/><i class="fa fa-spinner fa-spin fa-lg" style="font-size:100px;"></i>');
                // fetch data 
                $.ajax({
                    url: '/Customers/GetSynchronizationListData',
                    type: "POST",
                    success: function (result) {
                        $("#mainDiv").html(result);
                    },
                    error: function (xhr, status, error) {
                        VT.Util.Notification(false, error);
                    }
                });
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
            }
        });
    });

    // edit linked customer
    $(document).on("click", ".editLinkedEmployee", function () {

        var employeeId = $(this).data("editemployeeid");
        var qbId = $(this).data("editqbemployeeid");

        var editEmployeeId = employeeId;
        if (employeeId == "") {
            editCustomerId = qbId;
        }

        $.ajax({
            url: '/Customers/EditLinkedEmployee/' + editEmployeeId,
            type: "POST",
            success: function (result) {
                if (result.success) {
                    $("#CompanyId").val(result.employee.CompanyId);
                    $("#GivenName").val(result.employee.GivenName);
                    $("#FamilyName").val(result.employee.FamilyName);
                    $("#CompanyId").val(result.employee.CompanyId);
                    $("#EmployeeId").val(result.employee.EmployeeId);
                    $("#Email").val(result.employee.Email);
                    $("#QBEmployeeId").val(result.employee.QBEmployeeId);
                    $("#matchStatus").text(result.employee.IsMatch ? "Status : Employees Matched" : "Status : Employees Unmatched");

                    $("#modal-edit-linkedemployee-form").modal({
                        backdrop: 'static',
                        keyboard: false,
                        show: true
                    });
                }
                else {
                    VT.Util.HandleLogout(result.message);
                    VT.Util.Notification(false, result.message);
                }
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
            }
        });
    });

    function toggleChevron(e) {
        $(e.target)
        .prev('.panel-heading')
        .find("i.indicator")
        .toggleClass('glyphicon-chevron-down-custom glyphicon-chevron-up-custom');
    }
    $('#accordion').on('hidden.bs.collapse', toggleChevron);
    $('#accordion').on('shown.bs.collapse', toggleChevron);

    function remove(array, element) {

        const index = array.indexOf(element);
        if (index !== -1) {
            array.splice(index, 1);
        }
    }

    $(document).on("focus", ".changeColorDiv", function () {
        var employeeId = $(this).data("employeeid");
        var isMatch = $(this).data("ismatch");

        if (isMatch == "True") {
            $("#divClass-" + employeeId).removeClass("bg-success");
            $("#divClass-" + employeeId).css("background-color", '#93d584bd');
        }
        else {
            $("#divClass-" + employeeId).removeClass("bg-danger");
            $("#divClass-" + employeeId).css("background-color", '#f07070a6');
        }
    });

    $(document).on("focusout", ".changeColorDiv", function () {
        var employeeId = $(this).data("employeeid");
        var isMatch = $(this).data("ismatch");

        if (isMatch == "True") {
            $("#divClass-" + employeeId).addClass("bg-success");
            $("#divClass-" + employeeId).css("background-color", "");
        }
        else {
            $("#divClass-" + employeeId).addClass("bg-danger");
            $("#divClass-" + employeeId).css("background-color", "");
        }
    });

    $(document).on("focus", ".divsystemEmployee", function () {

        var employeeId = $(this).data("employeeid");
        if ($("#divSystemEmployee-" + employeeId).hasClass("bg-primary")) {
            $("#divSystemEmployee-" + employeeId).removeClass("bg-primary");
            $("#divSystemEmployee-" + employeeId).addClass("badge-warning-light");
            remove(sysEmpArray, employeeId);
        }
        else {

            $("#divSystemEmployee-" + employeeId).removeClass("badge-warning-light");
            $("#divSystemEmployee-" + employeeId).addClass("bg-primary");
            // selected
            sysEmpArray.push(employeeId);
            sysEmpArray.toString();
        }
    });

    $(document).on("focus", ".divqbEmployee", function () {

        var employeeId = $(this).data("employeeid");
        if ($("#divqbEmployee-" + employeeId).hasClass("bg-primary")) {
            $("#divqbEmployee-" + employeeId).removeClass("bg-primary");
            $("#divqbEmployee-" + employeeId).addClass("badge-warning-light");
            remove(qbEmpArray, employeeId);
        }
        else {
            $("#divqbEmployee-" + employeeId).removeClass("badge-warning-light");
            $("#divqbEmployee-" + employeeId).addClass("bg-primary");
            qbEmpArray.push(employeeId);
            qbEmpArray.toString();
        }
    });

    $("#modal-unlink-customer-form").on('hidden.bs.modal', function () {
        $("#hdnunlinkCustomerId").val('');
        $("#QBModalPopupCustomerId").val('');
        $(".has-error").removeClass("has-error");
    });

    $("#modal-edit-customer-form").on('hidden.bs.modal', function () {
        $("#tblC1").empty();
        $("#tblC2").empty();
        $("#tblRow1").removeClass("bg-success");
        $("#tblC1").removeClass("bg-success");
        $("#tblC2").removeClass("bg-success");
        $("#tblRow1").removeClass("bg-danger");
        $("#tblC1").removeClass("bg-danger");
        $("#tblC2").removeClass("bg-danger");
        $(".has-error").removeClass("has-error");
        saveLinkedCustomer.resetForm();
    });

}).apply(this, [jQuery]);