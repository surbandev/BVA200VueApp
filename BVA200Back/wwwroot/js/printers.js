function getListOfPrinters() {
    return new Promise((resolve, reject) => {
        axios.get(baseURL + 'Utilities/GetListOfPrinters').then((response) => {
            resolve(response.data);
        }).catch((err) => {
            console.error('Failed to call obtain list of printers', err);
            reject(err.response.statusMessage);
        });
    });
}

