import ShiftForm from "elements/forms/ShiftForm";
import Editor from "../Editor";
import { Line } from "../components";
import ShiftLinesHistory from "../histories/ShiftLinesHistory";
import dialogController, { Dialog } from "uform-dialog";
import icons from "icons";

export default class ShiftLinesCommand {

    static async invokeAsync(editor: Editor): Promise<void>;
    static async invokeAsync(lines: Line[], seconds?: number): Promise<void>;
    static async invokeAsync(input: Editor | Line[], seconds?: number): Promise<void> {
        if (input instanceof Array) {
            ShiftLinesCommand.doShiftAsync(input, seconds);
        } else {
            const form = new ShiftForm();
            form.description = "請在下方輸入要套用的時間變動量（分:秒）：";
            
            const dialog: Dialog = (await dialogController.showAsync("shift-all", {
                type: "info",
                title: "修改時間",
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
                                ShiftLinesCommand.doShiftAsync(input.currentSubtitle.lines.filter(line => line.selected), totalSecond);
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

    static async doShiftAsync(lines: Line[], seconds?: number): Promise<void> {
        if (lines.length === 0 || seconds === undefined || seconds === null || seconds === 0) {
            return;
        }

        const subtitle = lines[0].subtitle;

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
