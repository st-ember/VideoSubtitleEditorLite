import Editor from "../Editor";
import FocusLineCommand from "./FocusLineCommand";

export default class AddSelectNextLineCommand {

    static async checkAsync(editor: Editor): Promise<boolean> {
        if (!editor.currentSubtitle) {
            return false;
        }

        return true;
    }

    static async invokeAsync(editor: Editor): Promise<void> {
        if (!editor.currentSubtitle) {
            return;
        }

        if (editor.currentSubtitle.selectedLines.length > 0) {
            const last = editor.currentSubtitle.selectedLines[editor.currentSubtitle.selectedLines.length - 1];
            if (last.next) {
                last.next.selectAsync(true);
                editor.latestSelectedLineIndex = last.next.index;
            }
        } else {
            const line = editor.currentSubtitle.lines[editor.currentSubtitle.lines.length > editor.latestSelectedLineIndex ? editor.latestSelectedLineIndex : 0];
            line.selectAsync(true);
            editor.latestSelectedLineIndex = line.index;
        }

        await FocusLineCommand.invokeAsync(editor.currentSubtitle.lines[editor.latestSelectedLineIndex]);
        editor.updateMultipleSelectionButton();
    }
}