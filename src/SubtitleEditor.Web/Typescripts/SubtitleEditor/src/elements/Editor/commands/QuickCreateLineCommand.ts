import Editor from "../Editor";
import CreateLineCommand from "./CreateLineCommand";

export default class QuickCreateLineCommand {

    static async checkAsync(editor: Editor): Promise<boolean> {
        if (!editor.currentSubtitle || editor.editingLine) {
            return false;
        }

        if (!editor.currentTranscript || !editor.currentTranscript.modeState.quickBreakLine || editor.currentTranscript.modeState.modifing) {
            return false;
        }

        return true;
    }

    static async invokeAsync(editor: Editor): Promise<void> {
        if (!editor.currentSubtitle || editor.editingLine) {
            return;
        }

        if (!editor.currentTranscript || !editor.currentTranscript.modeState.quickBreakLine || editor.currentTranscript.modeState.modifing) {
            return;
        }

        const spans = editor.currentTranscript.listFirstLine();
        CreateLineCommand.invokeAsync(editor.currentTranscript, spans);
    }
}