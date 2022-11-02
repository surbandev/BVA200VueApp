// Home Button Method
function gotoHome() {
    window.location.replace(baseURL + "Home/Index?UserSessionGUID="+getUserSessionGUID());
}
function gotoArchivedTests() {
    window.location.replace(baseURL + "Utilities/GetTestResults?UserSessionGUID="+getUserSessionGUID());
}
function gotoQCLogs() {
    window.location.replace(baseURL + "Utilities/QCLogs?UserSessionGUID="+getUserSessionGUID());
}
function gotoUtilitiesLanding() {
    window.location.replace(baseURL + "Utilities/GetUtilitesLanding?UserSessionGUID="+getUserSessionGUID());
}
function gotoUserConfiguration() {
    window.location.replace(baseURL + "Utilities/GetUserConfiguration?UserSessionGUID="+getUserSessionGUID());
}
function gotoQCLanding(){
    window.location.replace(baseURL + "QC/QCLanding?UserSessionGUID="+getUserSessionGUID())
}
function gotoQCCalibration() {
    window.location.replace(baseURL + "QC/QCCalibration?UserSessionGUID="+getUserSessionGUID());
}
function gotoQCConstancy(){
    window.location.replace(baseURL + "QC/QCConstancy?UserSessionGUID="+getUserSessionGUID());
}
function gotoQCEfficiency(){
    window.location.replace(baseURL + "QC/QCEfficiency?UserSessionGUID="+getUserSessionGUID());
}
function gotoQCLinearity(){
    window.location.replace(baseURL + "QC/QCLinearity?UserSessionGUID="+getUserSessionGUID());
}
function gotoSPA() {
    window.location.replace(baseURL + "StandardBVA/GetNewTest?UserSessionGUID="+getUserSessionGUID());
}
function gotoManualCalc() {
    window.location.replace(baseURL + "Utilities/GetManualCalc?UserSessionGUID="+getUserSessionGUID());
}
function gotoDetectorSetup(){
    window.location.replace(baseURL + "TopazDetector/SetupDetector?UserSessionGUID="+getUserSessionGUID());
}
async function gotoLogin(){
    await logout();
    window.location.replace(baseURL);
}

async function goto(page){
    let status = await getDetectorState();
    if (status === 'Reading'){
        showConfirmationModal("Detector in use","If you navigate away, the reader will halt and any progress will be lost. Do you wish to continue",true,closeReaderAndNavigate,page);
    }else{
        navigateTo(page);
    }
}

async function closeReaderAndNavigate(page){
    await clearAndCloseReader();
    navigateTo(page);
}

function navigateTo(page){
    page = page.toString() || "";
    switch(page.toLowerCase()){
        case "home":
            gotoHome();
            break;
        case "archivedtests":
            gotoArchivedTests();
            break;
        case "qclogs":
            gotoQCLogs();
            break;
        case "utilitieslanding":
            gotoUtilitiesLanding();
            break;
        case "userconfiguration":
            gotoUserConfiguration();
            break;
        case "qccalibration":
            gotoQCCalibration();
            break;
        case "qcconstancy":
            gotoQCConstancy();
            break;
        case "qcefficiency":
            gotoQCEfficiency();
            break;
        case "qclinearity":
            gotoQCLinearity();
            break;
        case "qclanding":
            gotoQCLanding();
            break;
        case "spa":
            gotoSPA();
            break;
        case "manualcalc":
            gotoManualCalc();
            break;
        case "detectorsetup":
            gotoDetectorSetup();
            break;
        case "login":
            gotoLogin();
            break;
        default:
            alert('Invalid navigation');
            break;
    }
}