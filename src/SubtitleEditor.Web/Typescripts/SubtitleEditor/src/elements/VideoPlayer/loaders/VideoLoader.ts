import { VideoState } from "../models";
import { VideoOptions } from "../options";

export default interface VideoLoader {
    isMatch(options: VideoOptions): boolean;
    loadAsync(options: VideoOptions): Promise<VideoState>;
}