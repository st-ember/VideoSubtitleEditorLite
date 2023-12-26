import { Line } from "../components";
import UpdateLinesHistory from "../histories/UpdateLinesHistory";

export default class ReplaceCommand {

    static async invokeAsync(lines: Line[], target: string, text: string): Promise<void> {
        if (lines.length === 0 || !target) {
            return;
        }

        const subtitle = lines[0].subtitle;
        await subtitle.editor.cancelLineAsync();

        const beforeLineDatas = lines.map(line => {
            return { index: line.index, lineData: { ...line.data } };
        });

        const tasks = lines.map(async (line) => {
            line.replaceText(target, text);
            await line.applyAsync(line.inputData);
        });

        await Promise.all(tasks);

        const afterLineDatas = lines.map(line => {
            return { index: line.index, lineData: { ...line.data } };
        });

        await subtitle.addHistoryAsync(new UpdateLinesHistory(subtitle, { beforeLineDatas, afterLineDatas }));
    }
}