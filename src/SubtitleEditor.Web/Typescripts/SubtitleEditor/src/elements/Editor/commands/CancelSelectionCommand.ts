import Editor from "../Editor";

export default class CancelSelectionCommand {

    static async invokeAsync(editor: Editor): Promise<void> {
        if (editor.currentSubtitle && editor.currentSubtitle.lines.filter(line => line.selected).length > 0) {
            editor.currentSubtitle.lines.filter(line => line.selected).forEach(line => line.selectAsync(false));
        }
        editor.updateMultipleSelectionButton();
    }
}