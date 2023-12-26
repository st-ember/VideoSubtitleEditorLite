import Editor from "../Editor";
import MargeLineCommand from "./MargeLineCommand";

/** 如果正在編輯字幕，將該字幕與下一句字幕合併。此指令會觸發 MargeLineCommand。 */
export default class MargeNextLineCommand {

    static async checkAsync(editor: Editor): Promise<boolean> {
        if (!editor.currentSubtitle || !editor.editingLine || !editor.editingLine.next) {
            return false;
        }

        if (!editor.editingLine.caretAtLastPosition || editor.editingLine.editingTime) {
            return false;
        }

        return true;
    }

    static async invokeAsync(editor: Editor): Promise<boolean> {
        if (!editor.currentSubtitle || !editor.editingLine || !editor.editingLine.next) {
            return false;
        }

        if (!editor.editingLine.caretAtLastPosition || editor.editingLine.editingTime) {
            return false;
        }

        const line = editor.editingLine;
        const nextLine = editor.editingLine.next;
        await MargeLineCommand.invokeAsync([line, nextLine]);
        return true;
    }
}