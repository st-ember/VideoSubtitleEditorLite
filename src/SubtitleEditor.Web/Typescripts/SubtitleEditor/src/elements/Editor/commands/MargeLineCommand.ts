import { dialogController } from "DialogController";
import { Line } from "../components";
import { WordSegment } from "../models";
import MargeLineHistory from "../histories/MargeLineHistory";
import Editor from "../Editor";
import EditLineCommand from "./EditLineCommand";
import SetCaretToEndCommand from "./SetCaretToEndCommand";

/** 將輸入的字幕陣列所代表的字幕合併；或將編輯器內已勾選的字幕合併。合併時一律保留並編輯第一句，刪除剩餘句子。此指令會建立 MargeLineHistory。 */
export default class MargeLineCommand {

    static async checkAsync(editor: Editor): Promise<boolean> {
        return !!editor.currentSubtitle && editor.currentSubtitle.selectedLines.length > 1;
    }

    static async invokeAsync(editor: Editor): Promise<boolean>;
    static async invokeAsync(lines: Line[]): Promise<boolean>;
    static async invokeAsync(input: Editor | Line[]): Promise<boolean> {
        const lines = input instanceof Editor ? input.currentSubtitle?.lines.filter(line => line.selected) : input;
        if (!lines || lines.length < 2) {
            return false;
        }

        const adoptedLines = lines.sort((a, b) => a.index - b.index);
        let index = adoptedLines[0].index;
        let uncontinue = false;

        for (let i = 0; i < adoptedLines.length; i++) {
            const line = adoptedLines[i];
            if (line.index !== index) {
                uncontinue = true;
                break;
            }

            index++;
        }

        if (uncontinue && !await dialogController.confirmWarningAsync("確認", "選擇的字幕並未連續，合併時會將夾在中間的字幕一起合併，是否繼續進行？")) {
            return false;
        }

        const minIndex = lines[0].index;
        const maxIndex = lines[lines.length - 1].index;
        const targetLines = lines[0].subtitle.lines.filter(line => line.index >= minIndex && line.index <= maxIndex);
        const lineStates = targetLines.map(line => line.datas);
        const lineDatas = targetLines.map(line => line.data);

        await MargeLineCommand.doMargeAsync(targetLines);

        lines[0].subtitle.addHistoryAsync(new MargeLineHistory(lines[0].subtitle, { index: minIndex, lineStates, lineDatas }));

        return true;
    }

    static async doMargeAsync(lines: Line[]): Promise<void> {
        const targetLine = lines[0];
        const editing = !!targetLine.subtitle.editor.editingLine;
        const margedData = { ...targetLine.data };
        margedData.end = lines[lines.length - 1].data.end;
        
        if (!!margedData.wordSegments && margedData.wordSegments.length > 0) {
            const segments: WordSegment[] = [];
            lines.forEach(line => {
                if (line.data.wordSegments) {
                    line.data.wordSegments.forEach(segment => segments.push(segment));
                };
            });

            margedData.wordSegments = segments;
            margedData.content = segments.map(o => o.word).join("");
        } else {
            margedData.content = lines.map(o => o.data.content).join("");
        }

        targetLine.subtitle.setShowAnimation(false);
        for (let i = 0; i < lines.length; i++) {
            if (i > 0) {
                await lines[i].subtitle.deleteAsync(lines[i]);
            }
        }

        targetLine.subtitle.setShowAnimation(true);
        await targetLine.applyAsync(margedData);

        if (editing) {
            await EditLineCommand.invokeAsync(targetLine);
            await SetCaretToEndCommand.invokeAsync(targetLine.subtitle.editor);
        }
    }
}