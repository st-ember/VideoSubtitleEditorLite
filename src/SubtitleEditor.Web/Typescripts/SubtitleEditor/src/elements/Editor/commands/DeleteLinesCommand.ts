import Editor from "../Editor";
import { Line } from "../components";
import DeletesLineHistory from "../histories/DeleteLinesHistory";
import { LineData, LineDataState } from "../models";

/** 刪除指定的字幕；或刪除輸入的字幕陣列所代表的字幕；或刪除編輯器內已勾選的字幕。這個指令會建立 DeletesLineHistory。 */
export default class DeleteLinesCommand {

    static async checkAsync(editor: Editor): Promise<boolean> {
        return !!editor.currentSubtitle && editor.currentSubtitle.selectedLines.length > 0;
    }

    static async invokeAsync(editor: Editor): Promise<void>;
    static async invokeAsync(input: Line): Promise<void>;
    static async invokeAsync(inputs: Line[]): Promise<void>;
    static async invokeAsync(input: Editor | Line | Line[]): Promise<void> {
        const array = input instanceof Editor ? input.currentSubtitle?.lines.filter(line => line.selected) : input instanceof Array ? input : [input];
        if (!array || array.length === 0) {
            return;
        }

        const subtitle = array[0].subtitle;
        const datas: {
            /** 被刪除的 Line 的 Index。 */
            index: number;
            lineState: { start: LineDataState, end: LineDataState, value: LineDataState };
            lineData: LineData;
        }[] = [];

        for (let i = 0; i < array.length; i++) {
            const line = array[i];
            datas.push({ index: line.index, lineState: line.datas, lineData: line.data });
        }

        if (array.length > 1) {
            subtitle.setShowAnimation(false);
        }

        for (let i = 0; i < array.length; i++) {
            const line = array[i];
            await subtitle.deleteAsync(line);
        }

        if (array.length > 1) {
            subtitle.setShowAnimation(true);
        }

        await subtitle.addHistoryAsync(new DeletesLineHistory(subtitle, { lines: datas }));
    }
}
