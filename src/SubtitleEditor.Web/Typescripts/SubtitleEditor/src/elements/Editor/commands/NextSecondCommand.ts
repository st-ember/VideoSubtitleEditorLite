import Editor from "../Editor";

export default class NextSecondCommand {
    static async invokeAsync(editor: Editor): Promise<void> {
        editor.player.nextSecond();
    }
}