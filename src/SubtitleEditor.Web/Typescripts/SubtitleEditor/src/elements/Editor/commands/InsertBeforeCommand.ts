import Editor from "../Editor";
import { Line } from "../components";
import InsertLineCommand from "./InsertLineCommand";

export default class InsertBeforeCommand {

    static async checkAsync(line: Line): Promise<boolean>;
    static async checkAsync(editor: Editor): Promise<boolean>;
    static async checkAsync(input: Line | Editor): Promise<boolean> {
        return input instanceof Line || !!input.currentSubtitle && input.currentSubtitle.selectedLines.length > 0 || !!input.editingLine;
    }

    static async invokeAsync(line: Line): Promise<void>;
    static async invokeAsync(editor: Editor): Promise<void>;
    static async invokeAsync(input: Line | Editor): Promise<void> {
        let line: Line | undefined = undefined;
        if (input instanceof Line) {
            line = input;
        } else {
            if (!!input.editingLine) {
                line = input.editingLine;
            } else if (input.currentSubtitle && input.currentSubtitle.selectedLines.length > 0) {
                line = input.currentSubtitle.selectedLines[0];
            }  
        }

        if (line) {
            const index = line.index > 0 ? line.index - 1 : 0;
            InsertLineCommand.invokeAsync(line.subtitle, index);
        }
    }
}
