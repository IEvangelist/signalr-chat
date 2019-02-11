(function () {
    var connection =
        new signalR.HubConnectionBuilder()
            .withUrl('/chat')
            .build();

    var app = new Vue({
        el: '#chat',
        data: {
            messageId: null,
            message: '',
            messages: new Map()
        },
        methods: {
            postMessage: function () {
                if (this.message) {
                    connection.invoke('postMessage', this.message, this.messageId);
                    this.message = this.messageId = '';
                }                
            },
            toArray(messages) {
                return Array.from(messages);
            },
            nudge() {
                this.$forceUpdate();
            }
        }
    });

    connection.on('messageReceived', json => {
        app.messages.set(json.id, json);
        app.nudge();
        updateScroll();
        setTimeout(updateScroll);
    });

    // Reconnect loop
    const start = () => {
        connection.start().catch(err => {
            setTimeout(() => start(), 5000);
        });
    };

    connection.onclose(() => start());

    start();

    var updateScroll = function () {
        var element = document.querySelector("html");
        element.scrollTop = element.scrollHeight;
    };
})();