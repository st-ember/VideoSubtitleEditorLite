import { TimeElement } from "uform-form-time";
import { FormGeneric, NoteElement, TextElement } from "uform-form";

export default class ShiftForm extends FormGeneric<{ totalSecond: number }> {

    description: string = "";

    timeSpan: TimeElement = undefined!;

    async buildChildrenAsync(): Promise<void> {

        const desc = TextElement.fromAsync({ text: this.description, multipleLines: true });
        await this.appendAsync(desc);
        
        this.timeSpan = await TimeElement.fromAsync({
            mode: "minuteAndSecond"
        });
        await this.appendAsync(this.timeSpan);

        const note = NoteElement.fromAsync({
            type: "info",
            text: "您可以直接輸入數字，欄位將自動判斷輸入的數值。使用滑鼠或鍵盤左右鍵來切換要輸入的欄位。將滑鼠放置於欄位上並滾動滾輪可進行微調。"
        });
        await this.appendAsync(note);
    }

    async setValueAsync(value: { totalSecond: number }): Promise<void> {
        
    }

    async getValueAsync(): Promise<{ totalSecond: number }> {
        const timeSpan = await this.timeSpan.getValueAsync();
        let totalSecond = Math.abs(timeSpan.minute * 60) + timeSpan.second;
        if (timeSpan.minute < 0) {
            totalSecond *= -1;
        }

        return { totalSecond };
    }
}