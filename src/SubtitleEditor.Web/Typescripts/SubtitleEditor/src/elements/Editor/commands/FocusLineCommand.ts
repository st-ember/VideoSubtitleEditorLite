import { Line } from "../components";

export default class FocusLineCommand {

    static async invokeAsync(line: Line): Promise<void> {
        line.subtitle.editor.scrollToLine(line);
    }
}