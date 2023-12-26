import Editor from "../Editor";

export default class SaveCommand {

    static async invokeAsync(editor: Editor): Promise<void> {
        if (editor.currentSubtitle) {
            for (let i = 0; i < editor.currentSubtitle.lines.length; i++) {
                const line = editor.currentSubtitle.lines[i];
                await line.saveAsync();
            }
        }
    }
}