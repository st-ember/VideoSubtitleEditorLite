import Editor from "../Editor";
import { Line } from "../components";

export default class RecoverToOriginalCommand {

    static async invokeAsync(editor: Editor): Promise<void>
    static async invokeAsync(line: Line): Promise<void>;
    static async invokeAsync(lines: Line[]): Promise<void>;
    static async invokeAsync(input: Editor | Line | Line[]): Promise<void> {
        const lines = input instanceof Editor ? 
            (input.editingLine ? [input.editingLine] : input.currentSubtitle?.selectedLines) : 
            input instanceof Array ? input : [input];

        if (!lines || lines.length === 0) {
            return;
        }

        await lines[0].subtitle.editor.cancelLineAsync();
        await Promise.all(lines.map(line => line.recoverAsync()));
    }
}