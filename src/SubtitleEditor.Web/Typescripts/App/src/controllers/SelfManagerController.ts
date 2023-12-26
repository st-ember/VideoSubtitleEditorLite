import { selfManageApiMap } from "contexts/selfManageApiMap";
import SelfModifyForm from "elements/forms/SelfModifyForm";
import SelfModifyPasswordForm from "elements/forms/SelfModifyPasswordForm";
import UserGroupData from "elements/models/userGroup/UserGroupData";
import apiController from "uform-api";
import dialogController, { Dialog } from "uform-dialog";
import { permissionController } from "./PermissionController";
import { systemActions } from "contexts/systemActions";
import UserKeybindingForm from "elements/forms/UserKeybindingForm";
import { userMetaApiMap } from "contexts/userMetaApiMap";
import UserOptionsForm from "elements/forms/UserOptionsForm";

export default class SelfManagerController {

    private _element: HTMLDivElement = undefined!;
    private _name: string = "";
    private _menuElement: HTMLDivElement = document.createElement("div");

    private _hover: boolean = false;
    private _focused: boolean = false;
    private _menuHeight: number = 0;

    public isIdle: boolean = false;
    public logout: boolean = false;

    public init(element: HTMLDivElement): void {
        this._element = element;
        this._init();
    }

    public logoutNow(): void {
        if (!this.logout) {
            window.location.pathname = "/Account/Logout";
        }
    }

    private _init(): void {
        this._buildUserName();
        this._buildMenu();

        this._element.addEventListener("mouseenter", () => {
            this._hover = true;
            this._updateMenu();
        });

        this._element.addEventListener("mouseleave", () => {
            if (!this._focused) {
                this._hover = false;
                this._updateMenu();
            }
        });
    }

    private _buildUserName(): void {
        this._name = <string>this._element.getAttribute("data-name");

        const userNameElement = document.createElement("div");
        userNameElement.className = "user-item";
        userNameElement.id = "self-manage-user-name";
        userNameElement.innerHTML = `您好 ${this._name ? this._name : "Guest"}！`;
        this._element.appendChild(userNameElement);
    }

    private _buildMenu(): void {
        const menuButton = document.createElement("button");
        menuButton.id = "self-manage-menu-button";
        menuButton.className = "user-item";
        menuButton.type = "button";
        menuButton.title = "帳號選單";
        menuButton.innerHTML = `<i class="fa fa-cog"></i>`;
        this._element.appendChild(menuButton);

        menuButton.addEventListener("focus", () => {
            this._focused = true;
            this._hover = true;
            this._updateMenu();
        });

        menuButton.addEventListener("blur", () => {
            setTimeout(() => {
                this._focused = false;
                this._hover = false;
                this._updateMenu();
            }, 100);
        });

        let height = 2;

        this._menuElement = document.createElement("div");
        this._menuElement.id = "self-manage-menu";
        this._element.appendChild(this._menuElement);

        if (this._name) {
            if (permissionController.contains(systemActions.SelfUpdateUser)) {
                const anchor = document.createElement("a");
                anchor.title = "修改帳號資訊";
                anchor.innerHTML = `<i class="fa-regular fa-user"></i>修改帳號資訊`;
                this._menuElement.appendChild(anchor);
                anchor.addEventListener("click", () => this._showSelfModifyDialogAsync());
                height += anchor.clientHeight;
            }

            if (permissionController.contains(systemActions.SelfUpdateUserPassword)) {
                const anchor = document.createElement("a");
                anchor.title = "修改密碼";
                anchor.innerHTML = `<i class="fa fa-shield-alt fa-fw"></i>修改密碼`;
                this._menuElement.appendChild(anchor);
                anchor.addEventListener("click", () => this._showSelfModifyPasswordDialogAsync());
                height += anchor.clientHeight + 1;
            }

            if (permissionController.contains(systemActions.SaveSelfKeybinding)) {
                const anchor = document.createElement("a");
                anchor.title = "修改字幕編輯器的鍵盤快速鍵設定";
                anchor.innerHTML = `<i class="fa-regular fa-keyboard"></i>修改快速鍵設定`;
                this._menuElement.appendChild(anchor);
                anchor.addEventListener("click", () => this._showSelfModifyKeybindingDialogAsync());
                height += anchor.clientHeight + 1;
            }

            if (permissionController.contains(systemActions.SaveSelfOptions)) {
                const anchor = document.createElement("a");
                anchor.title = "修改帳號的各式喜好設定";
                anchor.innerHTML = `<i class="fa-regular fa-heart"></i>修改喜好設定`;
                this._menuElement.appendChild(anchor);
                anchor.addEventListener("click", () => this._showSelfModifyOptionsDialogAsync());
                height += anchor.clientHeight + 1;
            }
        }

        const logoutAnchor = document.createElement("a");
        logoutAnchor.className = "border-top";
        logoutAnchor.href = "/Account/Logout";
        logoutAnchor.title = "登出";
        logoutAnchor.innerHTML = `<i class="fa fa-sign-out-alt fa-fw"></i>登出`;
        logoutAnchor.addEventListener("click", () => this.logout = true);
        this._menuElement.appendChild(logoutAnchor);
        height += logoutAnchor.clientHeight + 1;

        setTimeout(() => {
            this._menuHeight = height;
            this._updateMenu();
        }, 0);
    }

    private _updateMenu(): void {
        if (this._hover) {
            this._menuElement.style.height = `${this._menuHeight}px`;
            this._menuElement.style.opacity = "1";
        } else {
            this._menuElement.style.height = "0";
            this._menuElement.style.opacity = "0";
        }
    }

    private async _showSelfModifyDialogAsync(): Promise<void> {
        const form = new SelfModifyForm();

        const dialog: Dialog = (await dialogController.showAsync("self-modify-user", {
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
                        const data = await form.getValueAsync();
                        const response = await apiController.queryAsync(selfManageApiMap.selfUpdateUser, data);
                        await loading.closeAsync();
                        if (response.success) {
                            const success = await dialogController.showSuccessAsync("成功", "成功儲存變更。");
                            success!.onCloseAsync = async () => dialog.closeAsync();
                        } else {
                            await dialogController.showErrorAsync("錯誤", `儲存使用者時發生錯誤：${response.message}`);
                        }
                    }
                },
                { text: "取消" }
            ],
            onShowAsync: async () => {
                await form.buildAsync();

                const loading = await dialogController.showLoadingAsync("載入中", "正在載入使用者資訊..");
                const response = await apiController.queryAsync(selfManageApiMap.getSelfModifyData);
                const userGroupResponse = await apiController.queryAsync(selfManageApiMap.getSelfModifyGroupData);
                await loading.closeAsync();

                if (response.success) {
                    await form.setValueAsync(response.data);

                    if (userGroupResponse.success) {
                        const userGroupDatas: UserGroupData[] = userGroupResponse.data;
                        await form.setUserGroupsAsync(userGroupDatas.map(o => o.id));
                    } else {
                        const errorDialog = await dialogController.showErrorAsync("錯誤", "載入使用者群組資訊時發生錯誤！");
                        errorDialog!.onCloseAsync = () => dialog.closeAsync();
                    }
                } else {
                    const errorDialog = await dialogController.showErrorAsync("錯誤", "載入使用者資訊時發生錯誤！");
                    errorDialog!.onCloseAsync = () => dialog.closeAsync();
                }
            },
            onCloseAsync: () => form.deleteAsync()
        }))!;
    }

    private async _showSelfModifyPasswordDialogAsync(): Promise<void> {
        const form = new SelfModifyPasswordForm();

        const dialog: Dialog = (await dialogController.showAsync("self-modify-user-password", {
            type: "info",
            width: 700,
            title: "修改密碼",
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

                        const loading = await dialogController.showLoadingAsync("儲存中", "正在更新密碼..");
                        const data = await form.getValueAsync();
                        const response = await apiController.queryAsync(selfManageApiMap.selfUpdateUserPassword, data);
                        await loading.closeAsync();
                        if (response.success) {
                            const success = await dialogController.showSuccessAsync("成功", "成功更新密碼。");
                            success.onCloseAsync = async () => dialog.closeAsync();
                        } else {
                            await dialogController.showErrorAsync("錯誤", `更新密碼時發生錯誤：${response.message}`);
                        }
                    }
                },
                { text: "取消" }
            ],
            onShowAsync: () => form.buildAsync(),
            onCloseAsync: () => form.deleteAsync()
        }))!;
    }

    private async _showSelfModifyKeybindingDialogAsync(): Promise<void> {
        const form = new UserKeybindingForm();

        const dialog: Dialog = (await dialogController.showAsync("self-modify-keybinding", {
            type: "info",
            width: 700,
            title: "修改字幕編輯器快速鍵",
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

                        const loading = await dialogController.showLoadingAsync("儲存中", "正在儲存快速鍵設定..");
                        const keybindings = await form.getValueAsync();
                        const response = await apiController.queryAsync(userMetaApiMap.saveKeybinding, { keybindings });
                        await loading.closeAsync();
                        if (response.success) {
                            const success = await dialogController.showSuccessAsync("成功", "成功儲存快速鍵設定！請重新整理畫面來套用新的快速鍵。");
                            success!.onCloseAsync = async () => dialog.closeAsync();
                        } else {
                            await dialogController.showErrorAsync("錯誤", `儲存快速鍵設定時發生錯誤：${response.message}`);
                        }
                    }
                },
                { text: "取消" },
                {
                    type: "success",
                    text: "還原",
                    align: "left",
                    callback: async () => {
                        if (await dialogController.confirmWarningAsync("確認", "您確定要還原快速鍵設定嗎？這個動作會將快速鍵設定清除並還原回最原始的預設值，一旦執行將沒辦法反悔。")) {
                            const loading = await dialogController.showLoadingAsync("還原中", "正在還原快速鍵設定..");
                            const response = await apiController.queryAsync(userMetaApiMap.recoverKeybinding);
                            await loading.closeAsync();
                            if (response.success) {
                                const success = await dialogController.showSuccessAsync("成功", "成功還原快速鍵設定！請重新整理畫面來套用新的快速鍵。");
                                success.onCloseAsync = async () => dialog.closeAsync();
                            } else {
                                await dialogController.showErrorAsync("錯誤", `還原快速鍵設定時發生錯誤：${response.message}`);
                            }
                        }
                    }
                }
            ],
            onShowAsync: async () => {
                await form.buildAsync();

                const loading = await dialogController.showLoadingAsync("載入中", "正在載入快速鍵設定..");
                const response = await apiController.queryAsync(userMetaApiMap.getKeybinding);
                await loading.closeAsync();

                if (response.success) {
                    await form.setValueAsync(response.data.keybindings);
                } else {
                    const errorDialog = await dialogController.showErrorAsync("錯誤", "載入快速鍵設定時發生錯誤！");
                    errorDialog.onCloseAsync = () => dialog.closeAsync();
                }
            },
            onCloseAsync: () => form.deleteAsync()
        }))!;
    }

    private async _showSelfModifyOptionsDialogAsync(): Promise<void> {
        const form = new UserOptionsForm();

        const dialog: Dialog = (await dialogController.showAsync("self-modify-options", {
            type: "info",
            width: 700,
            title: "修改喜好設定",
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

                        const loading = await dialogController.showLoadingAsync("儲存中", "正在儲存喜好設定..");
                        const request = await form.getValueAsync();
                        const response = await apiController.queryAsync(userMetaApiMap.saveSelfOptions, request);
                        await loading.closeAsync();
                        if (response.success) {
                            const success = await dialogController.showSuccessAsync("成功", "成功儲存喜好設定！");
                            success!.onCloseAsync = async () => dialog.closeAsync();
                        } else {
                            await dialogController.showErrorAsync("錯誤", `儲存喜好設定時發生錯誤：${response.message}`);
                        }
                    }
                },
                { text: "取消" }
            ],
            onShowAsync: async () => {
                await form.buildAsync();

                const loading = await dialogController.showLoadingAsync("載入中", "正在載入喜好設定..");
                const response = await apiController.queryAsync(userMetaApiMap.getSelfOptions);
                await loading.closeAsync();

                if (response.success) {
                    await form.setValueAsync(response.data);
                } else {
                    const errorDialog = await dialogController.showErrorAsync("錯誤", "載入喜好設定時發生錯誤！");
                    errorDialog.onCloseAsync = () => dialog.closeAsync();
                }
            },
            onCloseAsync: () => form.deleteAsync()
        }))!;
    }
}