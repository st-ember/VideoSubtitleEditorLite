import LogListDataModel from "elements/models/log/LogListDataModel";
import { FormElementContainerGeneric, TextElement, CodeElement } from "uform-form";

export default class LogDetailRow extends FormElementContainerGeneric<LogListDataModel> {

    private _result: TextElement = undefined!;
    private _actionMessage: CodeElement = undefined!;
    private _exception: CodeElement = undefined!;
    private _innerException: CodeElement = undefined!;

    private _data: LogListDataModel = undefined!;

    async buildChildrenAsync(): Promise<void> {
        this._result = await TextElement.fromAsync({ multipleLines: true });
        await this.appendAsync(this._result);
        this._result.element.style.marginTop = "0";

        this._actionMessage = await CodeElement.fromAsync({});
        await this.appendAsync(this._actionMessage);
        this._actionMessage.element.style.marginTop = "0";

        this._exception = await CodeElement.fromAsync({});
        await this.appendAsync(this._exception);
        this._exception.element.style.marginTop = "0";

        this._innerException = await CodeElement.fromAsync({});
        await this.appendAsync(this._innerException);
        this._innerException.element.style.marginTop = "0";
    }

    async getValueAsync(): Promise<LogListDataModel> {
        return this._data;
    }

    async setValueAsync(value: LogListDataModel): Promise<void> {
        if (!value) { return; }
        this._data = value;

        this._result.text = value.success ? "成功" : "失敗";
        this._actionMessage.text = value.actionMessage ?? "";
        this._exception.text = value.exception ?? "";
        this._innerException.text = value.innerException ?? "";

        await this._result.rebuildAsync();
        await this._actionMessage.rebuildAsync();
        await this._exception.rebuildAsync();
        await this._innerException.rebuildAsync();
    }
}