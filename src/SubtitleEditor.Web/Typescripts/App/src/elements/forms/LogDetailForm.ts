import LogDetailRow from "elements/formElements/LogDetailRow";
import LogListDataModel from "elements/models/log/LogListDataModel";
import LogListPrimaryDataModel from "elements/models/log/LogListPrimaryDataModel";
import { ElementExpander, FormGeneric, ReadonlyInputElement, ReadonlyTextAreaElement } from "uform-form";

export default class LogDetailForm extends FormGeneric<LogListPrimaryDataModel> {

    readonly: boolean = true;

    private _actionId: ReadonlyInputElement = undefined!;
    private _actionName: ReadonlyInputElement = undefined!;
    private _time: ReadonlyInputElement = undefined!;
    private _result: ReadonlyInputElement = undefined!;
    private _userId: ReadonlyInputElement = undefined!;
    private _userAccount: ReadonlyInputElement = undefined!;
    private _ipAddress: ReadonlyInputElement = undefined!;
    private _request: ReadonlyTextAreaElement = undefined!;
    private _response: ReadonlyTextAreaElement = undefined!;
    private _target: ReadonlyInputElement = undefined!;
    private _message: ReadonlyTextAreaElement = undefined!;
    private _code: ReadonlyInputElement = undefined!;
    private _exception: ReadonlyTextAreaElement = undefined!;
    private _innerException: ReadonlyTextAreaElement = undefined!;
    private _children: ElementExpander<LogDetailRow, LogListDataModel> = undefined!;

    async buildChildrenAsync(): Promise<void> {
        this._actionId = await ReadonlyInputElement.fromAsync({
            label: "操作 ID",
            width: "50%"
        });
        await this.appendAsync(this._actionId);

        this._actionName = await ReadonlyInputElement.fromAsync({
            label: "操作",
            width: "50%"
        });
        await this.appendAsync(this._actionName);

        this._time = await ReadonlyInputElement.fromAsync({
            label: "紀錄時間",
            width: "50%"
        });
        await this.appendAsync(this._time);

        this._result = await ReadonlyInputElement.fromAsync({
            label: "操作結果",
            width: "50%"
        });
        await this.appendAsync(this._result);

        this._userId = await ReadonlyInputElement.fromAsync({
            label: "使用者 ID",
            width: "50%"
        });
        await this.appendAsync(this._userId);

        this._userAccount = await ReadonlyInputElement.fromAsync({
            label: "使用者帳號",
            width: "50%"
        });
        await this.appendAsync(this._userAccount);

        this._ipAddress = await ReadonlyInputElement.fromAsync({
            label: "IP"
        });
        await this.appendAsync(this._ipAddress);

        this._request = await ReadonlyTextAreaElement.fromAsync({
            label: "前端要求"
        });
        await this.appendAsync(this._request);

        this._response = await ReadonlyTextAreaElement.fromAsync({
            label: "回應"
        });
        await this.appendAsync(this._response);

        this._target = await ReadonlyInputElement.fromAsync({
            label: "操作目標"
        });
        await this.appendAsync(this._target);

        this._message = await ReadonlyTextAreaElement.fromAsync({
            label: "訊息"
        });
        await this.appendAsync(this._message);

        this._code = await ReadonlyInputElement.fromAsync({
            label: "結果代碼"
        });
        await this.appendAsync(this._code);

        this._exception = await ReadonlyTextAreaElement.fromAsync({
            label: "例外內容"
        });
        await this.appendAsync(this._exception);

        this._innerException = await ReadonlyTextAreaElement.fromAsync({
            label: "內部例外"
        });
        await this.appendAsync(this._innerException);

        this._children = await ElementExpander.fromAsync<LogDetailRow, LogListDataModel>({
            showExpandButtons: false,
            formElementCreationFunc: () => new LogDetailRow(),
            tableOptions: {
                columns: [
                    { head: "結果", width: "50px" }, { head: "操作內容", flex: 1 }, { head: "例外內容", flex: 1 }, { head: "內部例外", flex: 0.5 }
                ]
            }
        });
        await this.appendAsync(this._children);
    }

    async setValueAsync(value: LogListPrimaryDataModel): Promise<void> {
        await this._actionId.setValueAsync(value.actionId ?? "");
        await this._actionName.setValueAsync(value.actionName ?? "");
        await this._time.setValueAsync(value.time ?? "");
        await this._result.setValueAsync(value.success ? "成功" : "失敗");
        await this._userId.setValueAsync(value.userId ?? "");
        await this._userAccount.setValueAsync(value.userAccount ?? "");
        await this._ipAddress.setValueAsync(value.ipAddress ?? "");
        await this._request.setValueAsync(value.request ?? "");
        await this._response.setValueAsync(value.response ?? "");
        await this._target.setValueAsync(value.target ?? "");
        await this._message.setValueAsync(value.message ?? "");
        await this._code.setValueAsync(value.code ?? "");
        await this._exception.setValueAsync(value.exception ?? "");
        await this._innerException.setValueAsync(value.innerException ?? "");

        if (!value.request) {
            await this._request.hideAsync();
        }

        if (!value.response) {
            await this._response.hideAsync();
        }

        if (!value.message) {
            await this._message.hideAsync();
        }

        if (!value.code) {
            await this._code.hideAsync();
        }

        if (!value.exception) {
            await this._exception.hideAsync();
        }

        if (!value.innerException) {
            await this._innerException.hideAsync();
        }

        if (!value.children || value.children.length === 0) {
            await this._children.hideAsync();
        } else {
            await this._children.setValueAsync(value.children);
        }
    }
}