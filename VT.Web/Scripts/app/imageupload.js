(function () {

    'use strict';

    // See the Configuring section to configure credentials in the SDK

    AWS.config.update({ accessKeyId: 'AKIAIM5TSADPNSAXTKBA', secretAccessKey: 'ycW5cR4QMBqhdzPHxmeu5F8ZW36HZTuD7lfdGMJL' });

    // Configure your region
    AWS.config.region = 'us-east-1';

    var bucket = new AWS.S3({ params: { Bucket: 'verifyteck' } });

    // soft delete user
    $(document).on('click', '#save-image', function () {
        var companyId = $("#CompanyId").val();
        var fileName = $("#bucketImageUrl").val();
        var buttonText = $(this).html();
        $(this).attr('disabled', '').html('<i class="fa fa-spinner fa-spin"></i>&nbsp; ' + buttonText);
        $.ajax({
            type: "POST",
            url: "/Organizations/SaveImageName",
            data: {
                CompanyId: companyId,
                File: fileName
            },
            success: function (result) {
                if (result.success) {
                    VT.Util.Notification(true, "Image has been saved successfully.");
                    $("#save-image").attr('disabled', null).html('Submit');
                    setTimeout(function () {
                        window.location.reload();
                    }, 2000);
                }
                else {
                    VT.Util.HandleLogout(result.message);
                    VT.Util.Notification(false, result.message);
                    $("#save-image").attr('disabled', null).html('Submit');
                }
            },
            error: function (xhr, status, error) {
                VT.Util.Notification(false, error);
            }
        });
        return false;
    });

    $('input[name=photo]').change(function (e) {
        debugger;
        var file = e.target.files[0];
        var w = 100;
        var h = 100;
        var q = parseInt($("#ImageQuality").val());
        var c = $("#ImageCrop").val() == "true";

        // CANVAS RESIZING
        $.canvasResize(file, {
            width: w, //TODO : use configuration 
            height: h,
            //crop: c,
            //quality: q,
            callback: function (data, width, height) {
                // Create a new formdata
                //var fd = new FormData();
                var f = $.canvasResize('dataURLtoBlob', data);
                f.name = file.name;
                var uploadInProgress = true;
                var companyId = $("#CompanyId").val();
                var fileName = file.name;
                if (f) {
                    var type = f.type;
                    if (type.indexOf("jpeg") != -1) {
                    }
                    else if (type.indexOf("png") != -1) {
                    }
                    else if (type.indexOf("gif") != -1) {
                    }
                    else if (type.indexOf("bmp") != -1) {
                    }
                    else {
                        return;
                    }
                    var key = "workflowimages/" + fileName;
                    $("#ImageFileNameAfter").val(fileName);
                    $("#bucketKey").val(key);
                    var params = {
                        Key: key,
                        Body: f,
                        Acl: 'public-read'
                    };
                    bucket.upload(params, function (err, res) {
                        if (err == null) {
                            var imageUrl = "http://verifyteck.s3.amazonaws.com/" + key;
                            $("#bucketImageUrl").val(imageUrl);
                            $("#resultsAfter").html('');
                            $('#blah').attr('src', imageUrl);
                            $("#save-image").show();
                            uploadInProgress = false;
                        }
                    });
                }
                else {
                    $("#resultsAfter").html('Nothing to upload.');
                }
            }
        });
    });

}).apply(this, [jQuery]);