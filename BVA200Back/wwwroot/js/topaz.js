function getControllerURL(){
    return baseURL + 'TopazDetector';
    //return baseURL + 'SimulatedTopazDetector';
}

function openTopazReader() {
    return new Promise((resolve,reject)=>{
        console.log('Opening Topaz Reader....')
        axios.post(getControllerURL()+'/OpenTopazReader').then(() => {
            console.log(new Date(),'Topaz Reader Opened.');
            resolve();
        }).catch((err) => {
            console.error('Failed to open Topaz Reader',err);
            if (err.response.status === 503) { //specific code that the detector isn't plugged in or usable
                reject('No Devices Found');
            } else {
                reject(err.response.statusMessage);
            }
        });
    });
};
function closeTopazReader() {
    return new Promise((resolve,reject)=>{
        console.log('Closing Topaz Reader...');
        axios.post(getControllerURL()+'/CloseTopazReader').then(() => {
            console.log(new Date(),'Topaz Reader closed.')
            resolve();
        }).catch((err) => {
            console.error('Failed to close Topaz Reader',err);
            reject(err);
        });
    })

};
function clearMemory() {
    return new Promise((resolve,reject)=>{
        console.log('Clearing Topaz Reader Memory...');
        axios.post(getControllerURL()+'/ClearMemory').then(() => {
            console.log(new Date(),'Topaz Reader Memory Cleared.');
            resolve();
        }).catch((err) => {
            console.error('Failed to clear Topaz Reader',err);
            reject(err);
        });
    })
};
function startRead() {
    return new Promise((resolve,reject)=>{
        console.log('Topaz Reader starting READ...');
        axios.post(getControllerURL()+'/StartRead').then(() => {
            console.log(new Date(),'Topaz Reader READ started.');
            resolve();
        }).catch((err) => {
            console.error('Failed to start READ',err);
            reject(err);
        });
    })
}
function stopRead() {
    return new Promise((resolve,reject)=>{
        console.log('Topaz Reader stoping READ...');
        axios.post(getControllerURL()+'/StopRead').then(() => {
            console.log(new Date(),'Topaz Reader READ stopped.');
            resolve();
        }).catch((err) => {
            console.error('Failed to stop READ',err);
            reject(err);
        });
    })
}
function readSpectrum() {
    return new Promise((resolve,reject)=>{
        console.log('Retrieving spectrum from Topaz Reader...');
        axios.post(getControllerURL()+'/ReadSpectrum').then((res) => {
            console.log(new Date(),'Spectrum received from Topaz Reader.',res);
            resolve(res.data);
        }).catch((err) => {
            console.error('Failed to retrieve spectrum from Topaz Reader',err);
            reject(err);
        });
    });
}
function updateDetectorSettings(payload) {
    return new Promise((resolve,reject)=>{
        console.log(new Date(),'Updating Topaz Reader Settings...');
        axios.post(getControllerURL()+'/SetDetectorParameters', payload).then(() => {
            console.log('Topaz Reader Settings updated.');
            resolve();
        }).catch((err) => {
            console.error('Failed top update Topaz Reader settings',err);
            reject(err);
        });
    })
}
function updateDetectorFineGain(payload) {
    return new Promise((resolve,reject)=>{
        console.log(new Date(),'Updating Topaz Reader Fine Gain...');
        axios.post(getControllerURL()+'/SetDetectorFineGain', payload).then(() => {
            console.log('Topaz Reader Fine Gain updated.');
            resolve();
        }).catch((err) => {
            console.error('Failed top update Topaz Reader Fine Gain',err);
            reject(err);
        });
    })
}
function getDetectorState(){
    return new Promise((resolve,reject)=>{
        console.log(new Date(),'Getting Topaz Reader State...');
        axios.get(getControllerURL()+'/GetDetectorState').then((res) => {
            console.log('Received Topaz Reader State.',res.data);
            resolve(res.data);
        }).catch((err) => {
            console.error('Failed to retrieve Topaz Reader State.',err);
            reject(err);
        });
    });
}
function clearAndCloseReader(){
    return new Promise((resolve,reject)=>{
        console.log(new Date(),'Clearing and Closing Topaz Reader...');
        axios.post(getControllerURL()+'/ClearAndClose').then((res) => {
            console.log('Topaz Reader Memory Cleared and Closed');
            resolve(res.data);
        }).catch((err) => {
            console.error('Failed to Clear and Close Topaz Reader.',err);
            reject(err);
        });
    });
}

async function resetReader() {
    await stopRead();
    await clearAndCloseReader();
}