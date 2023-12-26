import Editor from "../Editor";

export default class EditPrevLineCommand {

    static async checkAsync(editor: Editor): Promise<boolean> {
        if (!editor.currentSubtitle || !editor.editingLine || !editor.editingLine.prev) {
            return false;
        }

        return true;
    }

    static async invokeAsync(editor: Editor): Promise<void> {
        if (!editor.currentSubtitle || !editor.editingLine || !editor.editingLine.prev) {
            return;
        }

        await Promise.all(editor.currentSubtitle.selectedLines.map(o => o.selectAsync(false)));
        await editor.editLineAsync(editor.editingLine.prev);
    }
}

