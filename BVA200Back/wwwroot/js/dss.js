function sendBVAResultsToDSS(timestamp, patient, bvaResults) {
    return new Promise(async (resolve, reject) => {
        try {
            let dssBaseURL = await getSetting('dss_api_url');
            let dssApiKey = await getSetting('dss_api_key');
            let url = dssBaseURL + 'SaveBVAResults';

            let payload = {
                timestamp,
                patient,
                bvaResults
            };

            let config = {
                headers: {
                    apikey: dssApiKey
                }
            }

            axios.post(url, payload, config).then(() => {
                resolve(true);
            }).catch((err) => {
                console.error('Failed to get setting', err);
                if (err.response.status === 404) {
                    reject('Setting not found');
                } else {
                    reject(err.response.statusMessage);
                }
            });

        } catch (err) {
            throw(err);
        }
    });
}