import { Subtitle } from "../components";
import ShiftLinesHistory from "../histories/ShiftLinesHistory";

export default class ShiftAllLinesCommand {

    static async invokeAsync(subtitle: Subtitle, seconds: number): Promise<void> {
        if (seconds === 0 || subtitle.lines.length === 0) {
            return;
        }

        const lines = subtitle.lines;

        const beforeLineDatas = lines.map(line => {
            return { index: line.index, lineData: { start: line.data.start, end: line.data.end } };
        });

        const tasks = lines.map(async (line) => {
            await line.setStartAsync(line.startTime + seconds);
            await line.setEndAsync(line.endTime + seconds);
        });

        await Promise.all(tasks);

        const afterLineDatas = lines.map(line => {
            return { index: line.index, lineData: { start: line.data.start, end: line.data.end } };
        });

        await subtitle.addHistoryAsync(new ShiftLinesHistory(subtitle, { beforeLineDatas, afterLineDatas }));
        subtitle.editor.updateLineState();
    }
}
