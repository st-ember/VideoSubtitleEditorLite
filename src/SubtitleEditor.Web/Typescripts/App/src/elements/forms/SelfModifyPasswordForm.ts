import UpdatePasswordModel from "elements/models/selfManage/UpdatePasswordModel";
import { Form, InputElement, NoteElement } from "uform-form";

export default class SelfModifyPasswordForm extends Form {

    private _password: InputElement = undefined!;
    private _newPassword: InputElement = undefined!;
    private _confirm: InputElement = undefined!;
    private _passwordNote: NoteElement = undefined!;

    async buildChildrenAsync(): Promise<void> {
        this._password = await InputElement.fromAsync({
            label: "現在的密碼",
            required: true,
            type: "password",
            autocomplete: false
        });
        await this.appendAsync(this._password);

        this._newPassword = await InputElement.fromAsync({
            label: "新密碼",
            required: true,
            minLength: 8,
            type: "password",
            autocomplete: false
        });
        await this.appendAsync(this._newPassword);

        this._confirm = await InputElement.fromAsync({
            label: "確認密碼",
            required: true,
            minLength: 8,
            type: "password",
            placeholder: "請再輸入一次密碼",
            autocomplete: false
        });
        await this.appendAsync(this._confirm);

        this._passwordNote = await NoteElement.fromAsync({
            label: "",
            type: "danger",
            hide: true
        });
        await this.appendAsync(this._passwordNote);

        const ruleNote = NoteElement.fromAsync({
            label: "",
            type: "info",
            text: "密碼需要至少八碼，並包含半形的英文大寫、英文小寫、數字、特殊符號。"
        });
        await this.appendAsync(ruleNote);
    }

    async validateAsync(): Promise<boolean> {
        const password = await this._password.getValueAsync();
        const newPassword = await this._newPassword.getValueAsync();
        if (!password) {
            this._updatePasswordNoteAsync("舊密碼欄位為必填。");
            return false;
        } else if (!newPassword) {
            this._updatePasswordNoteAsync("新密碼欄位為必填。");
            return false;
        } else if (!await this._newPassword.validateAsync()) {
            this._updatePasswordNoteAsync(this._newPassword.getValidateError() ?? "格式錯誤。");
            return false;
        } else if (newPassword !== await this._confirm.getValueAsync()) {
            this._updatePasswordNoteAsync("密碼與確認密碼不相符。");
            return false;
        }

        return true;
    }

    private async _updatePasswordNoteAsync(error: string): Promise<void> {
        if (error) {
            await this._passwordNote.setValueAsync(error);
            await this._passwordNote.showAsync();
        } else {
            await this._passwordNote.hideAsync();
        }
    }

    async getValueAsync(): Promise<UpdatePasswordModel> {
        return {
            password: await this._password.getValueAsync(),
            newPassword: await this._newPassword.getValueAsync(),
            confirm: await this._confirm.getValueAsync()
        };
    }
}