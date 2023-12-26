import Editor from "../Editor";

export default class FinishLineCommand {

    static async checkAsync(editor: Editor): Promise<boolean> {
        return !!editor.editingLine;
    }

    static async invokeAsync(editor: Editor): Promise<void> {
        if (!editor.editingLine) {
            return;
        }

        await editor.finishLineAsync();
    }
}