import { SubtitleData } from "SubtitleEditor";

export default interface TopicSubtitleData {
    name: string;
    filename: string;
    extension: string;
    description?: string;
    subtitle: SubtitleData;
    transcript?: string;
    frameRate?: number;
    wordLimit?: number;
}