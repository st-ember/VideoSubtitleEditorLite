import { apiController } from "ApiController";
import { userApiMap } from "contexts/userApiMap";
import UserDataWithPassword from "elements/models/user/UserDataWithPassword";
import UserData from "elements/models/user/UserData";
import { CheckboxElement, Form, FormElementContainer, InputElement, NoteElement, TextAreaElement, TitleElement } from "uform-form";
import { SelectElement } from "uform-form-selector";
import { userGroupApiMap } from "contexts/userGroupApiMap";
import { permissionController } from "controllers/PermissionController";
import { systemActions } from "contexts/systemActions";

export default class UserModifyForm extends Form {

    userId: string = "";

    private _originalAccount: string = "";
    private _validAccount: boolean = true;

    private _account: InputElement = undefined!;
    private _accountNote: NoteElement = undefined!;
    private _userGroup: SelectElement = undefined!;
    private _nameInput: InputElement = undefined!;
    private _title: InputElement = undefined!;
    private _telephone: InputElement = undefined!;
    private _email: InputElement = undefined!;
    private _description: TextAreaElement = undefined!;
    private _editPassword: CheckboxElement = undefined!;
    private _passwordGroup: FormElementContainer = undefined!;
    private _password: InputElement = undefined!;
    private _confirm: InputElement = undefined!;
    private _passwordNote: NoteElement = undefined!;

    protected async buildContainerAsync(): Promise<void> {
        const accountTitle = TitleElement.fromAsync({ text: "帳號資訊" });
        await this.appendAsync(accountTitle);

        const accountGroup = FormElementContainer.fromAsync({
            label: "帳號",
            required: true,
            multipleLine: true,
            buildChildrenFunc: async group => {
                this._account = await InputElement.fromAsync({
                    required: true,
                    hideReqiredIcon: true,
                    maxLength: 256,
                    autocomplete: false
                });
                await group.appendAsync(this._account);

                this._accountNote = await NoteElement.fromAsync({
                    type: "success",
                    text: "",
                    hide: true
                });
                await group.appendAsync(this._accountNote);
            }
        });
        await this.appendAsync(accountGroup);

        const userGroupOptionResponse = await apiController.queryAsync(userGroupApiMap.listAsOption);
        this._userGroup = await SelectElement.fromAsync({
            label: "權限群組",
            allowEmpty: false,
            multiple: true,
            hide: !permissionController.contains(systemActions.UpdateUsersUserGroup),
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

        this._description = await TextAreaElement.fromAsync({
            label: "描述"
        });
        await this.appendAsync(this._description);

        const secretTitle = TitleElement.fromAsync({ 
            text: "密碼資訊",
            hide: !!this.userId && !permissionController.contains(systemActions.UpdatePassword)
        });
        await this.appendAsync(secretTitle);

        this._editPassword = await CheckboxElement.fromAsync({
            label: "修改密碼",
            hide: !this.userId || !!this.userId && !permissionController.contains(systemActions.UpdatePassword)
        });
        await this.appendAsync(this._editPassword);

        this._passwordGroup = await FormElementContainer.fromAsync({
            hide: !!this.userId,
            multipleLine: true,
            buildChildrenFunc: async group => {
                this._passwordNote = await NoteElement.fromAsync({
                    label: "",
                    type: "warning",
                    text: "",
                    hide: true
                });
                await group.appendAsync(this._passwordNote);

                this._password = await InputElement.fromAsync({
                    label: "密碼",
                    type: "password",
                    required: !this.userId,
                    minLength: 8
                });
                await group.appendAsync(this._password);

                this._confirm = await InputElement.fromAsync({
                    label: "確認密碼",
                    type: "password",
                    required: !this.userId,
                    minLength: 8,
                    placeholder: "請再輸入一次密碼"
                });
                await group.appendAsync(this._confirm);

                const ruleNote = NoteElement.fromAsync({
                    label: "",
                    type: "info",
                    text: "密碼需要至少八碼，並包含半形的英文大寫、英文小寫、數字、特殊符號。"
                });
                await group.appendAsync(ruleNote);
            }
        });
        await this.appendAsync(this._passwordGroup);

        this._account.addChangeFunc(() => this._updateAccountStateAsync());
        this._editPassword.addChangeFunc(() => this._updatePasswordGroupAsync());
    }

    private async _updateAccountStateAsync(): Promise<void> {
        const account = await this._account.getValueAsync();
        if (!this._originalAccount || this._originalAccount !== account) {
            const response = await apiController.queryAsync(userApiMap.isAccountExist, { account });
            if (response.success) {
                if (response.data === true) {
                    this._accountNote.type = "warning";
                    this._accountNote.text = "這個帳號已存在";
                    this._validAccount = false;
                } else {
                    this._accountNote.type = "success";
                    this._accountNote.text = "這個帳號可以使用";
                    this._validAccount = true;
                }
            } else {
                this._accountNote.type = "danger";
                this._accountNote.text = "檢查帳號發生錯誤";
                this._validAccount = false;
                await this._accountNote.rebuildAsync();
            }
        } else {
            this._validAccount = true;
            await this._accountNote.hideAsync();
        }
    }

    private async _updatePasswordGroupAsync(): Promise<void> {
        const editPassword = await this._editPassword.getValueAsync();
        if (editPassword && permissionController.contains(systemActions.UpdatePassword) || !this.userId) {
            await this._passwordGroup.showAsync();
        } else {
            await this._passwordGroup.hideAsync();
        }
    }

    private async _updatePasswordNoteAsync(error: string): Promise<void> {
        if (error) {
            await this._passwordNote.setValueAsync(error);
            await this._passwordNote.showAsync();
        } else {
            await this._passwordNote.hideAsync();
        }
    }

    async validateAsync(): Promise<boolean> {
        if (!this._validAccount || !await this._account.validateAsync()) {
            return false;
        }

        const neetPassword = !this.userId || await this._editPassword.getValueAsync() && permissionController.contains(systemActions.UpdatePassword);
        if (neetPassword) {
            const password = await this._password.getValueAsync();
            if (!password) {
                this._updatePasswordNoteAsync("密碼欄位為必填。");
                return false;
            } else if (!await this._password.validateAsync()) {
                this._updatePasswordNoteAsync(this._password.getValidateError() ?? "格式錯誤。");
                return false;
            } else if (password !== await this._confirm.getValueAsync()) {
                this._updatePasswordNoteAsync("密碼與確認密碼不相符。");
                return false;
            }
        }

        return true;
    }

    async getDataAsync(): Promise<UserDataWithPassword> {
        const userGroups = await this._userGroup.getArrayValueAsync();
        return {
            id: this.userId,
            account: await this._account.getValueAsync(),
            name: await this._nameInput.getValueAsync(),
            title: await this._title.getValueAsync(),
            telephone: await this._telephone.getValueAsync(),
            email: await this._email.getValueAsync(),
            description: await this._description.getValueAsync(),
            editPassword: await this._editPassword.getValueAsync(),
            password: await this._password.getValueAsync(),
            confirm: await this._confirm.getValueAsync(),
            status: 0,
            userGroups: userGroups.filter(o => o !== undefined).map(o => o!)
        };
    }

    async setValueAsync(value: UserData): Promise<void> {
        this.userId = value.id;
        this._originalAccount = value.account;
        
        await Promise.all([
            this._account.setValueAsync(value.account),
            this._nameInput.setValueAsync(value.name),
            this._title.setValueAsync(value.title),
            this._telephone.setValueAsync(value.telephone),
            this._email.setValueAsync(value.email),
            this._description.setValueAsync(value.description)
        ]);
    }

    async setUserGroupsAsync(ids: string[]): Promise<void> {
        await this._userGroup.setValueAsync(ids);
    }
}