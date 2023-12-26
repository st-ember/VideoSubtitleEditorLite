import LineData from "./LineData";
import ModifiedState from "./ModifiedState";

export default interface SubtitleData {
    header?: string;
    lines: LineData[];
    srt: string;
    modifiedStates: ModifiedState[];
}