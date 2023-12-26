import WordSegment from "./WordSegment";

export default interface LineData {
    format?: string;
    start: string;
    end: string;
    content: string;
    originalContent?: string;
    wordSegments?: WordSegment[];
    originalWordSegments?: WordSegment[];
}
