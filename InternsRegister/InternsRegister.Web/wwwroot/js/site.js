let connection = null;

window.signalRConnect = (url) => {
    if (connection) {
        console.warn("SignalR уже подключён");
        return;
    }

    connection = new signalR.HubConnectionBuilder()
        .withUrl(url)
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on("InternsUpdated", () => {
        DotNet.invokeMethodAsync("InternsRegister.Web", "InternsUpdated")
            .catch(err => console.error("Ошибка вызова InternsUpdated:", err));
    });

    connection.start()
        .then(() => console.log("SignalR подключён к", url))
        .catch(err => console.error("Ошибка подключения SignalR:", err));
};

window.signalRDisconnect = () => {
    if (connection) {
        connection.stop();
        connection = null;
        console.log("SignalR отключён");
    }
};