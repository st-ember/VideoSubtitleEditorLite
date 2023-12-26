import { apiController } from "ApiController";
import { userGroupApiMap } from "contexts/userGroupApiMap";
import PermissionOption from "elements/models/userGroup/PermissionOption";
import UserGroupData from "elements/models/userGroup/UserGroupData";
import { CheckboxGroupElement, FormGeneric, InputElement, NoteElement, OptionItem, TextAreaElement, TitleElement } from "uform-form"
import { SelectElement } from "uform-form-selector";

export default class UserGroupModifyForm extends FormGeneric<UserGroupData> {

    userGroupId: string = "";

    private _name: InputElement = undefined!;
    private _description: TextAreaElement = undefined!;
    private _groupType: SelectElement = undefined!;
    private _permissionGroups: { element: CheckboxGroupElement, names: string[], name: string }[] = [];

    private _groupOptions: PermissionOption[] = [];
    private _updatingByGroupType: boolean = false;

    async buildChildrenAsync(): Promise<void> {
        const groupTitle = TitleElement.fromAsync({ text: "群組資訊" });
        await this.appendAsync(groupTitle);

        this._name = await InputElement.fromAsync({
            label: "名稱",
            required: true,
            maxLength: 256
        });
        await this.appendAsync(this._name);

        this._description = await TextAreaElement.fromAsync({
            label: "說明"
        });
        await this.appendAsync(this._description);

        const groupTypeOptionResponse = await apiController.queryAsync(userGroupApiMap.listGroupTypeAsOption);
        const groupTypeOptions: OptionItem[] = groupTypeOptionResponse.success ? [{ text: "自訂", value: "-1" }].concat(<any>groupTypeOptionResponse.data) : [];
        this._groupType = await SelectElement.fromAsync({
            label: "群組類型",
            allowEmpty: true,
            multiple: false,
            options: groupTypeOptions
        });
        await this.appendAsync(this._groupType);

        const permissionTitle = TitleElement.fromAsync({ text: "權限設定" });
        await this.appendAsync(permissionTitle);

        const permissionOptionResponse = await apiController.queryAsync(userGroupApiMap.listPermissionAsOption);
        this._groupOptions = permissionOptionResponse.success ? <PermissionOption[]>(permissionOptionResponse.data) : [];

        for (let i = 0; i < this._groupOptions.length; i++) {
            const groupOption = this._groupOptions[i];
            const checkboxGroup = await CheckboxGroupElement.fromAsync({
                label: groupOption.text,
                multipleLine: true,
                options: groupOption.children
            });
            await this.appendAsync(checkboxGroup);
            this._permissionGroups.push({ 
                element: checkboxGroup, 
                names: groupOption.children.map(o => o.value!),
                name: groupOption.value!
            });

            checkboxGroup.addChangeFunc(() => this._updateGroupTypeAsync());
        }

        this._groupType.addChangeFunc(() => this._updatePermissionGroupFromGroupTypeAsync());
    }

    async validateAsync(): Promise<boolean> {
        return await this._name.validateAsync();
    }

    async getValueAsync(): Promise<UserGroupData> {
        const groupType = await this._groupType.getSingleValueAsync();
        return {
            id: this.userGroupId,
            name: await this._name.getValueAsync(),
            description: await this._description.getValueAsync(),
            groupTypeText: groupType !== undefined && groupType !== "-1" ? groupType : undefined,
            permission: (await this._getSelectedPermissionsAsync()).join(";")
        };
    }

    async setValueAsync(value: UserGroupData): Promise<void> {
        this.userGroupId = value.id;

        await Promise.all([
            this._name.setValueAsync(value.name),
            this._description.setValueAsync(value.description),
            this._groupType.setValueAsync(value.groupTypeText !== undefined && value.groupTypeText !== null ? value.groupTypeText : "-1")
        ]);

        const permissions = await this._getPermissionsFromGroupTypeAsync();
        if (permissions.length > 0) {
            await this._updatePermissionGroupsAsync(permissions, true);
        } else {
            await this._updatePermissionGroupsAsync(value.permission ? value.permission.split(';') : []);
        }
    }

    private async _getSelectedPermissionsAsync(): Promise<string[]> {
        const permissions: string[] = [];
        for (let i = 0; i < this._permissionGroups.length; i++) {
            const { element } = this._permissionGroups[i];
            const values = await element.getValueAsync();
            values.filter(o => o !== undefined).forEach(o => permissions.push(o!));
        }

        return permissions;
    }

    private async _updateGroupTypeAsync(): Promise<void> {
        if (!this._updatingByGroupType) {
            const permissions = await this._getPermissionsFromGroupTypeAsync();
            const selectedPermissions = await this._getSelectedPermissionsAsync();
            if (permissions.filter(p => selectedPermissions.indexOf(p) < 0).length > 0 || selectedPermissions.filter(s => permissions.indexOf(s) < 0).length > 0) {
                await this._groupType.setValueAsync("-1");
            }
        }
    }

    private async _updatePermissionGroupFromGroupTypeAsync(): Promise<void> {
        const permissions = await this._getPermissionsFromGroupTypeAsync();
        if (permissions.length > 0) {
            await this._updatePermissionGroupsAsync(permissions, true);
        }
    }

    private async _getPermissionsFromGroupTypeAsync(): Promise<string[]> {
        const groupType = await this._groupType.getSingleValueAsync();
        const permissions: string[] = [];
        if (!!groupType && groupType !== "-1") {
            const matchedGroup = groupType === "SystemAdmin" ?
                this._groupOptions :
                this._groupOptions.filter(groupOption => groupOption.value === "" || groupOption.value === groupType);
        
            matchedGroup.forEach(groupOption => {
                groupOption.children.forEach(o => permissions.push(o.value!));
            });

            return permissions;
        }

        return [];
    }

    private async _updatePermissionGroupsAsync(permissions: string[], updateByGroupType: boolean = false): Promise<void> {
        if (updateByGroupType) {
            this._updatingByGroupType = true;
        }

        for (let i = 0; i < this._permissionGroups.length; i++) {
            const { element } = this._permissionGroups[i];
            await element.setValueAsync(permissions);
        }

        if (updateByGroupType) {
            this._updatingByGroupType = false;
        }
    }
}