import Editor from "../Editor";
import { Line } from "../components";
import FocusLineCommand from "./FocusLineCommand";
import SelectLineCommand from "./SelectLineCommand";

export default class SelectPrevLineCommand {

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

        let line: Line;
        if (editor.currentSubtitle.selectedLines.length > 1) {
            line = editor.currentSubtitle.selectedLines[0];
        } else if (editor.currentSubtitle.selectedLines.length === 1) {
            line = editor.currentSubtitle.lines[editor.latestSelectedLineIndex - 1 >= 0 && editor.currentSubtitle.lines.length > editor.latestSelectedLineIndex - 1 ? editor.latestSelectedLineIndex - 1 : editor.currentSubtitle.lines.length - 1];
        } else {
            line = editor.currentSubtitle.lines[editor.currentSubtitle.lines.length > editor.latestSelectedLineIndex ? editor.latestSelectedLineIndex : 0];
        }

        await SelectLineCommand.invokeAsync(line);
        await FocusLineCommand.invokeAsync(editor.currentSubtitle.lines[editor.latestSelectedLineIndex]);
    }
}