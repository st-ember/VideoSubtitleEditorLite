import LineData from "./LineData";
import LineDataState from "./LineDataState";

export default interface ActionData {
    action: string;
}

export interface ActionWithData<T> extends ActionData {
    action: string;
    data: T;
}

export interface CreateLine {
    // 完成 Line 建立後，新的 Line 一定是位在清單的最後一位，所以不用記錄 Index。
    startSpanIndex: number;
    endSpanIndex: number;
    start: string;
    end: string;
    content: string;
}

export interface InsertLine {
    /** 完成 insert 後，新的 Line 的 Index。 */
    index: number;
}

export interface DeleteLines {
    lines: {
        /** 被刪除的 Line 的 Index。 */
        index: number;
        lineState: { start: LineDataState, end: LineDataState, value: LineDataState };
        lineData: LineData;
    }[]
}

export interface UpdateLine {
    /** 被修改的 Line 的 Index。 */
    index: number;
    beforeLineData: LineData;
    afterLineData: LineData;
}

export interface UpdateLines {
    beforeLineDatas: { index: number, lineData: LineData }[];
    afterLineDatas: { index: number, lineData: LineData }[];
}

export interface ShiftLines {
    beforeLineDatas: { index: number, lineData: { start: string, end: string } }[];
    afterLineDatas: { index: number, lineData: { start: string, end: string } }[];
}

export interface SplitLine {
    /** 被分割的 Line 的 Index。 */
    index: number;
    /** 分割時的游標位置 */
    caretPosition: number;
}

export interface MargeLine {
    /** 合併的字幕範圍第一筆的 Index */
    index: number;
    lineStates: { start: LineDataState, end: LineDataState, value: LineDataState }[];
    lineDatas: LineData[];
}

export interface RecreateTime {
    transcript: string;
    lines: {
        index: number;
        lineState: { start: LineDataState, end: LineDataState, value: LineDataState };
        lineData: LineData;
    }[];
}