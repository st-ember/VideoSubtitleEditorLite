import { defaultGuid } from "contexts/defaultGuid";
import { systemActions } from "contexts/systemActions";
import { userApiMap } from "contexts/userApiMap";
import { userGroupApiMap } from "contexts/userGroupApiMap";
import { userStatus } from "contexts/userStatus";
import { menuController } from "controllers/MenuController";
import { permissionController } from "controllers/PermissionController";
import UserModifyForm from "elements/forms/UserModifyForm";
import UserListCondition from "elements/models/user/UserListCondition";
import UserListData from "elements/models/user/UserListData";
import UserListResponse from "elements/models/user/UserListResponse";
import UserGroupData from "elements/models/userGroup/UserGroupData";
import apiController from "uform-api";
import dialogController, { Dialog } from "uform-dialog";
import { ListPage } from "uform-page";
import selectorController, { Selector } from "uform-selector";
import Utility from "uform-utility";

export default class UserListPage extends ListPage {

    paths = ["/User", "/Users", "/User/List"];
    tableBody?: HTMLTableSectionElement;
    loading?: HTMLDivElement;

    private _createButton?: HTMLButtonElement;

    private _conditionKeyword?: HTMLInputElement;
    private _conditionStatus?: HTMLInputElement;

    private _statusSelector?: Selector;

    protected async loadAsync(): Promise<void> {
        menuController.activeMenu(this.paths);
        menuController.activeMenu("/Management");

        this._createButton = <HTMLButtonElement>document.getElementById("create-button");
        this._createButton?.addEventListener("click", () => this._showCreateDialogAsync());

        this.showLoadingBox(true);
        await this.initConditionInputAsync();
        await this.buildTableBodyAsync();
    }

    protected async initConditionInputAsync(): Promise<void> {
        this._conditionKeyword = <HTMLInputElement>document.getElementById("condition-keyword");

        if (this._conditionKeyword) {
            this._conditionKeyword.parentElement!.style.width = "300px";
        }
        
        await this._initStatusAsync();
    }

    protected async buildTableBodyAsync(): Promise<void> {
        const condition: UserListCondition = {
            page: this.pageInput ? Number(this.pageInput.value) : 0,
            pageSize: this.pageSize ? Number(this.pageSize.value) : 10,
            orderColumn: this.orderColumnInput?.value ?? "",
            descending: this.descendingInput?.value === "True"
        };

        this.buildCondition(condition);
        this.showLoadingBox(true);
        const response = await apiController.queryAsync(userApiMap.list, condition);
        this.showLoadingBox(false);

        if (response.success) {
            if (this.tableContainer) {
                this.tableBody = <HTMLTableSectionElement>this.tableContainer.querySelector("tbody");

                const responseModel: UserListResponse = response.data;
    
                if (this.pageInput) {
                    this.pageInput.value = String(responseModel.page);
                }
    
                if (this.totalPageInput) {
                    this.totalPageInput.value = String(responseModel.totalPage);
                }
    
                if (responseModel.list && responseModel.list.length > 0) {
                    responseModel.list.forEach(data => this.buildTableRow(data));
                }
            }
        } else {
            await dialogController.showErrorAsync("錯誤", `列出資料時發生錯誤：${response.message}`);
        }
    }

    protected buildCondition(condition: UserListCondition): void {
        condition.keyword = this._conditionKeyword?.value;
        condition.status = this._conditionStatus?.value;
    }

    protected buildTableRow(data: UserListData): void {
        const row = document.createElement("tr");

        const updateButtonHtml = permissionController.contains(systemActions.UpdateUser) ?
            `<button class="primary update-button small-button" type="button" title="點選以修改使用者"><span>修改</span></button>` : "";
        const enableButtonHtml = data.status === 1 && permissionController.contains(systemActions.UpdateUserStatus) ?
            `<button class="dark primary-hover enable-button small-button" type="button" title="點選以啟用此使用者"><span>啟用</span></button>` : "";
        const disableButtonHtml = data.status === 0 && permissionController.contains(systemActions.UpdateUserStatus) ?
            `<button class="dark primary-hover disable-button small-button" type="button" title="點選以停用此使用者"><span>停用</span></button>` : "";
        const removeButtonHtml = permissionController.contains(systemActions.RemoveUser) ?
            `<button class="dark danger-hover remove-button small-button" type="button" title="點選以移除使用者"><span>移除</span></button>` : "";

        const statusClass = data.status === 1 ? "warning form-theme-color" : "";

        row.innerHTML = `
            <td align="left" width="160" class="nowrap">${data.account}</td>
            <td align="left" class="nowrap">${data.name ?? ""}</td>
            <td align="left" class="nowrap">${data.title ?? ""}</td>
            <td align="left" class="nowrap">${data.telephone ?? ""}</td>
            <td align="left">${data.email ?? ""}</td>
            <td align="left" class="table-list-items">${data.userGroups.map(o => `<span class="table-list-item">${o}</span>`).join("")}</td>
            <td align="left" class="${statusClass}">${userStatus[data.status]}</td>
            <td align="left">${data.create}</td>
            <td align="left" class="table-actions" width="110">
                ${updateButtonHtml}
                ${enableButtonHtml}
                ${disableButtonHtml}
                ${removeButtonHtml}
            </td>
            `;
            
        this.tableBody!.appendChild(row);

        const enableButton = <HTMLButtonElement>row.querySelector(".enable-button");
        enableButton?.addEventListener("click", () => this._showEnableDialogAsync(data.id));

        const disableButton = <HTMLButtonElement>row.querySelector(".disable-button");
        disableButton?.addEventListener("click", () => this._showDisableDialogAsync(data.id));

        const updateButton = <HTMLButtonElement>row.querySelector(".update-button");
        updateButton?.addEventListener("click", () => this._showUpdateDialogAsync(data.id));

        const removeButton = <HTMLButtonElement>row.querySelector(".remove-button");
        removeButton?.addEventListener("click", () => this._showRemoveDialogAsync(data.id));
    }

    protected showLoadingBox(show: boolean): void {
        if (show && !this.loading) {
            this.loading = document.createElement("div");
            this.loading.className = "loading-box";

            if (this.tableContainer && this.tableContainer.parentElement) {
                this.tableContainer.parentElement.appendChild(this.loading);
            }
        }

        if (show) {
            this.loading?.classList.remove("hide");
        } else {
            this.loading?.classList.add("hide");
        }
    }

    private async _initStatusAsync(): Promise<void> {
        this._conditionStatus = <HTMLInputElement>document.getElementById("condition-status");
        this._statusSelector = selectorController.get(document.getElementById("selector-user-status")!);

        await Utility.waitUntil(() => {
            this._statusSelector = selectorController.get(document.getElementById("selector-user-status")!);
            return !!this._statusSelector;
        }, 100);
        
        if (this._statusSelector) {
            this._statusSelector.onChangeFuncs.push(() => {
                if (this._conditionStatus) {
                    this._conditionStatus.value = (<string>this._statusSelector?.getValue() ?? "-1");
                }
            });

            const value = this._conditionStatus.value;
            this._statusSelector.setValueAsync(value ? value : "-1");
        }
    }

    private async _showEnableDialogAsync(id: string): Promise<void> {
        if (!await dialogController.confirmInfoAsync("確認", "您確定要啟用這位使用者？啟用後的使用者將恢復所有權限，且可登入系統。")) {
            return;
        }

        const response = await apiController.queryAsync(userApiMap.updateStatus, { id, status: 0 });
        if (response.success) {
            const dialog = await dialogController.showSuccessAsync("成功", "成功啟用使用者。");
            dialog!.onCloseAsync = async () => this.reload();
        } else {
            await dialogController.showErrorAsync("錯誤", `啟用使用者時發生錯誤：${response.message}`);
        }
    }

    private async _showDisableDialogAsync(id: string): Promise<void> {
        if (!await dialogController.confirmInfoAsync("確認", "您確定要停用這位使用者？該使用者所有的權限將被暫停，且不再能登入系統。")) {
            return;
        }

        const response = await apiController.queryAsync(userApiMap.updateStatus, { id, status: 1 });
        if (response.success) {
            const dialog = await dialogController.showSuccessAsync("成功", "成功停用使用者。");
            dialog!.onCloseAsync = async () => this.reload();
        } else {
            await dialogController.showErrorAsync("錯誤", `停用使用者時發生錯誤：${response.message}`);
        }
    }

    private async _showRemoveDialogAsync(id: string): Promise<void> {
        if (!await dialogController.confirmWarningAsync("確認", "您確定要移除這位使用者？")) {
            return;
        }

        const response = await apiController.queryAsync(userApiMap.remove, { id });
        if (response.success) {
            const dialog = await dialogController.showSuccessAsync("成功", "成功移除使用者。");
            dialog!.onCloseAsync = async () => this.reload();
        } else {
            await dialogController.showErrorAsync("錯誤", `移除使用者時發生錯誤：${response.message}`);
        }
    }

    private async _showCreateDialogAsync(): Promise<void> {
        const form = new UserModifyForm();

        const dialog: Dialog = (await dialogController.showAsync("create-user", {
            type: "info",
            width: 700,
            title: "建立使用者",
            body: form.element,
            headerCloseButton: true,
            buttons: [
                {
                    type: "primary",
                    text: "建立",
                    callback: async () => {
                        if (!await form.validateAsync()) {
                            return;
                        }

                        const loading = await dialogController.showLoadingAsync("建立中", "正在建立使用者..");
                        const data = await form.getDataAsync();
                        data.id = defaultGuid;
                        const response = await apiController.queryAsync(userApiMap.create, data);
                        if (!response.success) {
                            await dialogController.closeAsync(loading);
                            await dialogController.showErrorAsync("錯誤", `建立使用者時發生錯誤：${response.message}`);
                            return;
                        }

                        if (permissionController.contains(systemActions.UpdateUsersUserGroup)) {
                            const id = response.data;
                            const userGroupResponse = await apiController.queryAsync(userApiMap.updateUsersUserGroup, { id, userGroups: data.userGroups });
                            if (!userGroupResponse.success) {
                                await dialogController.closeAsync(loading);
                                await dialogController.showErrorAsync("錯誤", `使用者已建立成功，但設定使用者權限群組時發生錯誤：${response.message}`);
                                return;
                            }
                        }
                        await dialogController.closeAsync(loading);
                        const dialog = await dialogController.showSuccessAsync("成功", "成功建立使用者。");
                        dialog!.onCloseAsync = async () => this.reload();
                    }
                },
                {
                    type: "default",
                    text: "取消",
                    callback: () => dialogController.closeAsync(dialog)
                }
            ],
            onShowAsync: () => form.buildAsync(),
            onCloseAsync: () => form.deleteAsync()
        }))!;
    }

    private async _showUpdateDialogAsync(id: string): Promise<void> {
        const form = new UserModifyForm();
        form.userId = id;

        const dialog: Dialog = (await dialogController.showAsync("update-user", {
            type: "info",
            width: 700,
            title: "修改使用者",
            body: form.element,
            headerCloseButton: true,
            buttons: [
                {
                    type: "primary",
                    text: "儲存",
                    callback: async () => {
                        if (!await form.validateAsync()) {
                            return;
                        }

                        const loading = await dialogController.showLoadingAsync("儲存中", "正在儲存使用者..");
                        const data = await form.getDataAsync();
                        const response = await apiController.queryAsync(userApiMap.update, data);
                        if (!response.success) {
                            await dialogController.closeAsync(loading);
                            await dialogController.showErrorAsync("錯誤", `儲存使用者時發生錯誤：${response.message}`);
                            return;
                        }

                        if (data.editPassword && permissionController.contains(systemActions.UpdatePassword)) {
                            const response = await apiController.queryAsync(userApiMap.updatePassword, { id, newPassword: data.password, confirm: data.confirm });
                            if (!response.success) {
                                await dialogController.closeAsync(loading);
                                await dialogController.showErrorAsync("錯誤", `資料已儲存成功，但更新使用者密碼時發生錯誤：${response.message}`);
                                return;
                            }
                        }

                        if (permissionController.contains(systemActions.UpdateUsersUserGroup)) {
                            const userGroupResponse = await apiController.queryAsync(userApiMap.updateUsersUserGroup, { id, userGroups: data.userGroups });
                            if (!userGroupResponse.success) {
                                await dialogController.closeAsync(loading);
                                await dialogController.showErrorAsync("錯誤", `資料已儲存成功，但更新使用者權限群組時發生錯誤：${response.message}`);
                                return;
                            }
                        }

                        await dialogController.closeAsync(loading);
                        const dialog = await dialogController.showSuccessAsync("成功", "成功儲存變更。");
                        dialog!.onCloseAsync = async () => this.reload();
                    }
                },
                {
                    type: "default",
                    text: "取消",
                    callback: () => dialogController.closeAsync(dialog)
                }
            ],
            onShowAsync: async () => {
                await form.buildAsync();

                const loading = await dialogController.showLoadingAsync("載入中", "正在載入使用者資訊..");
                const response = await apiController.queryAsync(userApiMap.get, { id });
                const userGroupResponse = permissionController.contains(systemActions.UpdateUsersUserGroup) ?
                    await apiController.queryAsync(userGroupApiMap.listByUser, { id }) : undefined;
                await dialogController.closeAsync(loading);

                if (response.success) {
                    await form.setValueAsync(response.data);

                    if (permissionController.contains(systemActions.UpdateUsersUserGroup)) {
                        if (userGroupResponse!.success) {
                            const userGroupDatas: UserGroupData[] = userGroupResponse!.data;
                            await form.setUserGroupsAsync(userGroupDatas.map(o => o.id));
                        } else {
                            const errorDialog = await dialogController.showErrorAsync("錯誤", "載入使用者群組資訊時發生錯誤！");
                            errorDialog!.onCloseAsync = () => dialogController.closeAsync(dialog);
                        }
                    }
                } else {
                    const errorDialog = await dialogController.showErrorAsync("錯誤", "載入使用者資訊時發生錯誤！");
                    errorDialog!.onCloseAsync = () => dialogController.closeAsync(dialog);
                }
            },
            onCloseAsync: () => form.deleteAsync()
        }))!;
    }
}