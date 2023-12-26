import Editor from "../Editor";
import MargeLineCommand from "./MargeLineCommand";

/** 如果正在編輯字幕，將該字幕與上一句字幕合併。此指令會觸發 MargeLineCommand。 */
export default class MargePrevLineCommand {

    static async checkAsync(editor: Editor): Promise<boolean> {
        if (!editor.currentSubtitle || !editor.editingLine || !editor.editingLine.prev) {
            return false;
        }

        if (!editor.editingLine.caretAtFirstPosition && editor.editingLine.caretPosition !== undefined || editor.editingLine.editingTime) {
            return false;
        }

        return true;
    }

    static async invokeAsync(editor: Editor): Promise<boolean> {
        if (!editor.currentSubtitle || !editor.editingLine || !editor.editingLine.prev) {
            return false;
        }

        if (!editor.editingLine.caretAtFirstPosition && editor.editingLine.caretPosition !== undefined || editor.editingLine.editingTime) {
            return false;
        }

        const line = editor.editingLine;
        const prevLine = editor.editingLine.prev;
        await MargeLineCommand.invokeAsync([prevLine, line]);
        return true;
    }
}