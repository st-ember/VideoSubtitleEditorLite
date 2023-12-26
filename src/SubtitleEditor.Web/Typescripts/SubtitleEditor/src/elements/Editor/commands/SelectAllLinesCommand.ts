import Editor from "../Editor";

export default class SelectAllLinesCommand {

    static async invokeAsync(editor: Editor): Promise<void> {
        if (editor.currentSubtitle) {
            editor.currentSubtitle.lines.forEach(line => line.selectAsync(true));
            editor.latestSelectedLineIndex = editor.currentSubtitle.lines[editor.currentSubtitle.lines.length - 1].index;
        }
        editor.updateMultipleSelectionButton();
    }
}