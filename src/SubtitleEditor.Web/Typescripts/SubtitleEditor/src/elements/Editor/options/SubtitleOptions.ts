import { LineData, ModifiedState } from "../models";
import { VideoOptions } from "../../VideoPlayer/options";

export default interface SubtitleOptions {
    video?: VideoOptions;
    transcript?: string;
    lines?: LineData[];
    srt?: string;
    modifiedStates?: ModifiedState[];
}
