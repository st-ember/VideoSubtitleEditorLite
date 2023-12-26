import Editor from "../Editor";
import { Line } from "../components";

/** 從指定的字幕起始開始播放媒體；或從字幕陣列最前面的起始開始播放媒體；或從編輯器勾選項目最前面的起始開始播放媒體；或從編輯器正在編輯的字幕起始開始播放媒體；或直接開始播放媒體。 */
export default class PlayMediaCommand {

    static async checkAsync(line: Line): Promise<boolean>
    static async checkAsync(lines: Line[]): Promise<boolean>
    static async checkAsync(editor: Editor): Promise<boolean>
    static async checkAsync(input: Editor | Line | Line[]): Promise<boolean> {
        if (input instanceof Line) {
            return !input.subtitle.editor.player.playing;
        } else if (input instanceof Array) {
            return input.length > 0 && !input[0].subtitle.editor.player.playing;
        } else {
            return !input.player.playing && (!!input.currentSubtitle && input.currentSubtitle.selectedLines.length > 0 || !!input.editingLine)
        }
    }

    static async invokeAsync(line: Line): Promise<void>;
    static async invokeAsync(lines: Line[]): Promise<void>;
    static async invokeAsync(editor: Editor): Promise<void>;
    static async invokeAsync(input: Editor | Line | Line[]): Promise<void> {
        if (input instanceof Line) {
            input.subtitle.editor.play(input.startTime);
        } else if (input instanceof Array) {
            this._play(input);
        } else {
            if (!!input.currentSubtitle && input.currentSubtitle.selectedLines.length > 0) {
                this._play(input.currentSubtitle.selectedLines);
            } else if (input.editingLine) {
                input.play(input.editingLine.startTime);
            } else {
                input.play();
            }
        }
    }

    static _play(lines: Line[]): void {
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

        lines[0].subtitle.editor.play(startTime);
    }
}