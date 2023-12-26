import * as signalR from "@microsoft/signalr";
import sessionApiMap from "contexts/sessionApiMap";

class SessionController {

    private _connection: signalR.HubConnection;

    constructor() {
        this._connection = new signalR.HubConnectionBuilder()
            .withUrl(sessionApiMap.hub.url)
                .configureLogging(signalR.LogLevel.Warning)
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: retryContext => {
                        if (retryContext.elapsedMilliseconds < 60000) {
                            return 3000;
                        } else if (retryContext.elapsedMilliseconds < 3600000) {
                            return 60000;
                        } else if (retryContext.elapsedMilliseconds < 259200000) {
                            return 3600000;
                        } else {
                            return null;
                        }
                    }
                })
                .build();

        this._bindSignalrEvents();
    }

    public async startAsync(): Promise<void> {
        try {
            await this._connection.start();
            this._onStart();
        } catch (reason) {
            console.warn(reason);
        }
    }

    private _bindSignalrEvents(): void {
        this._connection.onreconnecting(() => this._onReconnecting());
        this._connection.onreconnected(() => this._onReconnected());
        this._connection.onclose(error => this._onClosed(error));

        this._connection.on("Relogin", () => this._onReloginReceived());
        this._connection.on("Logout", () => this._onLogoutReceived());
    }

    private _onStart(): void {
        console.log("已連線到伺服器。");
    }

    private _onReconnecting(): void {
        console.warn("連線中斷，正在重新連線中！");
    }

    private _onReconnected(): void {
        console.log("已重新連線到伺服器。");
    }

    private _onClosed(error?: any): void {
        console.warn("連線失敗，請重新登入系統。");
        if (error && error.message.indexOf("HubException: 401") >= 0) {
            window.location.pathname = "/account/logout";
        }
    }

    private _onReloginReceived(): void {
        console.warn("收到來自伺服器的全面登出要求！");
        window.location.pathname = "/";
    }

    private _onLogoutReceived(): void {
        console.warn("收到來自伺服器的登出要求！");
        window.location.pathname = "/account/logout";
    }
}

const sessionController = new SessionController();
export default sessionController;