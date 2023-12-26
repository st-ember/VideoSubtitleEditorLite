import Editor from "../Editor";

export default class UndoCommand {

    private static _undoing: boolean = false;

    static async checkAsync(): Promise<boolean> {
        return !UndoCommand._undoing;
    }

    static async invokeAsync(editor: Editor): Promise<void> {
        if (UndoCommand._undoing || !editor.currentSubtitle || editor.currentSubtitle.historyIndex < 0) {
            return;
        }
        
        UndoCommand._undoing = true;

        editor.disableUndoRedoButton();
        const history = editor.currentSubtitle.histories[editor.currentSubtitle.historyIndex];
        await history.undoAsync();
        editor.currentSubtitle.historyIndex--;
        editor.updateUndoRedoButton();
        editor.updateLineState();
        editor.updateMultipleSelectionButton();

        if (editor.sideGroupShowing) {
            await editor.searchpanel?.updateLinesAsync();
        }

        UndoCommand._undoing = false;
    }
}