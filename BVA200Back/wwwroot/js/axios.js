const baseURL = 'https://localhost:5001/';
var isAbsoluteURLRegex = /^(?:\w+:)\/\//;

axios.interceptors.request.use(function (config) {
    let UserSessionGUID = getUserSessionGUID();
    config.headers = {
        UserSessionGUID
    }
    
    if (!isAbsoluteURLRegex.test(config.url)) {
        if (config.url.startsWith('/')){
            config.url = config.url.substring(1);
        }
        config.url = baseURL + config.url;
    }

    return config;
}, function (error) {
    // Do something with request error
    return Promise.reject(error);
});

axios.interceptors.response.use(function (response) {
    refreshSessionCookie();
    return response;
}, function (error) {
    if (error.response.status === 401) {
        //this could be bad cookie or login failed.
        if (_.endsWith(error.response.config.url,'/Login')){
            //this is bad credentials.
            return Promise.reject(401);
        }
        onFail("Session has timed out.", "Session Timeout");
        deleteCookie("UserSessionGUID");
        gotoLogin();
        return Promise.reject(error);
    } else {
        refreshSessionCookie();
        return Promise.reject(error);
    }
});

