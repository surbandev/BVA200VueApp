function shortDate(d) {
    if (!d){
        d = new Date();
    }
    if (_.isString(d)) {
        d = new Date(d);
    }
    return d.toLocaleDateString()
}


function isoUTC (d) {
    if (!d){
        return undefined;
    }
    if (_.isString(d)) {
        d = new Date(d);
    }
    return d.toISOString();
}

function isoUTCNow(){
    return new Date().toISOString();
}