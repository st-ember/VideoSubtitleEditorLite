import Editor from "../Editor";

export default class PrevSecondCommand {
    static async invokeAsync(editor: Editor): Promise<void> {
        editor.player.prevSecond();
    }
}