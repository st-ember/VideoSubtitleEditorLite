import FixBookItem from "elements/models/fixBook/FixBookItem";
import { FormElementContainerGeneric, InputElement } from "uform-form";

export default class FixBookRow extends FormElementContainerGeneric<FixBookItem> {

    private _xInput: InputElement = undefined!;
    private _oInput: InputElement = undefined!;

    async buildChildrenAsync(): Promise<void> {
        this._xInput = await InputElement.fromAsync({});
        await this.appendAsync(this._xInput);

        this._oInput = await InputElement.fromAsync({});
        await this.appendAsync(this._oInput);
    }

    async getValueAsync(): Promise<FixBookItem> {
        return {
            original: await this._xInput.getValueAsync(),
            correction: await this._oInput.getValueAsync()
        }
    }

    async setValueAsync(value: FixBookItem): Promise<void> {
        await this._xInput.setValueAsync(value.original);
        await this._oInput.setValueAsync(value.correction);
    }
}