var authenticateUrl = "/account/login"; 

var sendRequest = function (url, verb, data, successCallback, errorCallback, options) {
    var requestOptions = options || {};
    requestOptions.type = verb;
    requestOptions.success = successCallback;
    requestOptions.error = errorCallback;
    if (!url || !verb) { errorCallback(401, "URL and HTTP verb required"); }
    if (data) { requestOptions.data = data; } $.ajax(url, requestOptions);
};

var setDefaultCallbacks = function (successCallback, errorCallback) {
    $.ajaxSetup({
        complete: function (jqXHR, status) {
            if (jqXHR.status >= 200 && jqXHR.status < 300) {
                successCallback(jqXHR.responseJSON);
            } else {
                errorCallback(jqXHR.status, jqXHR.statusText);
            }
        }
    });
};

var setAjaxHeaders = function (requestHeaders) {
    $.ajaxSetup({ headers: requestHeaders });
}; 

var authenticate = function (successCallback) {

    sendRequest(authenticateUrl, "POST", { "grant_type": "password", username: usermodel.username(), password: usermodel.password() }, function (data) {
        usermodel.authenticated(true);

        setAjaxHeaders({ Authorization: "bearer " + data.access_token });

        if (successCallback) { successCallback(); }
    });
}; 

$(document).ready(function () {
    ko.applyBindings();

    setDefaultCallbacks(function (data) {
        if (data) {
            console.log("---Begin Success---");
            console.log(JSON.stringify(data));
            console.log("---End Success---");
        }
        else {
            console.log("Success (no data)");
        }

        usermodel.gotError(false);

    },
        function (statusCode, statusText) {
            console.log("Error: " + statusCode + " (" + statusText + ")");
            usermodel.error(statusCode + " (" + statusText + ")");
            usermodel.gotError(true);
        });
}); 