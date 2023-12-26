import Editor from "../Editor";

export default class EditNextLineCommand {

    static async checkAsync(editor: Editor): Promise<boolean> {
        if (!editor.currentSubtitle || !editor.editingLine || !editor.editingLine.next) {
            return false;
        }

        return true;
    }

    static async invokeAsync(editor: Editor): Promise<void> {
        if (!editor.currentSubtitle || !editor.editingLine || !editor.editingLine.next) {
            return;
        }

        await Promise.all(editor.currentSubtitle.selectedLines.map(o => o.selectAsync(false)));
        await editor.editLineAsync(editor.editingLine.next);
    }
}