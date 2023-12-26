import { LineData, ModifiedState } from "SubtitleEditor";

export default interface TopicUpdateSubtitleRequest {
    id: string;
    lines: LineData[];
    modifiedStates: ModifiedState[];
}