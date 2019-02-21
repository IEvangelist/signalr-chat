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
            emojis: ['🤣', '🤬', '🤘']
        },
        watch: {
            message: _.debounce(function () {
                this.setTyping(false);
            }, 750)
        },
        computed: {
            usersTyping: function () {
                const length = this.typingUsers.length;
                if (length) {
                    switch (length) {
                        case 1:
                            return `// <strong>${this.typingUsers[0]}</strong> is typing...`;
                        case 2:
                            return `// <strong>${this.typingUsers[0]}</strong> and <strong>${this.typingUsers[1]}</strong> are typing...`;
                        default:
                            return '// Multiple people are typing...';
                    }
                }
                return '// ';
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
                return json.isEdit ? ' <span class="text-muted">(edited)</span>' : '';
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

    // Reconnect loop
    const start = () => {
        connection.start().catch(_ => {
            setTimeout(() => start(), 5000);
        });
    };

    connection.onclose(() => start());

    start();

    $(':text').focus();
})();