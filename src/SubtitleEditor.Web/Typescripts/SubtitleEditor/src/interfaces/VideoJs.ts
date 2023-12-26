
export interface VideoJsPlayerOption {
    autoplay?: boolean | "muted" | "play" | "any";
    controls?: boolean;
    height?: string | number;
    width?: string | number;
    loop?: boolean;
    muted?: boolean;
    /** A URL to an image that displays before the video begins playing. This is often a frame of the video or a custom title screen. As soon as the user hits "play" the image will go away. */
    poster?: string;
    preload?: "auto" | "metadata" | "none";
    /** Puts the player in fluid mode and the value is used when calculating the dynamic size of the player. The value should represent a ratio - two numbers separated by a colon (e.g. "16:9" or "4:3"). */
    aspectRatio?: string;
    audioOnlyMode?: boolean;
    audioPosterMode?: boolean;
    /** When true, the Video.js player will have a fluid size. In other words, it will scale to fit its container at the video's intrinsic aspect ratio, or at a specified aspectRatio. */
    fluid?: boolean;
    /** The inactivityTimeout determines how many milliseconds of inactivity is required before declaring the user inactive. A value of 0 indicates that there is no inactivityTimeout and the user will never be considered inactive. */
    inactivityTimeout?: number;
    /** Allows overriding the default message that is displayed when Video.js cannot play back a media source. */
    notSupportedMessage?: string;
    /** Control whether UI elements have a title attribute. A title attribute is shown on mouse hover, which can be helpful for usability, but has drawbacks for accessibility. 
     * Setting noUITitleAttributes to true prevents the title attribute from being added to UI elements, allowing for more accessible tooltips to be added to controls by a plugin or external framework. */
    noUITitleAttributes?: boolean;
    sources?: VideoJsSource[];
    userActions?: {
        click?: boolean | undefined | Function;
        doubleClick?: boolean | undefined | Function;
        hotkeys?: boolean | undefined | Function;
    };
    controlBar?: any;
    html5?: any;
}

export interface VideoJsSource {
    type: string;
    src: string;
}

export interface VideoJsTimeRange {
    /** number of different ranges of time have been buffered. */
    length: number;
    /** Time in seconds when the first range starts. */
    start(rangeIndex: number): number;
    /** Time in seconds when the first range ends */
    end(rangeIndex: number): number;
}

export interface VideoJsPlayer {
    on(event: "ready" | "ended" | "timeupdate" | "dispose" | "seeked" | "progress" | "durationchange", handler: () => void): void;
    dispose(): void;
    ready(handler: () => void): void;
    volumn(): number;
    volumn(newVolumn: number): void;
    muted(): boolean;
    muted(mute: boolean): void;
    /** to start playback on a player that has a source. */
    play(): void;
    /** to pause playback on a player that is playing. */
    pause(): void;
    /** determine if a player is currently paused. */
    paused(): boolean;
    /** give you the currentTime (in seconds) that playback is currently occuring at. */
    currentTime(): number;
    currentTime(newTime: number): void;
    /** give you the total duration of the video that is playing */
    duration(): number;
    /** give you the seconds that are remaing in the video. */
    remainingTime(): number;
    /** give you a timeRange object representing the current ranges of time that are ready to be played at a future time. */
    buffered(): VideoJsTimeRange;
    /** give you the current percentage of the video that is buffered. */
    bufferedPercent(): number;
    src(source: VideoJsSource): void;
    src(sources: VideoJsSource[]): void;
    poster(): string;
    poster(url: string): void;
    controlBar: any;
}