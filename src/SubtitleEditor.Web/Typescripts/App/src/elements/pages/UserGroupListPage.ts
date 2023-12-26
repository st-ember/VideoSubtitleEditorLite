import { defaultGuid } from "contexts/defaultGuid";
import { systemActions } from "contexts/systemActions";
import { userGroupApiMap } from "contexts/userGroupApiMap";
import { menuController } from "controllers/MenuController";
import { permissionController } from "controllers/PermissionController";
import UserGroupModifyForm from "elements/forms/UserGroupModifyForm";
import UserGroupListCondition from "elements/models/userGroup/UserGroupListCondition";
import UserGroupListData from "elements/models/userGroup/UserGroupListData";
import UserGroupListResponse from "elements/models/userGroup/UserGroupListResponse";
import apiController from "uform-api";
import dialogController, { Dialog } from "uform-dialog";
import { ListPage } from "uform-page";

export default class UserGroupListPage extends ListPage {

    paths = ["/UserGroup", "/UserGroups", "/UserGroup/List"];
    tableBody?: HTMLTableSectionElement;
    loading?: HTMLDivElement;

    private _createButton?: HTMLButtonElement;

    private _conditionKeyword?: HTMLInputElement;

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
    }

    protected async buildTableBodyAsync(): Promise<void> {
        const condition: UserGroupListCondition = {
            page: this.pageInput ? Number(this.pageInput.value) : 0,
            pageSize: this.pageSize ? Number(this.pageSize.value) : 10,
            orderColumn: this.orderColumnInput?.value ?? "",
            descending: this.descendingInput?.value === "True"
        };

        this.buildCondition(condition);
        this.showLoadingBox(true);
        const response = await apiController.queryAsync(userGroupApiMap.list, condition);
        this.showLoadingBox(false);

        if (response.success) {
            if (this.tableContainer) {
                this.tableBody = <HTMLTableSectionElement>this.tableContainer.querySelector("tbody");

                const responseModel: UserGroupListResponse = response.data;
    
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

    protected buildCondition(condition: UserGroupListCondition): void {
        condition.keyword = this._conditionKeyword?.value;
    }

    protected buildTableRow(data: UserGroupListData): void {
        const row = document.createElement("tr");
        const updateButtonHtml = permissionController.contains(systemActions.UpdateUserGroup) ?
            `<button class="primary update-button small-button" type="button" title="點選以修改權限群組"><span>修改</span></button>` : "";
        const duplicateButtonHtml = permissionController.contains(systemActions.DuplicateUserGroup) ?
            `<button class="dark primary-hover duplicate-button small-button" type="button" title="點選以再製權限群組"><span>再製</span></button>` : "";
        const deleteButtonHtml = permissionController.contains(systemActions.DeleteUserGroup) ?
            `<button class="dark danger-hover remove-button small-button" type="button" title="點選以刪除權限群組"><span>刪除</span></button>` : "";

        row.innerHTML = `
            <td align="left" class="nowrap">${data.name}</td>
            <td align="left">${data.description}</td>
            <td align="left">${data.groupType}</td>
            <td align="left">${data.userCount}</td>
            <td align="left">${data.create}</td>
            <td align="left" class="table-actions" width="110">
                ${updateButtonHtml}
                ${duplicateButtonHtml}
                ${deleteButtonHtml}
            </td>
            `;
            
        this.tableBody!.appendChild(row);

        const updateButton = <HTMLButtonElement>row.querySelector(".update-button");
        updateButton?.addEventListener("click", () => this._showUpdateDialogAsync(data.id, data.userCount));

        const duplicateButton = <HTMLButtonElement>row.querySelector(".duplicate-button");
        duplicateButton?.addEventListener("click", () => this._showDuplicateDialogAsync(data.id));

        const deleteButton = <HTMLButtonElement>row.querySelector(".remove-button");
        deleteButton?.addEventListener("click", () => this._showDeleteDialogAsync(data.id, data.userCount));
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

    private async _showDuplicateDialogAsync(id: string): Promise<void> {
        if (!await dialogController.confirmWarningAsync("確認", `您確定要再製這個權限群組？所有權限等資訊將被複製，但使用者不會被複製。`)) {
            return;
        }

        const response = await apiController.queryAsync(userGroupApiMap.duplicate, { id });
        if (response.success) {
            const dialog = await dialogController.showSuccessAsync("成功", "成功再製權限群組。");
            dialog!.onCloseAsync = async () => this.reload();
        } else {
            await dialogController.showErrorAsync("錯誤", `再製權限群組時發生錯誤：${response.message}`);
        }
    }

    private async _showDeleteDialogAsync(id: string, userCount: number): Promise<void> {
        const confirmText = userCount > 0 ? "如此群組仍有使用者存在，這些使用者將失去此群組授予的權限。建議確保在刪除前先將使用者指派至新的群組。" : "";
        if (!await dialogController.confirmWarningAsync("確認", `您確定要刪除這個權限群組？${confirmText}`)) {
            return;
        }

        const response = await apiController.queryAsync(userGroupApiMap.delete, { id });
        if (response.success) {
            const dialog = await dialogController.showSuccessAsync("成功", "成功刪除權限群組。");
            dialog!.onCloseAsync = async () => this.reload();
        } else {
            await dialogController.showErrorAsync("錯誤", `刪除權限群組時發生錯誤：${response.message}`);
        }
    }

    private async _showCreateDialogAsync(): Promise<void> {
        const form = new UserGroupModifyForm();

        const dialog: Dialog = (await dialogController.showAsync("create-user-group", {
            type: "info",
            width: 700,
            title: "建立權限群組",
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

                        const loading = await dialogController.showLoadingAsync("建立中", "正在建立權限群組..");
                        const data = await form.getValueAsync();
                        data.id = defaultGuid;
                        const response = await apiController.queryAsync(userGroupApiMap.create, data);
                        await dialogController.closeAsync(loading);

                        if (response.success) {
                            const dialog = await dialogController.showSuccessAsync("成功", "成功建立權限群組。");
                            dialog!.onCloseAsync = async () => this.reload();
                        } else {
                            await dialogController.showErrorAsync("錯誤", `建立權限群組時發生錯誤：${response.message}`);
                        }
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

    private async _showUpdateDialogAsync(id: string, userCount: number): Promise<void> {
        const form = new UserGroupModifyForm();
        form.userGroupId = id;

        const dialog: Dialog = (await dialogController.showAsync("update-user-group", {
            type: "info",
            width: 700,
            title: "修改權限群組",
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

                        const loading = await dialogController.showLoadingAsync("儲存中", "正在儲存權限群組..");
                        const response = await apiController.queryAsync(userGroupApiMap.update, await form.getValueAsync());
                        await dialogController.closeAsync(loading);

                        if (response.success) {
                            const userCountWarning = userCount > 0 ? "如有權限變更，使用者需要重新整理畫面才能正常生效。" : "";
                            const dialog = await dialogController.showSuccessAsync("成功", `成功儲存變更。${userCountWarning}`);
                            dialog!.onCloseAsync = async () => this.reload();
                        } else {
                            await dialogController.showErrorAsync("錯誤", `儲存權限群組時發生錯誤：${response.message}`);
                        }
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

                const loading = await dialogController.showLoadingAsync("載入中", "正在載入權限群組資訊..");
                const response = await apiController.queryAsync(userGroupApiMap.get, { id });
                await dialogController.closeAsync(loading);

                if (response.success) {
                    await form.setValueAsync(response.data);
                } else {
                    const errorDialog = await dialogController.showErrorAsync("錯誤", "載入權限群組資訊時發生錯誤！");
                    errorDialog!.onCloseAsync = () => dialogController.closeAsync(dialog);
                }
            },
            onCloseAsync: () => form.deleteAsync()
        }))!;
    }
}