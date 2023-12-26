import MediaPlayer from "../../interfaces/MediaPlayer";
import { VideoLoaderFactory } from "./loaders";
import { VideoState } from "./models";
import { VideoOptions } from "./options";
import { VideoJsPlayer, VideoJsPlayerOption } from "interfaces/VideoJs";

export default class VideoPlayer implements MediaPlayer {

    videoStates: VideoState[] = [];

    element: HTMLDivElement = document.createElement("div");
    playerElement: HTMLVideoElement = document.createElement("video");
    messageBox: HTMLDivElement = document.createElement("div");
    videoJsPlayer?: VideoJsPlayer;

    currentVideoState?: VideoState;

    onTriggerSwitch: () => Promise<void> = async () => {};
    onSwitchCompleted: () => Promise<void> = async () => {};
    onTimeUpdate: () => Promise<void> = async () => {};

    get playing(): boolean { return !!this.videoJsPlayer && !this.videoJsPlayer.paused(); }
    get time(): number { return !!this.videoJsPlayer ? this.videoJsPlayer.currentTime() : 0; }

    private static _videoLoaderFactory: VideoLoaderFactory = new VideoLoaderFactory();
    private static _videoStateMessageOptions = {
        "empty": { message: "尚未載入" },
        "checking": { message: "檢查中" },
        "checked": { message: "" },
        "error": { message: "發生錯誤" }
    };

    constructor() {
        if (!(<any>window).videoPlayerId) {
            (<any>window).videoPlayerId = 0;
        }

        this._build();
        this._bindEvents();
    }

    async loadAsync(options: VideoOptions | undefined): Promise<{ success: boolean, message?: string }> {
        this.unload();

        if (!options) {
            return { success: false, message: "必須提供 options" };
        }

        const checkResult = this._checkVideoOptions(options);
        if (!checkResult.success) {
            return checkResult;
        }
        
        const videoLoader = VideoPlayer._videoLoaderFactory.getVideoLoader(options);
        if (!videoLoader) {
            return { success: false, message: "source 格式錯誤" };
        }

        const videoState = await videoLoader.loadAsync(options);
        this.videoStates.push(videoState);

        if (videoState.state !== "error") {
            return { success: true };
        } else {
            this._disposeJsPlayer();
            return { success: false, message: videoState.error };
        }
    }

    unload(): void {
        if (this.currentVideoState) {
            this.currentVideoState.onCompleted = undefined;
            this.currentVideoState = undefined;
            this._clearVideoSource();
        }

        this._update();
    }

    isLoaded(name: string): boolean {
        return this.videoStates.filter(o => o.name === name).length > 0;
    }

    dispose(): void {
        this._disposeJsPlayer();
    }

    async switchToFirst(): Promise<void> {
        if (this.videoStates.length > 0) {
            await this.switchToAsync(this.videoStates[0].name);
        }
    }

    async switchToAsync(index: number): Promise<void>;
    async switchToAsync(name: string): Promise<void>;
    async switchToAsync(target: number | string): Promise<void> {
        let videoState: VideoState | undefined = undefined;
        if (typeof target === "number") {
            videoState = this.videoStates[target];
        } else {
            const matchs = this.videoStates.filter(o => o.name === target);
            videoState = matchs.length > 0 ? matchs[0] : undefined;
        }

        await this.onTriggerSwitch();
        this.unload();
        
        if (!videoState) {
            return;
        }

        this.currentVideoState = videoState;
        this._update();
    }

    play(time?: number): void {
        if (!this.videoJsPlayer) {
            return;
        }

        if (time !== undefined && time !== null && time >= 0) {
            this.videoJsPlayer.currentTime(time);
        }
        
        if (!this.playing) {
            this.videoJsPlayer.play();
        }
    }

    pause(): void {
        if (this.playing && this.videoJsPlayer) {
            this.videoJsPlayer.pause();
        }
    }

    stop(): void {
        if (!this.videoJsPlayer) {
            return;
        }

        if (this.playing) {
            this.videoJsPlayer.pause();
        }
        
        this.videoJsPlayer.currentTime(0);
    }

    addSecond(seconds: number): void {
        if (this.videoJsPlayer) {
            this.videoJsPlayer.currentTime(this.videoJsPlayer.currentTime() + seconds);
        }
    }

    nextSecond(): void {
        this.addSecond(1);
    }

    prevSecond(): void {
        this.addSecond(-1);
    }

    private _build(): void {
        this.element.className = "se-video-player";
        
        this.element.appendChild(this.playerElement);
        this.playerElement.id = `se-player-${(<any>window).videoPlayerId}`;
        this.playerElement.className = "video-js";
        this.playerElement.controls = true;

        this.element.appendChild(this.messageBox);
        this.messageBox.className = "se-video-message-box";

        this._buildVideoJsPlayer();
    }

    private _bindEvents(): void {
        this.videoJsPlayer?.on("timeupdate", () => {
            this.onTimeUpdate();
        });
    }

    private _buildVideoJsPlayer(): void {
        this._disposeJsPlayer();

        const options: VideoJsPlayerOption = {
            muted: true,
            controlBar: {
                playbackRateMenuButton: false,
                pictureInPictureToggle: false,
                fullscreenToggle: false
            },
            html5: {
                hls: {
                    overrideNative: false
                },
                nativeVideoTracks: true,
                nativeAudioTracks: true,
                nativeTextTracks: true
            }
        };

        const videoJsPlayer: VideoJsPlayer = (<any>window).videojs(this.playerElement, options);

        videoJsPlayer.controlBar.currentTimeDisplay.show();
        videoJsPlayer.controlBar.durationDisplay.show();

        var nextSecondButton = (<any>videoJsPlayer).getChild("ControlBar").addChild(
            "button", 
            {
                controlText: "下一秒",
                className: "se-next-second-button",
                clickHandler: () => this.nextSecond()
            }
        );

        var prevSecondButton = (<any>videoJsPlayer).getChild("ControlBar").addChild(
            "button", 
            {
                controlText: "上一秒",
                className: "se-prev-second-button",
                clickHandler: () => this.prevSecond()
            }
        );

        const prevSecondButtonElement = <HTMLButtonElement>prevSecondButton.el();
        const nextSecondButtonElement = <HTMLButtonElement>nextSecondButton.el();
        const skipButton = nextSecondButtonElement.parentElement?.querySelector(".vjs-skip-backward-undefined");
        
        if (prevSecondButtonElement.parentElement && skipButton) {
            prevSecondButtonElement.parentElement.insertBefore(prevSecondButtonElement, skipButton);
            nextSecondButtonElement.parentElement!.insertBefore(nextSecondButtonElement, skipButton);
        }

        this.videoJsPlayer = videoJsPlayer;
    }

    private _disposeJsPlayer(): void {
        if (this.videoJsPlayer) {
            this.videoJsPlayer.dispose();
        }
    }

    private _checkVideoOptions(options: VideoOptions): { success: boolean, message?: string } {
        if (!options.name) {
            return { success: false, message: "必須提供 name" };
        }

        if (this.isLoaded(options.name)) {
            return { success: false, message: "已經包含相同 name 的音檔" };
        }

        if (!options.source) {
            return { success: false, message: "必須提供 source" };
        }

        return { success: true };
    }

    private _update(): void {
        if (this.currentVideoState) {
            const { message } = VideoPlayer._videoStateMessageOptions[this.currentVideoState.state];
            if (!message) {
                this.messageBox.classList.remove("show");
            } else {
                this.messageBox.classList.add("show");
            }
            
            if (this.currentVideoState.state === "checking") {
                this.messageBox.innerHTML = message;
                this.currentVideoState.onCompleted = () => this._update();
            } else if (this.currentVideoState.state === "error") {
                this.messageBox.innerHTML = `${message}：${this.currentVideoState.error}`;
            } else if (this.currentVideoState.state === "checked") {
                this._appendVideoSource();
                this.onSwitchCompleted();
            }
        } else {
            this.messageBox.innerHTML = "尚未載入音檔";
            this.messageBox.classList.add("show");
        }
    }

    private _appendVideoSource(): void {
        if (!this.currentVideoState) {
            return;
        }
        
        if (this.videoJsPlayer) {
            this.videoJsPlayer.src({ src: this.currentVideoState.url, type: this.currentVideoState.type });
        }
    }

    private _clearVideoSource(): void {
        this._buildVideoJsPlayer();
        this._bindEvents();
    }
}