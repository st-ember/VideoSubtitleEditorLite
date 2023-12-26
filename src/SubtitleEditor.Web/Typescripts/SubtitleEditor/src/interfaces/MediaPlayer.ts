import MediaOptions from "./MediaOptions";

export default interface MediaPlayer {
    element: HTMLDivElement;
    onTriggerSwitch: () => Promise<void>;
    onSwitchCompleted: () => Promise<void>;
    onTimeUpdate: () => Promise<void>;

    get playing(): boolean;
    get time(): number;

    loadAsync(options: MediaOptions | undefined): Promise<{ success: boolean, message?: string }>
    unload(): void;
    dispose(): void;
    isLoaded(name: string): boolean;
    switchToFirst(): Promise<void>;
    switchToAsync(index: number): Promise<void>;
    switchToAsync(name: string): Promise<void>;
    play(time?: number): void;
    pause(): void;
    stop(): void;
}