import Editor from "../Editor";

export default class RedoCommand {

    private static _redoing: boolean = false;

    static async checkAsync(): Promise<boolean> {
        return !RedoCommand._redoing;
    }

    static async invokeAsync(editor: Editor): Promise<void> {
        if (RedoCommand._redoing || !editor.currentSubtitle || editor.currentSubtitle.historyIndex >= editor.currentSubtitle.histories.length - 1) {
            return;
        }

        RedoCommand._redoing = true;

        editor.disableUndoRedoButton();
        const history = editor.currentSubtitle.histories[editor.currentSubtitle.historyIndex + 1];
        await history.redoAsync();
        editor.currentSubtitle.historyIndex++;
        editor.updateUndoRedoButton();
        editor.updateLineState();
        editor.updateMultipleSelectionButton();

        if (editor.sideGroupShowing) {
            await editor.searchpanel?.updateLinesAsync();
        }

        RedoCommand._redoing = false;
    }
}