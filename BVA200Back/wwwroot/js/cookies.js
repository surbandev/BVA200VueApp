function setCookie (cName, cValue, exMinutes) {
    const d = new Date();
    if (exMinutes) {
        d.setTime(d.getTime() + exMinutes * 60 * 1000);
    } else {
        d.setTime(d.getTime() + 6 * 60 * 60 * 1000); //default to 6 hours
    }
    let expires = "expires=" + d.toUTCString();
    document.cookie = cName + "=" + cValue + ";" + expires + ";path=/;";
}

function getCookie (cName) {
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(";");
    for (var c of ca) {
        let cookie = _.trim(c).split("=");
        let cookieName = _.first(cookie);
        let cookieValue = _.last(cookie);
        if (_.isEqual(cookieName, cName)) {
            return cookieValue;
        }
    }
    return "";
}

function deleteCookie (cName) {
    this.setCookie(cName, "Session Time Out", -100);
}