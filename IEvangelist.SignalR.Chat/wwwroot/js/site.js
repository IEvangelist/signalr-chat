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
                connection.invoke('postMessage', this.message, this.messageId);
                this.message = '';
            },
            toArray(map) {
                const result = Array.from(map);
                return result;
            },
            nudge() {
                this.$forceUpdate();
            }
        }
    });

    connection.on('messageReceived', function (json) {
        app.messages.set(json.id, json);
        app.nudge();
    });

    connection.start();
})();