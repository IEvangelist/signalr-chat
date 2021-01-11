window.app = {
    speak: (message, defaultVoice, voiceSpeed, lang) => {
        const utterance = new SpeechSynthesisUtterance(message);
        const voices = window.speechSynthesis.getVoices();
        try {
            utterance.voice =
                !!defaultVoice && defaultVoice !== 'Auto'
                    ? voices.find(v => v.name === defaultVoice)
                    : voices.find(v => !!lang && v.lang.startsWith(lang) || v.name === 'Google US English') || voices[0];
        } catch { }
        utterance.volume = 1;
        utterance.rate = voiceSpeed || 1;

        window.speechSynthesis.speak(utterance);
    },
    notify: (title, message) => {
        if (toastr) {
            toastr.info(message, title);
        }
    },
    updateScroll: () => {
        const element = document.querySelector('html');
        element.scrollTop = element.scrollHeight;
    },
    focus: (id) => {
        const element = document.getElementById(id);
        if (element) {
            element.focus();
        }
    },
    getClientVoices: dotnetObj => {
        let voices = speechSynthesis.getVoices();
        if (!voices || !voices.length) {
            speechSynthesis.onvoiceschanged =
                dotnetObj.invokeMethodAsync(
                    "UpdateClientVoices", voices = speechSynthesis.getVoices());
        }
        return voices;
    }
};

if (toastr) {
    toastr.options = {
        toastClass: 'info',
        iconClasses: {
            info: 'alert alert-info'
        },
        newestOnTop: true,
        positionClass: 'toast-top-center',
        preventDuplicates: true
    };
}

// Prevent bots from speaking when user closes tab or window.
window.addEventListener('beforeunload', _ => {
    if (window.speechSynthesis && window.speechSynthesis.pending === true) {
        window.speechSynthesis.cancel();
    }
});