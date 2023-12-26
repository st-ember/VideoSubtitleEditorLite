import { defaultGuid } from "contexts/defaultGuid";
import { userGroupApiMap } from "contexts/userGroupApiMap";
import UserData from "elements/models/user/UserData";
import apiController from "uform-api";
import { FormGeneric, InputElement, ReadonlyInputElement, ReadonlyPickerElement } from "uform-form";

export default class SelfModifyForm extends FormGeneric<UserData> {

    private _data: UserData = undefined!;

    private _account: ReadonlyInputElement = undefined!;
    private _userGroup: ReadonlyPickerElement = undefined!;
    private _nameInput: InputElement = undefined!;
    private _title: InputElement = undefined!;
    private _telephone: InputElement = undefined!;
    private _email: InputElement = undefined!;

    async buildChildrenAsync(): Promise<void> {
        this._account = await ReadonlyInputElement.fromAsync({
            label: "帳號"
        });
        await this.appendAsync(this._account);

        const userGroupOptionResponse = await apiController.queryAsync(userGroupApiMap.listAsOption);
        this._userGroup = await ReadonlyPickerElement.fromAsync({
            label: "權限群組",
            options: userGroupOptionResponse.success ? userGroupOptionResponse.data : []
        });
        await this.appendAsync(this._userGroup);

        this._nameInput = await InputElement.fromAsync({
            label: "姓名",
            maxLength: 256
        });
        await this.appendAsync(this._nameInput);

        this._title = await InputElement.fromAsync({
            label: "職稱",
            maxLength: 256
        });
        await this.appendAsync(this._title);

        this._telephone = await InputElement.fromAsync({
            label: "電話",
            maxLength: 256,
            format: "phone"
        });
        await this.appendAsync(this._telephone);

        this._email = await InputElement.fromAsync({
            label: "Email",
            maxLength: 256,
            format: "email"
        });
        await this.appendAsync(this._email);
    }

    async getValueAsync(): Promise<UserData> {
        return {
            id: defaultGuid,
            account: "",
            name: await this._nameInput.getValueAsync(),
            title: await this._title.getValueAsync(),
            telephone: await this._telephone.getValueAsync(),
            email: await this._email.getValueAsync(),
            description: "",
            status: 0
        };
    }

    async setValueAsync(value: UserData): Promise<void> {
        this._data = { ...value };

        await Promise.all([
            this._account.setValueAsync(value.account),
            this._nameInput.setValueAsync(value.name),
            this._title.setValueAsync(value.title),
            this._telephone.setValueAsync(value.telephone),
            this._email.setValueAsync(value.email)
        ]);
    }

    async setUserGroupsAsync(ids: string[]): Promise<void> {
        await this._userGroup.setValueAsync(ids);
    }
}