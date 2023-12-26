import { Line } from "../components";
import UpdateLineHistory from "../histories/UpdateLineHistory";
import { LineData } from "../models";

export default class UpdateLineCommand {

    static async invokeAsync(line: Line, lineData: LineData): Promise<{ success: boolean, edited: boolean }> {
        if (line.index < 0) {
            return { success: false, edited: false };
        }

        const dataBeforeUpdate: LineData = { ...line.data };
        const { success, edited } = await line.applyAsync(lineData);
        if (success && edited) {
            await line.subtitle.addHistoryAsync(new UpdateLineHistory(line.subtitle, { index: line.index, beforeLineData: dataBeforeUpdate, afterLineData: lineData }));
        }
        return { success, edited };
    }
}
