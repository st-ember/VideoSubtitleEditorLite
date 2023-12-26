import UserOptionsModel from "elements/models/userMeta/UserOptionsModel";
import { FormGeneric, NoteElement, NumberElement } from "uform-form";

export default class UserOptionsForm extends FormGeneric<UserOptionsModel> {

    private _frameRate: NumberElement = undefined!;
    private _wordLimit: NumberElement = undefined!;

    async buildChildrenAsync(): Promise<void> {
        
        this._frameRate = await NumberElement.fromAsync({
            label: "每秒畫格數",
            min: 0,
            max: 1024,
            step: 0.01,
            required: false
        });
        await this.appendAsync(this._frameRate);

        const frameRateNote = NoteElement.fromAsync({
            label: "",
            type: "info",
            text: "如果您新建立的單集使用上傳的字幕，則設定每秒畫格數後，系統會將字幕內小於一秒的數字當作「影格編號」來計算。您每一次建立單集時都可以再針對該單集獨立調整。"
        });
        await this.appendAsync(frameRateNote);

        this._wordLimit = await NumberElement.fromAsync({
            label: "字數限制",
            min: 0,
            max: 1024,
            step: 1,
            required: false
        });
        await this.appendAsync(this._wordLimit);

        const wordLimitNote = NoteElement.fromAsync({
            label: "",
            type: "info",
            text: "設定字數限制時會在編輯字幕時見到標示限制的紅色線條，超過此線條即表示字數以超過限制，但系統不會硬性要求字數不得超過限制。您可以針對每一個單集獨立調整。"
        });
        await this.appendAsync(wordLimitNote);
    }

    async getValueAsync(): Promise<UserOptionsModel> {
        return {
            frameRate: await this._frameRate.getValueAsync(),
            wordLimit: await this._wordLimit.getValueAsync()
        }
    }

    async setValueAsync(value: UserOptionsModel): Promise<void> {
        await this._frameRate.setValueAsync(value.frameRate);
        await this._wordLimit.setValueAsync(value.wordLimit);
    }
}