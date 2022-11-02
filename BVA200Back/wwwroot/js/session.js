// USER IN AND OUT METHODS
function login(userName, userPassword) {
    return new Promise((resolve, reject) => {
        const url = "Home/Login";
        const payload = {
            UserName: userName,
            Password: userPassword
        };
        axios.post(url, payload).then((response) => {
            let sessionGUID = response.data.userSessionGUID;
            let timeout = response.data.sessionTimeout;
            window.localStorage.setItem("sessionTimeout",timeout);
            setUserSessionCookie(sessionGUID, timeout);
            resolve();
        }).catch((err) => {
            console.log(err);
            if (err === 401){
                onFail("User Name or Password is incorrect.", "Invalid Credentials!");
                reject();
                return;
            }
            onFail("Failed To Sign In", "Failed");
            reject();
        });
    })
};

function logout() {
    return new Promise(async (resolve,reject)=>{
        let detectorState = await getDetectorState();
        if (detectorState === "Reading") {
            //show confirmation modal and if they click "OK", stop read, clear memory, and close the reader THEN logout
        }

        /*
            Just sending the Axios Post will have the UserSessionGUID in the header
            so there's no need to pass a payload.

            The Axios intercepter reads the cookie at the time of the request so we can't just delete the cookie
            then make the request.

        */
        const url = "Home/Logout";
        axios.post(url).then(function () {
            onSuccess("User Signed Out.", "Success!");
            deleteUserSessionCookie();
            resolve();
        }).catch((err) => {
            deleteUserSessionCookie();
            onError("Failed To Logout", "Failed");
            reject(err);
        });
    });
}

function getUserSessionGUID() {
    return getCookie("UserSessionGUID");
}

function setUserSessionCookie (cValue, exMinutes) {
    this.setCookie("UserSessionGUID", cValue, exMinutes);
}

function deleteUserSessionCookie(){
    deleteCookie("UserSessionGUID");
}

function refreshSessionCookie() {
    //this one we're just going to assume the cookie name
    var timeout = window.localStorage.getItem("sessionTimeout");
    var cookieTimeout = moment(Date.now).add(Number(timeout), 'm').toDate();
    let userSessionGUID = getCookie("UserSessionGUID");
    setCookie("UserSessionGUID", userSessionGUID, cookieTimeout);
    console.log('Timeout cookie refreshed');
}