import Editor from "../Editor";
import { Line } from "../components";

/** 播放指定字幕時間區間；或播放字幕陣列最前到最末的時間區間；或播放編輯器已勾選項目最前到最末的時間區間；或播放編輯器正在編輯字幕的時間區間。 */
export default class PlayPeriodCommand {

    static async checkAsync(line: Line): Promise<boolean>
    static async checkAsync(lines: Line[]): Promise<boolean>
    static async checkAsync(editor: Editor): Promise<boolean>
    static async checkAsync(input: Editor | Line | Line[]): Promise<boolean> {
        if (input instanceof Line) {
            return true;
        } else if (input instanceof Array) {
            return input.length > 0;
        } else {
            return !!input.currentSubtitle && input.currentSubtitle.selectedLines.length > 0;
        }
    }

    static async invokeAsync(line: Line): Promise<void>;
    static async invokeAsync(lines: Line[]): Promise<void>;
    static async invokeAsync(editor: Editor): Promise<void>;
    static async invokeAsync(input: Editor | Line | Line[]): Promise<void> {
        if (input instanceof Line) {
            input.subtitle.editor.playPeriod(input.startTime, input.endTime);
        } else if (input instanceof Array) {
            this._playPeriod(input);
        } else if (input.currentSubtitle && input.currentSubtitle.selectedLines.length > 0) {
            this._playPeriod(input.currentSubtitle.selectedLines);
        } else if (input.editingLine) {
            input.playPeriod(input.editingLine.startTime, input.editingLine.endTime);
        }
    }

    static _playPeriod(lines: Line[]): void {
        if (lines.length === 0) {
            return;
        }

        let startTime = -1;
        let endTime = -1;

        for (let i = 0; i < lines.length; i++) {
            const line = lines[i];
            if (startTime === -1 || startTime > line.startTime) {
                startTime = line.startTime;
            }

            if (endTime === -1 || endTime < line.endTime) {
                endTime = line.endTime;
            }
        }

        if (startTime < 0) {
            startTime = 0;
        }

        if (endTime < startTime) {
            endTime = startTime;
        }

        lines[0].subtitle.editor.playPeriod(startTime, endTime);
    }
}