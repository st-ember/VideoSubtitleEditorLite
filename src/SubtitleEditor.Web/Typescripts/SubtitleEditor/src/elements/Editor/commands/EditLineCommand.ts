import Editor from "../Editor";
import { Line } from "../components";

export default class EditLineCommand {

    static async checkAsync(editor: Editor): Promise<boolean> {
        return !!editor.currentSubtitle && editor.currentSubtitle.selectedLines.length > 0;
    }

    static async invokeAsync(line: Line): Promise<void>;
    static async invokeAsync(editor: Editor): Promise<void>;
    static async invokeAsync(input: Line | Editor): Promise<void> {
        let line: Line;
        let editor: Editor;
        if (input instanceof Editor) {
            editor = input;
            if (!editor.currentSubtitle || editor.currentSubtitle.selectedLines.length === 0) {
                return;
            }

            line = editor.currentSubtitle.selectedLines[0];
            await Promise.all(editor.currentSubtitle.selectedLines.map(line => line.selectAsync(false)));
        } else {
            line = input;
            editor = line.subtitle.editor;
        }

        await editor.editLineAsync(line);

        editor.updateMultipleSelectionButton();
    }
}