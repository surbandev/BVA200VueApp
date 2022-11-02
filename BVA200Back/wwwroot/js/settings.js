function getSetting(setting) {
    return new Promise((resolve, reject) => {
        let payload = {
            "Setting": setting
        };
        axios.post(baseURL + 'Utilities/GetSetting', payload).then((response) => {
            let value = _.get(response, 'Value');
            resolve(value);
        }).catch((err) => {
            console.error('Failed to get setting', err);
            if (err.response.status === 404) { 
                reject('Setting not found');
            } else {
                reject(err.response.statusMessage);
            }
        });

    });
}