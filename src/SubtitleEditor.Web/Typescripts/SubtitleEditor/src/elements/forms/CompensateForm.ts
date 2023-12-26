import { TimeElement } from "uform-form-time";
import { CheckboxElement, FormGeneric, NoteElement, TextElement } from "uform-form";

export default class CompensateForm extends FormGeneric<{ totalSecond?: number }> {
    description: string = "";

    withLimit: CheckboxElement = undefined!;
    timeSpan: TimeElement = undefined!;

    private _note: NoteElement = undefined!;

    async buildChildrenAsync(): Promise<void> {

        const desc = TextElement.fromAsync({ text: this.description, multipleLines: true });
        await this.appendAsync(desc);

        this.withLimit = await CheckboxElement.fromAsync({
            text: "限制最多可以補償的時間"
        });
        await this.appendAsync(this.withLimit);
        
        this.timeSpan = await TimeElement.fromAsync({
            mode: "minuteAndSecond",
            hide: true
        });
        await this.appendAsync(this.timeSpan);

        this._note = await NoteElement.fromAsync({
            type: "info",
            text: "您可以直接輸入數字，欄位將自動判斷輸入的數值。使用滑鼠或鍵盤左右鍵來切換要輸入的欄位。將滑鼠放置於欄位上並滾動滾輪可進行微調。",
            hide: true
        });
        await this.appendAsync(this._note);

        this.withLimit.addChangeFunc(async () => {
            const withLimit = await this.withLimit.getValueAsync();
            if (withLimit) {
                this.timeSpan.showAsync();
                this._note.showAsync();
            } else {
                this.timeSpan.hideAsync();
                this._note.hideAsync();
            }
        });
    }

    async setValueAsync(value: { totalSecond?: number }): Promise<void> {
        
    }

    async getValueAsync(): Promise<{ totalSecond?: number }> {
        const withLimit = await this.withLimit.getValueAsync();
        if (!withLimit) {
            return { totalSecond: undefined };
        }

        const timeSpan = await this.timeSpan.getValueAsync();
        let totalSecond = Math.abs(timeSpan.minute * 60) + timeSpan.second;
        return { totalSecond };
    }
} 