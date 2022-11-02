var battery = undefined;
var alertLevels = [0, 5, 10, 15, 25, 50];

async function initBatteryMonitor() {
    battery = await getBattery();
    if (!battery){
        return;
    }
    battery.onchargingchange = function () {
        batteryChargingStatusChanged();
    }
    battery.onlevelchange = function () {
        batteryLevelChanged();
    }
    battery.onchargingtimechange = function () {
        batteryChargingTimeChanged();
    }
    battery.ondischargingtimechange = function () {
        batteryDischargingTimeChanged();
    }
    $('#batteryLevel').on('click', (async () => {
        showBatteryInfo();
    }));

    //Despite having listeners, we want to set the initial class for the battery
    batteryLevelChanged();
}

function getBattery() {
    return new Promise((resolve, reject) => {
        navigator.getBattery().then(function (b) {
            resolve(b);
        }).catch((err) => {
            //battery not supported in browser
        });
    })
}

function getBatteryLevel() {
    if (!battery){
        return null;
    }
    return Number((battery.level * 100).toFixed(0));
}

function getBatteryChargingTime() {
    if (!battery){
        return null;
    }
    return battery.chargingTime;//seconds
}

function getBatteryDischargingTime() {
    if (!battery){
        return null;
    }
    return battery.dischargingTime;//seconds
}

function isBatteryCharging() {
    if (!battery){
        return null;
    }
    return battery.charging;
}

function getBatteryTimeRemaining(style) {
    if (!battery){
        return null;
    }
    var chargingTime = getBatteryChargingTime();
    var dischargingTime = getBatteryDischargingTime();
    var chargingStatus = isBatteryCharging();
    var time = chargingStatus === true ? chargingTime : dischargingTime;//seconds

    var hours = Number((time / (60 ** 2)).toFixed(0));
    if (hours > 1) {
        time = time - (hours * (60 ** 2));
    } 

    var minutes = Number((time / 60).toFixed(0));

    switch (style){
        case 1:
            if (hours < 10) {
                hours = "0" + hours;
            }
            if (minutes < 10) {
                minutes = "0" + minutes;
            }
            time = hours + ":" + minutes;
            break;
        case 2:
            time = hours + " hours, " + minutes + " minutes";
            break;
        default:
            //return seconds
            break;
    }
    return time;
}

async function batteryLevelChanged() {
    var batteryLevel = await getBatteryLevel();
    var classesToRemove = [
        'fa-battery-empty',
        'fa-battery-quarter',
        'fa-battery-half',
        'fa-battery-three-quarters',
        'fa-battery-full',
    ];

    //clear out any existing classes that pertain to battery
    for (var c of classesToRemove) {
        $("#batteryLevel").removeClass(c);
    }
    if (batteryLevel === 0) {
        $("#batteryLevel").addClass('fa-battery-empty').css("color", "red");
    }
    if (_.inRange(batteryLevel, 1, 26)) {
        $("#batteryLevel").addClass('fa-battery-quarter').css("color", "red");
    }
    if (_.inRange(batteryLevel, 26, 51)) {
        $("#batteryLevel").addClass('fa-battery-half').css("color", "orange");
    }
    if (_.inRange(batteryLevel, 51, 76)) {
        $("#batteryLevel").addClass('fa-battery-three-quarters').css("color", "yellow");
    }
    if (_.inRange(batteryLevel, 76, 95)) {
        $("#batteryLevel").addClass('fa-battery-three-quarters').css("color", "green");
    }
    if (_.inRange(batteryLevel, 95, 101)) {
        $("#batteryLevel").addClass('fa-battery-full').css("color", "green");
    }

    //specific warnings

    if (_.includes(alertLevels, batteryLevel)) {
        showBatteryInfo();
    }

    console.log("Battery level: "
        + batteryLevel + "%");
}

function batteryChargingStatusChanged() {
    console.log('battery', battery)
    showBatteryInfo();
}

function batteryChargingTimeChanged() {
    //placeholder in case we need to tap into it later
}

function batteryDischargingTimeChanged() {
    //placeholder in case we need to tap into it later
}

function showBatteryInfo() {
    var batteryLevel = getBatteryLevel();
    var chargingStatus = isBatteryCharging();

    var html = `
        <p>
            {{alert}}
            {{chargingStatus}}<br />
            Battery Level: {{batteryLevel}}%<br />
            {{timeString}}<br />
        </p>
    `;
    html = html.replace(/{{chargingStatus}}/g, chargingStatus === true ? "Plugged In" : "Not Plugged In");
    html = html.replace(/{{batteryLevel}}/g, batteryLevel);
    if (batteryLevel < 100) {
        var time = getBatteryTimeRemaining(1);
        html = html.replace(/{{time}}/g, time);
        if (chargingStatus === true) {
            html = html.replace(/{{timeString}}/g, "Est. time until charged: " + time);
        } else {
            html = html.replace(/{{timeString}}/g, "Est. Battery time remaining: " + time);
        }
    } else {
        html = html.replace(/{{timeString}}/g, "Fully Charged");
    }

    var alert = "";
    //var alertLevels = [0,5,10,15,25,50];
    if (batteryLevel === 0) {
        alert = `BATTERY DEPLETED!<br />
       PLUG IN YOUR CHARGER ASAP!`;
    }
    if (_.inRange(batteryLevel, 1, 6)) {
        alert = `BATTERY CRITICALLY LOW!<br />
        PLUG IN YOUR CHARGER ASAP!`;
    }
    if (_.inRange(batteryLevel, 6, 16)) {
        alert = `BATTERY DANGEROUSLY LOW!<br />
        PLUG IN YOUR CHARGER ASAP!`;
    }
    if (_.inRange(batteryLevel, 16, 26)) {
        alert = `Battery is getting low.<br />
        Plug in your charger soon.`;
    }
    if (_.inRange(batteryLevel, 26, 51)) {
        alert = `Battery has less than 50% remaining.<br />
        Consider plugging in your charger soon.`;
    }
    //anything over 50, I don't think we need to add an alert string

    html = html.replace(/{{alert}}/g, alert + "</br />");
    showInfo(html, 'Battery Info');
}