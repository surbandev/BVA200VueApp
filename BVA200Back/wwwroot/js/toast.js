function onSuccess(message, title) {
    toastr.options = {
        "positionClass": "toast-top-right",
    };
    toastr.success(message, title);
}

function onWarning(message, title) {
    toastr.options = {
        "positionClass": "toast-top-right",
    };
    toastr.info(message, title);
}

function onFail(message, title) {
    toastr.options = {
        "positionClass": "toast-top-right",
    };
    toastr.error(message, title);
}

function showInfo(message,title){
    toastr.options = {
        "positionClass": "toast-top-right",
    };
    toastr.info(message, title);
}