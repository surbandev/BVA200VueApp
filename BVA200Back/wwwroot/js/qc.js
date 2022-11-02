const detectorEfficiencyIdeal = 0.11;
const efficiencyPctThresh = 0.7;
const linearityThresh = 0.1;
const nanocuriesToCPM = 2200;
const i131HalfLifeDays = 8.0197;
const cesiumHalfLifeDays = 30.05 * 365.24;
const bariumHalfLifeDays = 10.551 * 365.24;
const minEfficiencyThresh = 0.07;
const constancyShortTimeBackgroundThresh = 0.5;
const constancyLongtermThresh = 0.05;
const constancyShorttermStandardThresh = 0.1;

function efficiencyCheck(cpm, activityNci, thresh) {
    thresh = thresh || minEfficiencyThresh;

    let efficiency = actualEfficiency(cpm, activityNci)
    if (efficiency >= thresh) {
        console.log("Measured efficiency of", efficiency, "meets threshold of", pctf(thresh));
        return true;
    }
    else {
        console.log("Measured efficiency of", efficiency, "is below threshold of", pctf(thresh));
        return false;
    }
}

function constancyCheck(measured, time_of_measurement, halflife_days, thresh) {
    let n = measured.length;
    if (n != time_of_measurement.length) {
        throw ("Measured and time_of_measurement must agree")
    }

    var tTimeOfMeasurement = time_of_measurement;
    if (Array.isArray(time_of_measurement)) {
        tTimeOfMeasurement = time_of_measurement[0];
    }

    let toTime = [];
    for (var i = 0; i < n; i++) {
        toTime.push(tTimeOfMeasurement);
    }

    let adjusted = decayAdjust(measured, halflife_days, time_of_measurement, toTime);

    let expected = [];
    for (var i = 0; i < n; i++) {
        expected.push(adjusted[0]);
    }

    let out = linearityCheck(adjusted, expected, thresh, "Time");
    console.log('OUT', out);
    console.log('Measured', measured);
    console.log('Adjusted', adjusted);
    return (out)
}

function linearityCheck(measured, expected, thresh, id) {
    thresh = thresh || 0.1;
    id = id || "Sample";

    let n = measured.length;//measured appears to be an array?
    if (n < 2) {
        throw ('Must have at least 2 requirements');
    }
    if (expected.length != n) {
        throw ('Unexpected number of measurements');
    }
    let N = n * (n - 1) / 2;
    let ratios = [];
    for (var i = 0; i < N; i++) {
        ratios.push(null);
    }
    let predictedRatios = [];
    let agreement = [];
    let results = [];
    let testNames = [];
    let resultText = [];

    for (var i = 0; i < (n - 1); i++) {
        for (var j = i + 1; j < n; j++) {
            testNames.push(id + "_" + i + "_to_" + j);
            var ratio = measured[i] / measured[j];
            var predictedRatio = expected[i] / expected[j];
            var tAgreement = absdev(ratio, predictedRatio);
            ratios.push(ratio);
            predictedRatios.push(predictedRatio);
            agreement.push(tAgreement);
            var good = tAgreement < thresh;
            results.push(good);
            if (good)
                resultText.push("PASSED: deviation from expected ratio of " + pctf(tAgreement) + " is within threshold of " + pctf(thresh))
            else
                resultText.push("FAILED: deviation from expected ratio of " + pctf(tAgreement) + " is greater than threshold of " + pctf(thresh))
        }
    }
    //results array should be boolean
    let overallResult = !results.includes(false);
    let maxDev = Math.max(...agreement);
    let overallResultText = "";
    if (overallResult) {
        overallResultText = "PASSED: maximum deviation from expected ratio of " + pctf(maxDev) + " is within threshold of " + pctf(thresh);
    } else {
        overallResultText = "FAILED: maximum deviation from expected ratio of " + pctf(maxDev) + " is greater than threshold of " + pctf(thresh);
    }

    return overallResultText;//Yeah, deviates from J's code but for the purpose of getting some output...
}

function expectedCPM(activityNci, efficiency) {
    efficiency = efficiency || detectorEfficiencyIdeal;
    var returnArray = [];
    for (var i = 0; i < activityNci.length; i++) {
        var res = activityNci[i] * efficiency * nanocuriesToCPM;
        returnArray.push(res);
    }

    return returnArray;
}

function absdev(measured, expected) {
    return Math.abs(pctdev(measured, expected));
}

function pctdev(measured, expected) {
    return (measured - expected) / expected;
}

function decayAdjust(measured, halfLifeDays, fromTime, toTime) {
    var returnArray = [];
    for (var i = 0; i < measured.length; i++) {
        var tFromTime = fromTime;
        var tToTime = toTime;
        if (Array.isArray(fromTime)) {
            tFromTime = fromTime[i];
        }
        if (Array.isArray(toTime)) {
            tToTime = tToTime[i];
        }
        var m = measured[i];
        var t = m * (0.5 ** decayHalfLives(halfLifeDays, tFromTime, tToTime));
        returnArray.push(t);
    }
    return returnArray;
}

function decayHalfLives(halflifeDays, fromTime, toTime) {
    var ft = new Date(fromTime).getTime();
    var tt = new Date(toTime).getTime();
    var delta = (ft - tt) / 86400000; //millisecs to days
    return Number(delta / halflifeDays);
}

function actualEfficiency(cpm, activity_nci) {
    if (!Array.isArray(cpm)) {
        var res = cpm / (activity_nci * nanocuriesToCPM);
        return pctf(res);
    } else {
        let rArray = [];
        for (var i = 0; i < cpm.length; i++) {
            var res = cpm[i] / (activity_nci[i] * nanocuriesToCPM);
            rArray.push(pctf(res));
        }
        return rArray;
    }
}

function inferredActivity(cpm, efficiency) {
    if (efficiency) {
        efficiency = detectorEfficiencyIdeal;
    }
    return cpm / (efficiency * nanocuriesToCPM);
}

function pctf(x, digits, nsmall) {
    digits = digits || 1;
    nsmall = nsmall || 1;

    return ((x * 100).toFixed(digits) + "%");
}

function hasName(x) {
    //Yeah, I don't know about this one.
    /*
      x.n <- names(x)
      if (is.null(x.n))
            x.n <- rep("",length(x))
      out <- (nchar(x.n)>0)
      return(out)
      */
}

function listn(...params) {
    /*
        This appears to be a function to just print stuff.
        Leaving as a stub for now.
    */
    /*
        lis.names <- sapply(as.list(substitute(list(...)))[-1L],as.character)
        out <- list(...)
        # figure out if we have any names
        i.need <- !has.name(out)
        names(out)[i.need] <- lis.names[i.need]
        return(out)
    */
}

function test() {
    var standardActivityNci = [20, 10, 5];
    var goodStandardCPM = [4378, 2208, 1143];
    var badStandardCPM = [4400, 2630, 1193];
    var toc = new Date("2022-10-01 10:03:43");
    var testDate = new Date("2022-10-18 08:32:32");
    var short_times = ["2022-10-18 08:32:32", "2022-10-18 08:33:32", "2022-10-18 08:34:32", "2022-10-18 08:35:32", "2022-10-18 08:36:32"];
    var bad_short_term__background_counts = [45, 60, 37, 67, 50];
    var good_short_term__background_counts = [45, 50, 37, 57, 50];
    var eff_check_activity_nCi = 20
    var good_eff_check_cpm = 5000
    var bad_eff_check_cpm = 3000


    var badTest = linearityCheck(badStandardCPM, standardActivityNci);
    var goodTest = linearityCheck(goodStandardCPM, standardActivityNci);

    var goodEfficiencyActual = actualEfficiency(goodStandardCPM, standardActivityNci);
    var badEfficiencyActual = actualEfficiency(badStandardCPM, standardActivityNci);
    var expectedEfficiency = expectedCPM(standardActivityNci);

    var decayHL = decayHalfLives(i131HalfLifeDays, toc, testDate);
    var decayAdjust1 = decayAdjust(standardActivityNci, i131HalfLifeDays, toc, testDate);
    var decayAdjust2 = decayAdjust(standardActivityNci, i131HalfLifeDays, testDate, toc);

    var goodEfficiency = efficiencyCheck(good_eff_check_cpm, eff_check_activity_nCi, minEfficiencyThresh);
    var badEfficiency = efficiencyCheck(bad_eff_check_cpm, eff_check_activity_nCi, minEfficiencyThresh);

    var goodConstancy = constancyCheck(good_short_term__background_counts, short_times, i131HalfLifeDays, constancyShortTimeBackgroundThresh);
    var badConstancy = constancyCheck(bad_short_term__background_counts, short_times, i131HalfLifeDays, constancyShortTimeBackgroundThresh);

    console.log('badTest', badTest);
    console.log('goodTest', goodTest);
    console.log('goodEfficiencyActual', goodEfficiencyActual);
    console.log('badEfficiencyActual', badEfficiencyActual);
    console.log('expectedEfficiency', expectedEfficiency);
    console.log('decayHL', decayHL);
    console.log('decayAdjust1', decayAdjust1);
    console.log('decayAdjust2', decayAdjust2);
    console.log('goodEfficiency', goodEfficiency);
    console.log('badEfficiency', badEfficiency);
    console.log('goodConstancy', goodConstancy);
    console.log('badConstancy', badConstancy);
}

test();