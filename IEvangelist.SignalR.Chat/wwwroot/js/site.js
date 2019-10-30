(function () {
    const connection =
        new signalR.HubConnectionBuilder()
            .withUrl('/chat')
            .build();

    const app = new Vue({
        el: '#chat',
        data: {
            messageId: null,
            message: '',
            messages: new Map(),
            currentUser: $('#user').val(),
            typingUsers: [],
            isTyping: false,
            emojis: ['🤣', '🤬', '🤘'],
            userLoggedOnMessage: '',
            voiceSpeed: 1
        },
        watch: {
            message: _.debounce(function () {
                this.setTyping(false);
            }, 750)
        },
        computed: {
            isFlashing: function() {
                return !!this.typingUsers.length;
            },
            usersTyping: function () {
                const length = this.typingUsers.length;
                if (length) {
                    switch (length) {
                        case 1:
                            return `💬 <strong>${this.typingUsers[0]}</strong> is typing...`;
                        case 2:
                            return `💬 <strong>${this.typingUsers[0]}</strong> and <strong>${this.typingUsers[1]}</strong> are typing...`;
                        default:
                            return '💬 Multiple people are typing...';
                    }
                }
                return '&nbsp;';
            }
        },
        methods: {
            postMessage() {
                if (this.message) {
                    connection.invoke('postMessage', this.message, this.messageId);
                    this.message = '';
                    this.messageId = null;
                }
            },
            setTyping(isTyping) {
                if (this.isTyping && isTyping) {
                    return;
                }

                connection.invoke('userTyping', this.isTyping = isTyping);
            },
            toArray(messages) {
                return Array.from(messages).slice().reverse();
            },
            nudge() {
                this.$forceUpdate();
            },
            appendEdited(json) {
                return this.isMyMessage(json.user) && json.isEdit
                    ? ' <span class="text-muted">(edited)</span>'
                    : '';
            },
            startEdit(json) {
                if (this.isMyMessage(json.user)) {
                    this.message = json.text;
                    this.messageId = json.id;
                }
                $(':text').focus();
            },
            appendToMessage(text) {
                this.message += text;
                $(':text').focus();
                this.setTyping(false);
            },
            isMyMessage(user) {
                return user === this.currentUser;
            },
            speak(message, lang) {
                const utterance = new SpeechSynthesisUtterance(message);
                const voices = window.speechSynthesis.getVoices();
                utterance.voice =
                    voices.find(v => !!lang && v.lang.startsWith(lang) || v.name === 'Google US English') || voices[0];
                utterance.volume = 1;
                utterance.rate = this.voiceSpeed || 1;

                window.speechSynthesis.speak(utterance);
            }
        }
    });

    const updateScroll = () => {
        const element = document.querySelector('html');
        element.scrollTop = element.scrollHeight;
    };

    connection.on('messageReceived',
        json => {
            app.messages.set(json.id, json);
            app.nudge();
            if (json.isEdit) {
                return;
            } else if (json.isChatBot && json.sayJoke) {
                app.speak(json.text, json.lang);
            }
            updateScroll();
            setTimeout(updateScroll);
        });

    connection.on('userTyping',
        json => {
            const index = app.typingUsers.indexOf(json.user);
            if (index === -1) {
                app.typingUsers.push(json.user);
            } else if (!json.isTyping) {
                app.typingUsers.splice(index, 1);
            }
        });

    connection.on('userLoggedOn',
        json => {
            if (json && json.user) {
                toastr.info(`${json.user} logged on...`, 'Hey!');
            }
        });

    connection.on('userLoggedOff',
        json => {
            if (json && json.user) {
                toastr.info(`${json.user} logged off...`, 'Hey!');
            }
        });

    const start = () => {
        connection.start().catch(_ => {
            setTimeout(() => start(), 5000);
        });
    };

    connection.onclose(() => start());

    start();

    toastr.options = {
        toastClass: 'info',
        iconClasses: {
            info: 'alert alert-info'
        },
        newestOnTop: true,
        positionClass: 'toast-top-center',
        preventDuplicates: true
    };

    Vue.config.errorHandler = err => {
        console.log('Exception: ', err);
    };

    // Prevent bots from speaking when user closes tab or window.
    if (window) {
        window.addEventListener('beforeunload', _ => {
            if (window.speechSynthesis && window.speechSynthesis.pending === true) {
                window.speechSynthesis.cancel();
            }
        });
    }

    $(':text').focus();
})();