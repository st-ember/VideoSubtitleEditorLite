import Editor from "../Editor";
import { Line } from "../components";
import ShiftLinesHistory from "../histories/ShiftLinesHistory";
import dialogController, { Dialog } from "uform-dialog";
import icons from "icons";
import CompensateForm from "elements/forms/CompensateForm";

export default class CompensateLinesCommand {

    static async invokeAsync(editor: Editor): Promise<void>;
    static async invokeAsync(lines: Line[], seconds?: number): Promise<void>;
    static async invokeAsync(input: Editor | Line[], seconds?: number): Promise<void> {
        if (input instanceof Array) {
            CompensateLinesCommand.doCompensateAsync(input, seconds);
        } else {
            const form = new CompensateForm();
            form.description = "此功能會嘗試延長字幕時間來縮短字幕間的空白。您可以讓系統自動填滿所有空白，或限制最長可以增加的時間。";
            
            const dialog: Dialog = (await dialogController.showAsync("shift-all", {
                type: "info",
                title: "補償字幕間的空白",
                width: 420,
                body: form.element,
                buttons: [
                    {
                        text: `${icons.Finish}確認修改`,
                        type: "primary",
                        keybindings: "Enter",
                        callback: async () => {
                            const { totalSecond } = await form.getValueAsync();
                            if (input.currentSubtitle) {
                                CompensateLinesCommand.doCompensateAsync(input.currentSubtitle.lines.filter(line => line.selected), totalSecond);
                            }
                            
                            await dialog.closeAsync();
                        }
                    },
                    { text: "關閉", keybindings: "Escape" }
                ],
                onShowAsync: async () => {
                    await form.buildAsync();
                    form.timeSpan.minuteInput.focus();
                },
                onCloseAsync: () => form.deleteAsync()
            }))!;
        }
    }

    static async doCompensateAsync(lines: Line[], seconds?: number): Promise<void> {
        if (lines.length === 0) {
            return;
        }

        const subtitle = lines[0].subtitle;

        const beforeLineDatas = lines.map(line => {
            return { index: line.index, lineData: { start: line.data.start, end: line.data.end } };
        });

        let modified = false;
        const tasks = lines.map(async (line) => {
            const next = line.next;
            if (next) {
                const difference = next.startTime - line.endTime;
                const adoptedCompensateSeconds = seconds === undefined || seconds === null || seconds >= difference ? difference : seconds;
                await line.setEndAsync(line.endTime + adoptedCompensateSeconds);
                modified = true;
            }
        });

        await Promise.all(tasks);

        if (modified) {
            const afterLineDatas = lines.map(line => {
                return { index: line.index, lineData: { start: line.data.start, end: line.data.end } };
            });
    
            await subtitle.addHistoryAsync(new ShiftLinesHistory(subtitle, { beforeLineDatas, afterLineDatas }));
            subtitle.editor.updateLineState();
        }
    }
}