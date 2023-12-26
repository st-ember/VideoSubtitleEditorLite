import { FormGeneric, RadioButtonElement } from "uform-form";

export class EncodingSelectionForm extends FormGeneric<{ encoding: string | undefined }> {

    private _encoding: RadioButtonElement = undefined!;

    async buildChildrenAsync(): Promise<void> {
        this._encoding = await RadioButtonElement.fromAsync({
            options: [
                { text: "UTF-16", value: "UTF-16" },
                { text: "UTF-8", value: "UTF-8" },
                { text: "UTF-8 with BOM", value: "UTF-8 with BOM" }
            ]
        });
        await this.appendAsync(this._encoding);
    }

    async getValueAsync(): Promise<{ encoding: string | undefined; }> {
        const encoding = await this._encoding.getValueAsync();
        return { encoding };
    }
}