import { logApiMap } from "contexts/logApiMap";
import { menuController } from "controllers/MenuController";
import LogDetailForm from "elements/forms/LogDetailForm";
import LogListCondition from "elements/models/log/LogListCondition";
import LogListPrimaryDataModel from "elements/models/log/LogListPrimaryDataModel";
import LogListResponse from "elements/models/log/LogListResponse";
import apiController from "uform-api";
import dialogController, { Dialog } from "uform-dialog";
import { ListPage } from "uform-page";
import selectorController, { Selector } from "uform-selector";
import Utility from "uform-utility";

export default class LogListPage extends ListPage {

    paths = ["/Log", "/Logs", "/Log/List"];
    tableBody?: HTMLTableSectionElement;
    loading?: HTMLDivElement;

    private _conditionTarget?: HTMLInputElement;
    private _conditionIPAddress?: HTMLInputElement;
    private _conditionUser?: HTMLInputElement;
    private _conditionActions?: HTMLInputElement;
    private _conditionSuccess?: HTMLInputElement;

    private _actionsSelector?: Selector;
    private _successSelector?: Selector;

    protected async loadAsync(): Promise<void> {
        menuController.activeMenu(this.paths);
        menuController.activeMenu("/Management");

        this.showLoadingBox(true);
        await this.initConditionInputAsync();
        await this.buildTableBodyAsync();
    }

    protected async initConditionInputAsync(): Promise<void> {
        this._conditionTarget = <HTMLInputElement>document.getElementById("condition-target");
        this._conditionIPAddress = <HTMLInputElement>document.getElementById("condition-ip");
        this._conditionUser = <HTMLInputElement>document.getElementById("condition-user");

        if (this._conditionTarget) {
            this._conditionTarget.parentElement!.style.width = "160px";
        }

        if (this._conditionIPAddress) {
            this._conditionIPAddress.parentElement!.style.width = "160px";
        }

        if (this._conditionUser) {
            this._conditionUser.parentElement!.style.width = "160px";
        }

        await this._initActionsAsync();
        await this._initSuccessAsync();
    }

    protected async buildTableBodyAsync(): Promise<void> {
        const condition: LogListCondition = {
            page: this.pageInput ? Number(this.pageInput.value) : 0,
            pageSize: this.pageSize ? Number(this.pageSize.value) : 10,
            orderColumn: this.orderColumnInput?.value ?? "",
            descending: this.descendingInput?.value === "True"
        };

        this.buildCondition(condition);

        this.showLoadingBox(true);
        const response = await apiController.queryAsync(logApiMap.list, condition);
        this.showLoadingBox(false);

        if (response.success) {
            if (this.tableContainer) {
                this.tableBody = <HTMLTableSectionElement>this.tableContainer.querySelector("tbody");

                const responseModel: LogListResponse = response.data;
    
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

    protected buildCondition(condition: LogListCondition): void {
        const period = this.getPeriod();
        condition.start = period.start;
        condition.end = period.end;
        condition.target = this._conditionTarget?.value;
        condition.ipAddress = this._conditionIPAddress?.value;
        condition.user = this._conditionUser?.value;
        condition.actions = this._conditionActions?.value;
        condition.isActionSuccess = this._conditionSuccess && this._conditionSuccess?.value && this._conditionSuccess.value !== "-1" ?
            this._conditionSuccess.value === "1" : undefined
    }

    protected buildTableRow(data: LogListPrimaryDataModel): void {
        const accountHtml = data.userAccount && data.userId ?
            `<a class="table-action user-account" href="/User/List?Keyword=${data.userAccount}">${data.userAccount}</a>` : 
            data.userAccount ?? data.userId ?? "-";

        const successColor = data.success ? "" : " danger form-theme-color";
        const message = data.message ?? "-";

        const row = document.createElement("tr");
        row.innerHTML = `
            <td align="center" class="nowrap${successColor}" width="78">${data.success ? `成功` : `失敗`}</td>
            <td align="left" width="160">${data.time}</td>
            <td align="left" class="nowrap">${accountHtml}</td>
            <td align="left" class="nowrap">${data.ipAddress ?? "-"}</td>
            <td align="left" class="table-list-items"><span class="table-list-item">${data.actionName}</span></td>
            <td align="left">${data.target ?? "-"}</td>
            <td align="left" class="word-break-all">${message.length > 100 ? `${message.substring(0, 100)}...` : message}</td>
            <td align="left" class="table-actions" width="92">
                <button class="primary detail-button small-button" type="button" title="點選以開啟詳細內容視窗"><span>詳細內容</span></button>
            </td>
            `;
            
        this.tableBody!.appendChild(row);

        const detailButton = <HTMLButtonElement>row.querySelector(".detail-button");
        detailButton.addEventListener("click", () => this._showDetailDialogAsync(data));
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

    protected getPeriod(): { start: string, end: string } {
        const startInput = <HTMLInputElement>document.getElementById("condition-start");
        const endInput = <HTMLInputElement>document.getElementById("condition-end");
        return {
            start: startInput ? startInput.value : "",
            end: endInput ? endInput.value : ""
        }
    }

    private async _initActionsAsync(): Promise<void> {
        this._conditionActions = <HTMLInputElement>document.getElementById("condition-actions");
        this._actionsSelector = selectorController.get(document.getElementById("selector-actions")!);

        await Utility.waitUntil(() => {
            this._actionsSelector = selectorController.get(document.getElementById("selector-actions")!);
            return !!this._actionsSelector;
        }, 100);
        
        if (this._actionsSelector) {
            this._actionsSelector.onChangeFuncs.push(() => {
                if (this._conditionActions) {
                    this._conditionActions.value = (<string[]>this._actionsSelector?.getValue() ?? ["-1"]).join(";");
                }
            });

            const value = this._conditionActions.value;
            this._actionsSelector.setValueAsync(value ? value.split(";") : ["-1"]);
        }
    }

    private async _initSuccessAsync(): Promise<void> {
        this._conditionSuccess = <HTMLInputElement>document.getElementById("condition-success");
        this._successSelector = selectorController.get(document.getElementById("selector-success")!);

        await Utility.waitUntil(() => {
            this._successSelector = selectorController.get(document.getElementById("selector-success")!);
            return !!this._successSelector;
        }, 100);

        if (this._successSelector) {
            this._successSelector.onChangeFuncs.push(() => {
                if (this._conditionSuccess) {
                    this._conditionSuccess.value = <string>this._successSelector?.getValue() ?? "-1";
                }
            });

            const value = this._conditionSuccess.value;
            this._successSelector.setValueAsync(value ? value.split(";") : ["-1"]);
        }
    }

    private async _showDetailDialogAsync(data: LogListPrimaryDataModel): Promise<void> {
        const form = new LogDetailForm();

        const dialog: Dialog = (await dialogController.showAsync("log-detail", {
            type: "info",
            width: "calc(100% - 40px)",
            title: "系統紀錄",
            body: form.element,
            headerCloseButton: true,
            buttons: [{
                type: "default",
                text: "關閉",
                callback: () => dialogController.closeAsync(dialog)
            }],
            onShowAsync: async () => {
                await form.buildAsync();
                await form.setValueAsync(data);
            },
            onCloseAsync: () => form.deleteAsync()
        }))!;
    }
}