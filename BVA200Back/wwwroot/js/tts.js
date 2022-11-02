function tts(stringToSpeak) {
    return new Promise((resolve, reject) => {
        let payload = {
            "StringToSpeak": stringToSpeak
        };
        axios.post(baseURL + 'Utilities/SpeakString', payload).then(() => {
            resolve();
        }).catch((err) => {
            console.error('Failed to call TTS system', err);
            reject(err.response.statusMessage);
        });

    });
}

function testTTS() {
    tts("This is a test of the Text To Speech system");
}