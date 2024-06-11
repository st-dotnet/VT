(function () {

    'use strict';

    $("#btnDeleteCs").click(function () {

        var checkedCount = $("#CompanyServiceListGrid input:checked").length;
        if (checkedCount > 0) {
            $("#modal-del-cs-form").show();
            $("#modal-del-cs-form").modal({
                backdrop: 'static',
                keyboard: false,
                show: true
            });
        } else {
            VT.Util.Notification(false, "Please select at least one company service to deactivate.");
        }
    });

    // activate service
    $(document).on('click', '.companyservice-activate', function () {
        
        var id = $(this).data("id");
        $("#hdnActivateServiceId").val(id);
        $("#modal-activate-companyservice-form").modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });
    });

    //de-activate service
    $(document).on('click', '.companyservice-deactivate', function () {
        var id = $(this).data("id");
        $("#hdnDeactiveServiceId").val(id);
        $("#modal-deactivate-companyservice-form").modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });
    });

    //Activate service
    $("#btnActivateService").click(function () {
        
        var id = $("#hdnActivateServiceId").val();
        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

        $.ajax({
            url: '/CompanyServices/ActivateCompanyService/' + id,
            type: "POST",
            success: function (result) {
                if (result.success) {
                    VT.Util.Notification(true, result.message);
                    $("#modal-activate-companyservice-form").modal("hide");
                    //refresh grid
                    var grid = $("#CompanyServiceListGrid").data("kendoGrid");
                    grid.dataSource.read();
                }
                else {
                    VT.Util.HandleLogout(result.message);
                    VT.Util.Notification(false, "Some error occured while activating customer.");
                }
                $("#btnActivateService").attr('disabled', null).html('Submit');
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
                $("#btnActivateService").attr('disabled', null).html('Submit');
            }
        });
    });

    //Deactivate service
    $("#btnDeactivateService").click(function () {
        
        var id = $("#hdnDeactiveServiceId").val();
        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

        $.ajax({
            url: '/CompanyServices/DeactivateCompanyService/' + id,
            type: "POST",
            success: function (result) {
                if (result.success) {
                    VT.Util.Notification(true, result.message);
                    $("#modal-deactivate-companyservice-form").modal("hide");
                    //refresh grid
                    var grid = $("#CompanyServiceListGrid").data("kendoGrid");
                    grid.dataSource.read();
                }
                else {
                    VT.Util.HandleLogout(result.message);
                    VT.Util.Notification(false, "Some error occured while deactivating customer.");
                }
                $("#btnDeactivateService").attr('disabled', null).html('Submit');
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
                $("#btnDeactivateService").attr('disabled', null).html('Submit');
            }
        });
    });


    $("#btnUndeleteService").click(function () {
        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

        var id = $("#hdnCompanyServiceId").val();

        $.ajax({
            url: '/CompanyServices/UnDeleteCompanyServices/' + id,
            type: "POST", 
            success: function (result) {
                if (result.success) {
                    VT.Util.Notification(true, "CompanyService have been successfully activated.");
                    //refresh grid
                    var grid = $("#CompanyServiceListGrid").data("kendoGrid");
                    grid.dataSource.read();
                } else {
                    VT.Util.HandleLogout(result.message);
                    VT.Util.Notification(false, result.message);
                }

                $("#modal-undelete-form").modal("hide");
                $("#btnUndeleteService").attr('disabled', null).html('Submit');
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
                $("#btnUndeleteService").attr('disabled', null).html('Submit');
            }
        });
    });

    $("#btnModalSubmit").click(function () { 
        var buttonText = $("#btnModalSubmit").html();
        $("#btnModalSubmit").attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);
        
        var ids = [];
        $("#CompanyServiceListGrid input:checked").each(function () {
            ids.push($(this).val());
        });
        console.log(ids);
        $.ajax({
            url: '/CompanyServices/DeleteCompanyServices',
            type: "POST",
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ Ids: ids }),
            success: function (result) { 
                if (result.success) {
                    VT.Util.Notification(true, "Selected CompanyService(s) have been successfully deactivated.");
                    //refresh grid
                    var grid = $("#CompanyServiceListGrid").data("kendoGrid");
                    grid.dataSource.read();
                } else {
                    VT.Util.HandleLogout(result.message);
                    VT.Util.Notification(false, result.message);
                }

                $("#modal-del-cs-form").modal("hide");
                $("#btnModalSubmit").attr('disabled', null).html('Submit');
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
                $("#btnModalSubmit").attr('disabled', null).html('Submit');
            }
        });
    });

    $(document).on('click', '.edit-companyservice', function () {
        var id = $(this).data("id");
        $.ajax({
            url: '/CompanyServices/GetCompanyService/' + id,
            type: 'POST',
            success: function (result) { 
                if (result && result.success == true) {

                    $("#CompanyServiceId").val(result.cs.Id);
                    $("#Name").val(result.cs.Name);
                    $("#Description").val(result.cs.Description);
                    $("#CompanyId").val(result.cs.CompanyId);

                    $("#modal-save-cs-title").html("Edit Company Service");
                    $("#modal-add-cs-form").modal({
                        backdrop: 'static',
                        keyboard: false,
                        show: true
                    });

                } else {
                    VT.Util.HandleLogout(result.message);
                    VT.Util.Notification(false, result.message);
                }
            }
        });
        return false;
    });

    //close-company service details
    $(document).on('click', '.close-companyService', function () {
        $("#divcsDetails").hide();
    });

    //modal-undelete-form
    $(document).on('click', '.undelete-companyservice', function () {
        var id = $(this).data("id");
        $("#hdnCompanyServiceId").val(id);
        $("#modal-undelete-form").modal({
            backdrop: 'static',
            keyboard: false,
            show: true 
        });
    });

    $(document).on('click', '.view-companyservice', function () {
        var id = $(this).data("id");
        $("#divcsDetails").html('<i class="fa fa-spinner fa-spin"></i>&nbsp; Please wait..');
        $.ajax({
            url: '/CompanyServices/GetCompanyServiceDetail/' + id,
            type: 'POST',
            success: function (result) {
                VT.Util.HandleLogout(result.message);
                $("#divcsDetails").show();
                $("#divcsDetails").html(result);
                $('html,body').animate({
                    scrollTop: $("#divcsDetails").offset().top
                }, 'slow');
            },
            error: function (xhr, status, error) {
                $("#divcsDetails").html('No data found.');
            }
        });
        return false;
    });

    // basic
    var saveCompanyServiceForm  = $("#saveCompanyServiceForm").validate({
        rules: {
            CompanyId : {
                required: true
            },
            Name: {
                required: true
            },
            Description: {
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
            var buttonText = $("#btnSaveCompanyService").html();
            $("#btnSaveCompanyService").attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);

            $(form).ajaxSubmit({
                success: function (data) {
                    if (data && data.success) {
                        $("#modal-add-cs-form").modal("hide");
                       
                        $("#modal-save-cs-title").html("Add Company Service");
                        //refresh grid
                        var grid = $("#CompanyServiceListGrid").data("kendoGrid");
                        grid.dataSource.read();

                        VT.Util.Notification(true, "Company Service has been successfully saved.");
                    } else {
                        VT.Util.HandleLogout(data.message);
                        $('#saveCompanyServiceForm .alert-danger').removeClass("hide").find(".error-message").html(data.message);
                        VT.Util.Notification(false, "Some error occured while saving current company service.");
                    }

                    $("#btnSaveCompanyService").attr('disabled', null).html(buttonText);
                    $(form).resetForm();
                    $("#CompanyServiceId").val(0);
                },
                error: function (xhr, status, error) {
                    $("#btnSaveCompanyService").attr('disabled', null).html(buttonText);
                }
            });
            return false;
        }
    });

    $("#modal-add-cs-form").on('hidden.bs.modal', function () {
        $("#modal-save-cs-title").html("Add Company Service");
        saveCompanyServiceForm.resetForm();
        $("#CompanyServiceId").val(0);
        $(".has-error").removeClass("has-error");
    });

}).apply(this, [jQuery]);