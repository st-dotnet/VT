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
        var isIntegrate = $("#IsQuickbooksIntegrated").is(":checked");
        if (isIntegrate) {
            $(".quikcbook-settings-div").show();
        }
        else {
            $(".quikcbook-settings-div").hide();
        }
    });

    $(document).on('change', '.js-switch3', function (ev) {
        debugger;
        //console.log(ev);
        //console.log(ev.currentTarget.checked);
        var isSettingsOn = $(this).data('issettingson');
        var ischecked = $(this).data('switchery');

        if (isSettingsOn == "True") {
            if (ischecked == false) {
                $(this).attr('switchery', true);
                $(this).data('switchery', true);

                $.ajax({
                    url: '/Quickbooks/GetModal',
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
                            url: '/Quickbooks/GetSynchronizationListData',
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
                    url: '/Quickbooks/GetModal',
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
                            url: '/Quickbooks/GetSynchronizationListData',
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

    // -- CUSTOMER SYNCHRONIZATION--
    $(document).on("click", "#save-update-list", function () {

        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

        $.ajax({
            url: '/Quickbooks/SaveUpdatedList',
            type: "POST",
            success: function (result) {

                if (result.success) {
                    VT.Util.Notification(true, result.message);
                    $("#modal-sync-cutomers-form").modal("hide");
                    setTimeout(function () {
                        window.location.href = "/Quickbooks";
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
            url: '/Quickbooks/EditLinkedCustomer',
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

    // save customer settings
    $("#btnunlinkCustomer").click(function () {

        var customerId = $("#hdnunlinkCustomerId").val();
        var qbCustomerId = $("#QBModalPopupEntityId").val();
        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

        $.ajax({
            url: '/Quickbooks/UnlinkCustomer',
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
            url: '/Quickbooks/SaveUnlinkedCustomer',
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
        var realmId = $("#RealmId").val();

        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

        $.ajax({
            url: '/Quickbooks/SaveSettings/',
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
                QbSettingsId: qbSettingsId,
                RealmId: realmId
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

    // edit linked customer
    $(document).on("click", "#sync-linked-customers", function () {
        $.ajax({
            url: '/Customers/GetSynchronizationListData/',
            type: "POST",
            success: function (result) {
                $("#mainDiv").html(result);
                $("#modal-sync-cutomers-form").modal({
                    backdrop: 'static',
                    keyboard: false,
                    show: true
                });
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
            }
        });
    });
    ////  
    // ----EMPLOYEE SYNCHRONIZATION----
    $(document).on('change', '.js-switch4', function (ev) {
        //console.log(ev);
        //console.log(ev.currentTarget.checked);
        var isSettingsOn = $(this).data('issettingson');
        var ischecked = $(this).data('switchery');

        if (isSettingsOn == "True") {
            if (ischecked == false) {
                $(this).attr('switchery', true);
                $(this).data('switchery', true);
                $.ajax({
                    url: '/Quickbooks/GetEmployeeModal',
                    type: "POST",
                    success: function (result) {
                        $("#mainEmployeeDiv").html(result);
                        $("#modal-sync-employees-form").modal({
                            backdrop: 'static',
                            keyboard: false,
                            show: true
                        });
                        $("#mainEmployeeDiv").html('<b>Please wait quickbooks list is being prepared...</b> <br/><br/><br/><br/><i class="fa fa-spinner fa-spin fa-lg" style="font-size:100px;"></i>');
                        // fetch data 
                        $.ajax({
                            url: '/Quickbooks/GetEmployeeSynchronizationList',
                            type: "POST",
                            success: function (result) {
                                $("#mainEmployeeDiv").html(result);
                            },
                            error: function (xhr, status, error) {
                                $("#mainEmployeeDiv").html(error);
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
                    url: '/Quickbooks/GetEmployeeSynchronizationData',
                    type: "POST",
                    success: function (result) {
                        $("#mainEmployeeDiv").html(result);
                        $("#modal-sync-employees-form").modal({
                            backdrop: 'static',
                            keyboard: false,
                            show: true
                        });
                        $("#mainEmployeeDiv").html('<b>Please wait quickbooks list is being prepared...</b> <br/><br/><br/><br/><i class="fa fa-spinner fa-spin fa-lg" style="font-size:100px;"></i>');
                        // fetch data 
                        $.ajax({
                            url: '/Quickbooks/GetEmployeeSynchronizationList',
                            type: "POST",
                            success: function (result) {
                                $("#mainEmployeeDiv").html(result);
                            },
                            error: function (xhr, status, error) {
                                $("#mainEmployeeDiv").html(error);
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

    // edit linked employee
    $(document).on("click", ".editLinkedEmployee", function () {
        var employeeId = $(this).data("editemployeeid");
        var qbId = $(this).data("editqbemployeeid");

        var editEmployeeId = employeeId;
        if (employeeId == "") {
            editEmployeeId = qbId;
        }
        $.ajax({
            url: '/Quickbooks/EditLinkedEmployee/' + editEmployeeId,
            type: "POST",
            success: function (result) {
                if (result.success) {
                    debugger;
                    $("#CompanyId").val(result.employee.CompanyId);
                    $("#GivenName").val(result.employee.GivenName);
                    $("#FamilyName").val(result.employee.FamilyName);
                    $("#CompanyId").val(result.employee.CompanyId);
                    $("#EmployeeId").val(result.employee.EmployeeId);
                    $("#Email").val(result.employee.Email);
                    $("#QBEmployeeId").val(result.employee.QBEmployeeId);

                    $("#matchStatus").text(result.employee.IsMatch ? "Set Employee Data" : "Set Employee Data");
                    $("#tblSysstemEmployee tbody").append(result.systememployee);
                    $("#tblQBEmployee tbody").append(result.qbemployee);
                    if (result.employee.IsMatch) {
                        $("#tblRow1E").addClass("bg-success");
                        $("#tblC1E").addClass("bg-success");
                        $("#tblC2E").addClass("bg-success");
                    }
                    else {
                        $("#tblRow1E").addClass("bg-danger");
                        $("#tblC1E").addClass("bg-danger");
                        $("#tblC2E").addClass("bg-danger");
                    }
                    $("#modal-edit-linkemployee-form").modal({
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

    var saveLinkedEmployee = $("#saveEditEmployeePopup").validate({
        rules: {
            GivenName: { required: true },
            FamilyName: { required: true },
            Email: { required: true },
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
            var buttonText = $("#btneditSaveEmployee").html();
            $("#btneditSaveEmployee").attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

            $(form).ajaxSubmit({
                success: function (data) {
                    $("#btneditSaveEmployee").attr('disabled', null).html('Submit');
                    $("#modal-edit-linkemployee-form").modal("hide");
                    $("#mainEmployeeDiv").html(data);
                    $("#linkedemployeespanel").show();
                },
                error: function (xhr, status, error) {
                    $("#btneditSaveEmployee ").attr('disabled', null).html(buttonText);
                }
            });
            return false;
        }
    });

    $(document).on("click", "#save-updateEmployee-list", function () {
        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);
        $.ajax({
            url: '/Quickbooks/SaveEmployeeUpdatedList',
            type: "POST",
            success: function (result) {
                if (result.success) {
                    VT.Util.Notification(true, result.message);
                    $("#modal-sync-employees-form").modal("hide");
                    setTimeout(function () {
                        window.location.href = "/Quickbooks";
                    }, 3000);
                }
                else {
                    VT.Util.Notification(false, result.message);
                }
                $("#save-updateEmployee-list").attr('disabled', null).html(buttonText);
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
                $("#save-updateEmployee-list").attr('disabled', null).html(buttonText);
            }
        });
    });

    $(document).on("click", ".unlinkEmployee", function () {
        debugger;
        var employeeId = $(this).data("unlinkemployeeid");
        var qbEmployeeId = $(this).data("unlinkqbemployeeid");

        $("#hdnunlinkEmployeeId").val(employeeId);
        $("#QBEmpId").val(qbEmployeeId);
        $("#modal-unlink-employee-form").modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });
    });

    // save customer settings
    $("#btnunlinkEmployee").click(function () {
        var employeeId = $("#hdnunlinkEmployeeId").val();
        var qbEmployeeId = $("#QBEmpId").val();
        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

        $.ajax({
            url: '/Quickbooks/UnlinkEmployee',
            data: {
                EntityId: employeeId,
                QBEntityId: qbEmployeeId
            },
            type: "POST",
            success: function (result) {

                $("#btnunlinkEmployee").attr('disabled', null).html(buttonText);
                $("#modal-unlink-employee-form").modal("hide");
                $("#mainEmployeeDiv").html(result);
                $("#linkedemployeespanel").show();
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
                $("#btnunlinkEmployee").attr('disabled', null).html(buttonText);
            }
        });
    });

    // ----SERVICE SYNCHRONIZATION----
    $(document).on('change', '.js-switch2', function (ev) {
        //console.log(ev);
        //console.log(ev.currentTarget.checked);
        var isSettingsOn = $(this).data('issettingson');
        var ischecked = $(this).data('switchery');

        if (isSettingsOn == "True") {
            if (ischecked == false) {
                $(this).attr('switchery', true);
                $(this).data('switchery', true);
                $.ajax({
                    url: '/Quickbooks/GetServiceModal',
                    type: "POST",
                    success: function (result) {
                        $("#mainServiceDiv").html(result);
                        $("#modal-sync-services-form").modal({
                            backdrop: 'static',
                            keyboard: false,
                            show: true
                        });
                        $("#mainServiceDiv").html('<b>Please wait quickbooks list is being prepared...</b> <br/><br/><br/><br/><i class="fa fa-spinner fa-spin fa-lg" style="font-size:100px;"></i>');
                        // fetch data 
                        $.ajax({
                            url: '/Quickbooks/GetServiceSynchronizationListData',
                            type: "POST",
                            success: function (result) {
                                $("#mainServiceDiv").html(result);
                            },
                            error: function (xhr, status, error) {
                                $("#mainServiceDiv").html(error);
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
                    url: '/Quickbooks/GetServiceModal',
                    type: "POST",
                    success: function (result) {
                        $("#mainServiceDiv").html(result);
                        $("#modal-sync-services-form").modal({
                            backdrop: 'static',
                            keyboard: false,
                            show: true
                        });
                        $("#mainServiceDiv").html('<b>Please wait quickbooks list is being prepared...</b> <br/><br/><br/><br/><i class="fa fa-spinner fa-spin fa-lg" style="font-size:100px;"></i>');
                        // fetch data 
                        $.ajax({
                            url: '/Quickbooks/GetServiceSynchronizationListData',
                            type: "POST",
                            success: function (result) {
                                $("#mainServiceDiv").html(result);
                            },
                            error: function (xhr, status, error) {
                                $("#mainServiceDiv").html(error);
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

    // edit linked services
    $(document).on("click", ".editLinkedService", function () {
        var serviceId = $(this).data("editserviceid");
        var qbserviceId = $(this).data("editqbserviceid");
        $.ajax({
            url: '/Quickbooks/EditLinkedService',
            type: "POST",
            data: {
                ServiceId: serviceId,
                QBServiceId: qbserviceId
            },
            success: function (result) {
                if (result.success) {
                    $("#CompanyId").val(result.service.CompanyId);
                    $("#ServiceId").val(result.service.ServiceId);
                    $("#QBServiceId").val(result.service.QBServiceId);

                    $("#Name").val(result.service.Name);
                    $("#Description").val(result.service.Description);
                    $("#matchServiceStatus").text(result.service.IsMatch ? "Set Service Data" : "Set Service Data");

                    $("#tblSysstemService tbody").append(result.systemservice);
                    $("#tblQBService tbody").append(result.qbService);

                    if (result.service.IsMatch) {
                        $("#tblRow1S").addClass("bg-success");
                        $("#tblServiceC1").addClass("bg-success");
                        $("#tblServiceC2").addClass("bg-success");
                    }
                    else {
                        $("#tblRow1S").addClass("bg-danger");
                        $("#tblServiceC1").addClass("bg-danger");
                        $("#tblServiceC2").addClass("bg-danger");
                    }
                    $("#modal-edit-linkservice-form").modal({
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

    var saveLinkedService = $("#saveEditServicePopup").validate({
        rules: {
            Name: { required: true },
            Description: { required: true },
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
            var buttonText = $("#btneditSaveService").html();
            $("#btneditSaveService").attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

            $(form).ajaxSubmit({
                success: function (data) {
                    debugger;
                    $("#btneditSaveService").attr('disabled', null).html('Submit');
                    $("#modal-edit-linkservice-form").modal("hide");
                    $("#mainServiceDiv").html(data);
                    $("#linkedServicePanel").show();
                },
                error: function (xhr, status, error) {
                    $("#btneditSaveService").attr('disabled', null).html(buttonText);
                }
            });
            return false;
        }
    });

    $(document).on("click", ".unlinkService", function () {
        debugger;
        var serviceId = $(this).data("unlinkserviceid");
        var qbServiceId = $(this).data("unlinkqbserviceid");

        $("#hdnunlinkServiceId").val(serviceId);
        $("#QBServId").val(qbServiceId);
        $("#modal-unlink-service-form").modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });
    });

    // unlink service
    $("#btnunlinkService").click(function () {
        var serviceId = $("#hdnunlinkServiceId").val();
        var qbServiceId = $("#QBServId").val();
        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

        $.ajax({
            url: '/Quickbooks/UnlinkService',
            data: {
                EntityId: serviceId,
                QBEntityId: qbServiceId
            },
            type: "POST",
            success: function (result) {
                $("#btnunlinkService").attr('disabled', null).html(buttonText);
                $("#modal-unlink-service-form").modal("hide");
                $("#mainServiceDiv").html(result);
                $("#linkedServicePanel").show();
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
                $("#btnunlinkService").attr('disabled', null).html(buttonText);
            }
        });
    });

    var sysServiceArray = [];
    var qbServiceArray = [];

    $(document).on("click", "#btnServicesClose", function () {
        $("#ServicesSettings").trigger("click");
    });

    $(document).on("focus", ".changeServiceColorDiv", function () {
        var serviceId = $(this).data("serviceid");
        var isMatch = $(this).data("ismatch");

        if (isMatch == "True") {
            $("#divClassService-" + serviceId).removeClass("bg-success");
            $("#divClassService-" + serviceId).css("background-color", '#93d584bd');
        }
        else {
            $("#divClassService-" + serviceId).removeClass("bg-danger");
            $("#divClassService-" + serviceId).css("background-color", '#f07070a6');
        }
    });

    $(document).on("focus", ".divservicesystemServices", function () {
        debugger;
        var serviceId = $(this).data("serviceid");
        if (sysServiceArray.length < 1) {
            if ($("#divSystemService-" + serviceId).hasClass("bg-primary")) {
                $("#divSystemService-" + serviceId).removeClass("bg-primary");
                $("#divSystemService-" + serviceId).addClass("badge-warning-light");
                remove(sysServiceArray, serviceId);
            }
            else {
                $("#divSystemService-" + serviceId).removeClass("badge-warning-light");
                $("#divSystemService-" + serviceId).addClass("bg-primary");
                sysServiceArray.push(serviceId);
                sysServiceArray.toString();

                if ($(".divservicesystemSelected").hasClass("bg-primary")) {
                    $(".divservicesystemSelected").removeClass("bg-primary");
                    $(".divservicesystemSelected").addClass("alert-info");
                }
            }
        }
        else {
            var id = sysServiceArray[0];
            if (id == serviceId) {
                $("#divSystemService-" + serviceId).removeClass("bg-primary");
                $("#divSystemService-" + serviceId).addClass("badge-warning-light");
            }
            else {
                $("#divSystemService-" + serviceId).removeClass("badge-warning-light");
                $("#divSystemService-" + serviceId).addClass("bg-primary");
                sysServiceArray.push(serviceId);
                sysServiceArray.toString();
            }
            for (var i = 0; i < sysServiceArray.length; i++) {
                $("#divSystemService-" + sysServiceArray[i]).removeClass("bg-primary");
                $("#divSystemService-" + sysServiceArray[i]).addClass("badge-warning-light");
                remove(sysServiceArray, sysServiceArray[i]);
            }
        }
        if ($(".divserviceqbSelected").hasClass("bg-primary") && sysServiceArray.length == 1) {
            $("#link-services").show();
        }
        else {
            $("#link-services").hide();
        }
        if ($(".divserviceqbSelected").hasClass("bg-primary") && $(".divservicesystemSelected").hasClass("bg-primary")) {
            $("#link-services").hide();
        }
        if (sysServiceArray.length == 1 && qbServiceArray.length == 1) {
            $("#link-services").show();
        }
    });

    // div qb services
    $(document).on("focus", ".divqbServices", function () {
        debugger;
        var serviceId = $(this).data("serviceid");
        if (qbServiceArray.length < 1) {
            if ($("#divserviceqbService-" + serviceId).hasClass("bg-primary")) {
                $("#divserviceqbService-" + serviceId).removeClass("bg-primary");
                $("#divserviceqbService-" + serviceId).addClass("badge-warning-light");
                remove(qbServiceArray, serviceId);
            }
            else {
                $("#divserviceqbService-" + serviceId).removeClass("badge-warning-light");
                $("#divserviceqbService-" + serviceId).addClass("bg-primary");
                qbServiceArray.push(serviceId);
                qbServiceArray.toString();
                if ($(".divserviceqbSelected").hasClass("bg-primary")) {
                    $(".divserviceqbSelected").removeClass("bg-primary");
                    $(".divserviceqbSelected").addClass("alert-info");
                }
            }
        }
        else {
            var id = qbServiceArray[0];
            if (id == serviceId) {
                $("#divserviceqbService-" + serviceId).removeClass("bg-primary");
                $("#divserviceqbService-" + serviceId).addClass("badge-warning-light");
            }
            else {
                $("#divserviceqbService-" + serviceId).removeClass("badge-warning-light");
                $("#divserviceqbService-" + serviceId).addClass("bg-primary");
                qbServiceArray.push(serviceId);
                qbServiceArray.toString();
            }
            for (var s = 0; s < qbServiceArray.length; s++) {
                $("#divserviceqbService-" + qbServiceArray[s]).removeClass("bg-primary");
                $("#divserviceqbService-" + qbServiceArray[s]).addClass("badge-warning-light");
                remove(qbServiceArray, qbServiceArray[s]);
            }
        }

        if ($(".divservicesystemSelected").hasClass("bg-primary") && qbServiceArray.length == 1) {
            $("#link-services").show();
        }
        else {
            $("#link-services").hide();
        }

        if ($(".divserviceqbSelected").hasClass("bg-primary") && $(".divservicesystemSelected").hasClass("bg-primary")) {
            $("#link-services").hide();
        }
        if (sysServiceArray.length == 1 && qbServiceArray.length == 1) {
            $("#link-services").show();
        }
    });

    $(document).on("focus", ".divservicesystemSelected", function () {

        if ($(".divservicesystemSelected").hasClass("alert-info")) {
            $(".divservicesystemSelected").removeClass("alert-info");
            $(".divservicesystemSelected").addClass("bg-primary");

            for (var i = 0; i < sysServiceArray.length; i++) {
                $("#divSystemService-" + sysServiceArray[i]).removeClass("bg-primary");
                $("#divSystemService-" + sysServiceArray[i]).addClass("badge-warning-light");
                remove(sysServiceArray, sysServiceArray[i]);
            }
        }
        else {
            $(".divservicesystemSelected").removeClass("bg-primary");
            $(".divservicesystemSelected").addClass("alert-info");
        }
        if ($(".divservicesystemSelected").hasClass("bg-primary") && qbServiceArray.length == 1) {
            $("#link-services").show();
        }
        else {
            $("#link-services").hide();
        }
    });

    $(document).on("focus", ".divserviceqbSelected", function () {
        debugger;

        if ($(".divserviceqbSelected").hasClass("alert-info")) {
            $(".divserviceqbSelected").removeClass("alert-info");
            $(".divserviceqbSelected").addClass("bg-primary");

            for (var i = 0; i < qbServiceArray.length; i++) {
                $("#divserviceqbService-" + qbServiceArray[i]).removeClass("bg-primary");
                $("#divserviceqbService-" + qbServiceArray[i]).addClass("badge-warning-light");
                remove(qbServiceArray, qbServiceArray[i]);
            }
        }
        else {
            $(".divserviceqbSelected").removeClass("bg-primary");
            $(".divserviceqbSelected").addClass("alert-info");
        }
        if ($(".divserviceqbSelected").hasClass("bg-primary") && sysServiceArray.length == 1) {
            $("#link-services").show();
        }
        else {
            $("#link-services").hide();
        }
    });

    var saveEditLinkedService = $("#saveEditLinkedServicePopup").validate({
        rules: {
            Name1: { required: true },
            Description1: { required: true },
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
            var buttonText = $("#btneditLinkUnlinkedSaveService").html();
            $("#btneditLinkUnlinkedSaveService").attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);
            $(form).ajaxSubmit({
                success: function (data) {
                    debugger;
                    $("#btneditLinkUnlinkedSaveService").attr('disabled', null).html('Submit');
                    $("#modal-edit-linkUnlinkService-form").modal("hide");
                    $("#mainServiceDiv").html(data);
                    $("#linkedServicePanel").show();
                },
                error: function (xhr, status, error) {
                    $("#btneditLinkUnlinkedSaveService").attr('disabled', null).html(buttonText);
                }
            });
            return false;
        }
    });

    // edit linked employee
    $(document).on("click", "#link-services", function () {
        debugger;
        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);
        $.ajax({
            url: '/Quickbooks/ShowLinkedServices/',
            type: "POST",
            data: {
                SystemServices: sysServiceArray,
                QbServices: qbServiceArray
            },
            success: function (result) {
                if (result.success) {
                    debugger;
                    $("#CompanyId1").val(result.service.CompanyId);
                    $("#ServiceId1").val(result.service.ServiceId);
                    $("#QBServiceId1").val(result.service.QBServiceId);

                    $("#Name1").val(result.service.Name);
                    $("#Description1").val(result.service.Description);
                    $("#matchStatusS").text(result.service.IsMatch ? "Set Service Data" : "Set Service Data");
                    $("#tblSysstemService1 tbody").append(result.systemservice);
                    $("#tblQBService1 tbody").append(result.qbService);

                    if (result.service.IsMatch) {
                        $("#tblServiceRow11S").addClass("bg-success");
                        $("#tblServiceC11").addClass("bg-success");
                        $("#tblServiceC21E").addClass("bg-success");
                    }
                    else {
                        $("#tblServiceRow11S").addClass("bg-danger");
                        $("#tblServiceC11").addClass("bg-danger");
                        $("#tblServiceC21E").addClass("bg-danger");
                    }
                    $("#link-services").attr('disabled', null).html(buttonText);
                    $("#modal-edit-linkUnlinkService-title").html("Linked Employee")
                    $("#modal-edit-linkUnlinkService-form").modal({
                        backdrop: 'static',
                        keyboard: false,
                        show: true
                    });
                    $("#link-services").attr('disabled', null).html(buttonText);
                    sysServiceArray.length = 0;
                    qbServiceArray.length = 0;
                    $("#link-services").hide();
                }
                else {
                    VT.Util.HandleLogout(result.message);
                    VT.Util.Notification(false, result.message);
                    $("#link-services").attr('disabled', null).html(buttonText);
                }
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
            }
        });
        return false;
    });

    $(document).on("click", "#save-updateservice-list", function () {

        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

        $.ajax({
            url: '/Quickbooks/SaveServiceUpdatedList',
            type: "POST",
            success: function (result) {

                if (result.success) {
                    VT.Util.Notification(true, result.message);
                    $("#modal-sync-services-form").modal("hide");
                    setTimeout(function () {
                        window.location.href = "/Quickbooks";
                    }, 3000);
                }
                else {
                    VT.Util.Notification(false, result.message);
                }
                $("#save-updateservice-list").attr('disabled', null).html(buttonText);
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
                $("#save-updateservice-list").attr('disabled', null).html(buttonText);
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

    // ----HIDDEN MODALS POPUPS----
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
    });

    $("#modal-edit-linkemployee-form").on('hidden.bs.modal', function () {
        $("#tblC1E").empty();
        $("#tblC2E").empty();
        $("#tblRow1E").removeClass("bg-success");
        $("#tblC1E").removeClass("bg-success");
        $("#tblC2E").removeClass("bg-success");
        $("#tblRow1E").removeClass("bg-danger");
        $("#tblC1E").removeClass("bg-danger");
        $("#tblC2E").removeClass("bg-danger");
        $(".has-error").removeClass("has-error");
    });

    $("#modal-edit-linkemployee-form").on('hidden.bs.modal', function () {
        saveLinkedEmployee.resetForm();
        $(".has-error").removeClass("has-error");
    });

    $("#modal-edit-linkservice-form").on('hidden.bs.modal', function () {
        saveLinkedService.resetForm();
        $("#tblServiceC1").empty();
        $("#tblServiceC2").empty();
        $("#tblRow1S").removeClass("bg-success");
        $("#tblServiceC1").removeClass("bg-success");
        $("#tblServiceC2").removeClass("bg-success");

        $("#tblRow1S").removeClass("bg-danger");
        $("#tblServiceC1").removeClass("bg-danger");
        $("#tblServiceC2").removeClass("bg-danger");
        $(".has-error").removeClass("has-error");
    });

    $("#modal-unlink-service-form").on('hidden.bs.modal', function () {
        $("#hdnunlinkServiceId").val('');
        $("#QBServId").val('');
        $(".has-error").removeClass("has-error");
    });

    $("#modal-edit-linkUnlinkService-form").on('hidden.bs.modal', function () {
        saveEditLinkedService.resetForm();
        $("#tblServiceC11").empty();
        $("#tblServiceC21E").empty();
        $("#tblServiceRow11S").removeClass("bg-success");
        $("#tblServiceC11").removeClass("bg-success");
        $("#tblServiceC21E").removeClass("bg-success");

        $("#tblServiceRow11S").removeClass("bg-danger");
        $("#tblServiceC11").removeClass("bg-danger");
        $("#tblServiceC21E").removeClass("bg-danger");
        $(".has-error").removeClass("has-error");
    });

}).apply(this, [jQuery]);