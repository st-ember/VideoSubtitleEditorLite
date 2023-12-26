import { systemActions } from "contexts/systemActions";
import { topicApiMap } from "contexts/topicApiMap";
import { menuController } from "controllers/MenuController";
import { permissionController } from "controllers/PermissionController";
import TopicBenchmarkForm from "elements/forms/TopicBenchmarkForm";
import TopicCreationForm from "elements/forms/TopicCreationForm";
import TopicUpdateForm from "elements/forms/TopicUpdateForm";
import BenchmarkRequest from "elements/models/benchmark/BenchmarkRequest";
import TopicListCondition from "elements/models/topic/TopicListCondition";
import TopicListData from "elements/models/topic/TopicListData";
import TopicListResponse from "elements/models/topic/TopicListResponse";
import apiController from "uform-api";
import dialogController, { Dialog } from "uform-dialog";
import dropdownController, { Dropdown } from "uform-dropdown";
import { ListPage } from "uform-page";
import popoverController from "uform-popover";
import selectorController, { Selector } from "uform-selector";
import Utility from "uform-utility";

export default class TopicListPage extends ListPage {

    paths = ["/Topics", "/Topic/Entry", "/Topic/List"];
    tableBody?: HTMLTableSectionElement;
    loading?: HTMLDivElement;

    private _createButton?: HTMLButtonElement;
    private _removeCheckedAnchor?: HTMLAnchorElement;
    private _archiveCheckedAnchor?: HTMLAnchorElement;

    private _conditionKeyword?: HTMLInputElement;
    private _conditionTopicStatus?: HTMLInputElement;
    private _conditionAsrStatus?: HTMLInputElement;
    private _conditionConvertStatus?: HTMLInputElement;
    private _topicStatusSelector?: Selector;
    private _asrStatusSelector?: Selector;
    private _convertStatusSelector?: Selector;
    private _checkAllCheckbox?: HTMLInputElement;
    private _checkboxs: { id: string, element: HTMLInputElement }[] = [];

    private _batchCommandButton?: HTMLButtonElement;
    private _batchCommandDropdown?: Dropdown;

    private _list: TopicListData[] = [];
    private _createDialogShowing: boolean = false;
    private _updateDialogShowing: boolean = false;
    private _benchmarkDialogShowing: boolean = false;
    private _checkedIds: string[] = [];

    private get _isAllChecked(): boolean { return this._checkboxs.filter(o => !o.element.checked).length === 0; }

    protected async loadAsync(): Promise<void> {
        menuController.activeMenu(this.paths);
        menuController.activeMenu(["/Topic"]);

        this._checkAllCheckbox = <HTMLInputElement>document.getElementById("data-check-all");
        this._checkAllCheckbox?.addEventListener("change", () => this._checkAll());

        this._createButton = <HTMLButtonElement>document.getElementById("create-button");
        this._createButton?.addEventListener("click", () => this._showCreateDialogAsync());

        this._removeCheckedAnchor = <HTMLAnchorElement>document.getElementById("remove-checked-anchor");
        this._removeCheckedAnchor?.addEventListener("click", () => this._batchRemoveAsync());

        this._archiveCheckedAnchor = <HTMLAnchorElement>document.getElementById("archive-checked-anchor");
        this._archiveCheckedAnchor?.addEventListener("click", () => this._batchArchiveAsync());

        if (this._removeCheckedAnchor || this._archiveCheckedAnchor) {
            const container = document.getElementById("batch-command-group")!;
            const body = <HTMLDivElement>container.querySelector(".dropdown-button-container")!;
            const button = document.createElement("button");
            button.type = "button";
            button.className = "success";
            button.innerHTML = `<i class="fa-solid fa-bolt-lightning"></i><span>批次動作</span>`;
            container.appendChild(button);
            this._batchCommandButton = button;

            this._batchCommandDropdown = dropdownController.add({
                target: button,
                content: body
            });
        }

        this.showLoadingBox(true);
        this._updateBatchButtonStatus();
        await this.initConditionInputAsync();
        await this.buildTableBodyAsync();
    }

    protected async initConditionInputAsync(): Promise<void> {
        this._conditionKeyword = <HTMLInputElement>document.getElementById("condition-keyword");

        if (this._conditionKeyword) {
            this._conditionKeyword.parentElement!.style.width = "300px";
        }

        await this._initTopicStatusAsync();
        //await this._initAsrStatusAsync();
        await this._initConvertStatusAsync();
    }

    protected async buildTableBodyAsync(): Promise<void> {
        const condition: TopicListCondition = {
            page: this.pageInput ? Number(this.pageInput.value) : 0,
            pageSize: this.pageSize ? Number(this.pageSize.value) : 10,
            orderColumn: this.orderColumnInput?.value ?? "",
            descending: this.descendingInput?.value === "True"
        };

        this.buildCondition(condition);

        this.showLoadingBox(true);
        const response = await apiController.queryAsync(topicApiMap.list, condition);
        this.showLoadingBox(false);

        if (response.success) {
            if (this.tableContainer) {
                this.tableBody = <HTMLTableSectionElement>this.tableContainer.querySelector("tbody");

                const responseModel: TopicListResponse = response.data;
    
                if (this.pageInput) {
                    this.pageInput.value = String(responseModel.page);
                }
    
                if (this.totalPageInput) {
                    this.totalPageInput.value = String(responseModel.totalPage);
                }
    
                if (responseModel.list && responseModel.list.length > 0) {
                    this._list = responseModel.list;
                    responseModel.list.forEach(data => this.buildTableRow(data));
                }
            }
        } else {
            await dialogController.showErrorAsync("錯誤", `列出資料時發生錯誤：${response.message}`);
        }
    }

    protected buildCondition(condition: TopicListCondition): void {
        const period = this.getPeriod();
        condition.start = period.start;
        condition.end = period.end;
        condition.keyword = this._conditionKeyword?.value;
        condition.topicStatus = this._conditionTopicStatus?.value ?? "-1";
        condition.asrMediaStatus = this._conditionAsrStatus?.value ?? "-1";
        condition.convertMediaStatus = this._conditionConvertStatus?.value ?? "-1";
    }

    protected buildTableRow(data: TopicListData): void {
        const row = document.createElement("tr");

        this._buildRowElements(data, row);
            
        if (this.tableBody) {
            this.tableBody.appendChild(row);
        }

        this._bindRowUpdateEvent(data, row);
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

    private async _initTopicStatusAsync(): Promise<void> {
        this._conditionTopicStatus = <HTMLInputElement>document.getElementById("condition-topic-status");
        this._topicStatusSelector = selectorController.get(document.getElementById("selector-topic-status")!);

        await Utility.waitUntil(() => {
            this._topicStatusSelector = selectorController.get(document.getElementById("selector-topic-status")!);
            return !!this._topicStatusSelector;
        }, 100);
        
        if (this._topicStatusSelector) {
            this._topicStatusSelector.onChangeFuncs.push(() => {
                if (this._conditionTopicStatus) {
                    this._conditionTopicStatus.value = <string>this._topicStatusSelector?.getValue() ?? "-1";
                }
            });

            const value = this._conditionTopicStatus.value;
            this._topicStatusSelector.setValueAsync(value ? value.split(";") : ["-1"]);
        }
    }

    //private async _initAsrStatusAsync(): Promise<void> {
    //    this._conditionAsrStatus = <HTMLInputElement>document.getElementById("condition-asr-status");
    //    this._asrStatusSelector = selectorController.get(document.getElementById("selector-asr-status")!);

    //    await Utility.waitUntil(() => {
    //        this._asrStatusSelector = selectorController.get(document.getElementById("selector-asr-status")!);
    //        return !!this._asrStatusSelector;
    //    }, 100);

    //    if (this._asrStatusSelector) {
    //        this._asrStatusSelector.onChangeFuncs.push(() => {
    //            if (this._conditionAsrStatus) {
    //                this._conditionAsrStatus.value = <string>this._asrStatusSelector?.getValue() ?? "-1";
    //            }
    //        });

    //        const value = this._conditionAsrStatus.value;
    //        this._asrStatusSelector.setValueAsync(value ? value.split(";") : ["-1"]);
    //    }
    //}

    private async _initConvertStatusAsync(): Promise<void> {
        this._conditionConvertStatus = <HTMLInputElement>document.getElementById("condition-convert-status");
        this._convertStatusSelector = selectorController.get(document.getElementById("selector-convert-status")!);

        await Utility.waitUntil(() => {
            this._convertStatusSelector = selectorController.get(document.getElementById("selector-convert-status")!);
            return !!this._convertStatusSelector;
        }, 100);

        if (this._convertStatusSelector) {
            this._convertStatusSelector.onChangeFuncs.push(() => {
                if (this._conditionConvertStatus) {
                    this._conditionConvertStatus.value = <string>this._convertStatusSelector?.getValue() ?? "-1";
                }
            });

            const value = this._conditionConvertStatus.value;
            this._convertStatusSelector.setValueAsync(value ? value.split(";") : ["-1"]);
        }
    }

    private _buildRowElements(data: TopicListData, row: HTMLTableRowElement): void {
        const checkboxTd = document.createElement("td");
        const checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.id = `data-check-${data.id}`;
        checkbox.addEventListener("change", () => this._updateCheckStatus());
        checkboxTd.appendChild(checkbox);

        this._checkboxs.push({ id: data.id, element: checkbox });
            
        row.innerHTML = `
            <td align="left" class="word-break-all">${data.name}</td>
            <td align="left" class="nowrap">${data.originalSize > 0 ? `${Math.round(data.originalSize / 1048576 * 10) / 10} MB` : "0.0 MB"}</td>
            <td align="left" class="nowrap">${data.size > 0 ? `${Math.round(data.size / 1048576 * 10) / 10} MB` : "0.0 MB"}</td>
            <td align="left" class="nowrap">${data.lengthText}</td>
            <td align="left" class="nowrap">${data.processTimeText}</td>
            <td align="left">${data.topicStatusText}</td>
            <td align="left" class="asr-media-status"></td>
            <td align="left" class="convert-media-status"></td>
            <td align="left" class="nowrap">${data.create}</td>
            <td align="left" class="table-actions" width="110"></td>
            `;

        row.insertBefore(checkboxTd, row.firstChild);

        const asrMediaStatusTd = <HTMLElement>row.querySelector(".asr-media-status");
        const convertMediaStatusTd = <HTMLElement>row.querySelector(".convert-media-status");
        const tableActionTd = <HTMLElement>row.querySelector(".table-actions");

        const updateButton = document.createElement("button");
        updateButton.type = "button";
        updateButton.className = "primary update-button small-button";
        updateButton.title = "點選以開啟修改視窗";
        updateButton.innerHTML = `<span>修改</span>`;
        updateButton.addEventListener("click", () => this._showUpdateDialogAsync(data.id))
        tableActionTd.appendChild(updateButton);

        if ((data.asrMediaStatus === 3 || data.asrMediaStatus === 5) && data.convertMediaStatus === 2) {
            const updateSubtitleAnchor = document.createElement("a");
            updateSubtitleAnchor.className = "button primary update-subtitle-button small-button";
            updateSubtitleAnchor.title = "點選以編輯字幕";
            updateSubtitleAnchor.innerHTML = `<span>編輯字幕</span>`;
            updateSubtitleAnchor.href = `/Editor?id=${data.id}`;
            tableActionTd.appendChild(updateSubtitleAnchor);
        }

        if (data.asrMediaStatus === 4) {
            const anchor = document.createElement("a");
            anchor.title = "點選以瀏覽錯誤訊息";
            anchor.className = "table-action media-error-anchor";
            anchor.innerHTML = data.asrMediaStatusText;
            anchor.addEventListener("click", () => this._showMediaErrorDialogAsync(data.mediaError))
            asrMediaStatusTd.appendChild(anchor);
        } else {
            asrMediaStatusTd.innerText = data.asrMediaStatus === 1 ? `${data.asrMediaStatusText}(${data.progress}%)` : data.asrMediaStatusText;
        }

        if (data.convertMediaStatus === 3) {
            const anchor = document.createElement("a");
            anchor.title = "點選以瀏覽錯誤訊息";
            anchor.className = "table-action media-error-anchor";
            anchor.innerHTML = data.convertMediaStatusText;
            anchor.addEventListener("click", () => this._showMediaErrorDialogAsync(data.mediaError))
            convertMediaStatusTd.appendChild(anchor);
        } else {
            convertMediaStatusTd.innerText = data.convertMediaStatusText;
        }

        const dropdownButton = document.createElement("button");
        dropdownButton.type = "button";
        dropdownButton.className = "small-button dark dropdown-button";
        dropdownButton.innerHTML = `<i class="fa-solid fa-caret-down"></i>其他動作`;
        tableActionTd.appendChild(dropdownButton);

        const dropdownBody = document.createElement("div");
        dropdownBody.className = "dropdown-button-container";

        let otherCommandCount = 0;

        if (data.topicStatus !== 2 && data.topicStatus !== 3 && permissionController.contains(systemActions.ArchiveTopic)) {
            this._addDropdownOption(dropdownBody, "", "封存", "封存此單集", `<i class="fa-solid fa-box-archive"></i>`).addEventListener("click", () => this._showArchiveDialogAsync(data.id));
            otherCommandCount++;
        }
        
        if (data.topicStatus === 2 && permissionController.contains(systemActions.RemoveTopic) && data.asrMediaStatus !== 1 && data.convertMediaStatus !== 1) {
            this._addDropdownOption(dropdownBody, "danger-hover", "移除", "移除此單集", `<i class="fa-regular fa-trash-can"></i>`).addEventListener("click", () => this._showRemoveDialogAsync(data.id));
            otherCommandCount++;
        }
        
        if (data.topicStatus === 2 && permissionController.contains(systemActions.SetTopicStatusToNormal)) {
            this._addDropdownOption(dropdownBody, "", "還原回正常", "將此單集還原回正常狀態", `<i class="fa-solid fa-arrow-rotate-left"></i>`).addEventListener("click", () => this._showSetNormalDialogAsync(data.id));
            otherCommandCount++;
        }

        if (permissionController.contains(systemActions.DoTopicConversionBenchmark) && (data.convertMediaStatus === 2 || data.convertMediaStatus === 3)) {
            this._addDropdownOption(dropdownBody, "", "轉檔測試", "使用此單集進行轉檔測試", `<i class="fa-solid fa-vial"></i>`).addEventListener("click", () => this._showBenchmarkDialogAsync(data.id));
            otherCommandCount++;
        }
        
        if (data.asrMediaStatus === 4 || data.convertMediaStatus === 3) {
            this._addDropdownOption(dropdownBody, "", "重新執行", "重新執行一次流程", `<i class="fa-solid fa-arrow-rotate-left"></i>`).addEventListener("click", () => this._showReExecuteDialogAsync(data.id));
            otherCommandCount++;
        }

        if (otherCommandCount === 0) {
            dropdownButton.remove();
        } else {
            dropdownController.add({
                target: dropdownButton,
                content: dropdownBody
            });
        }
    }

    private _addDropdownOption(dropdownBody: HTMLDivElement, className: string, title: string, description: string, iconHtml?: string): HTMLAnchorElement {
        const adoptedIconHtml = iconHtml ? iconHtml : `<span class="empty-icon"></span>`;
        const anchor = document.createElement("a");
        anchor.className = `dropdown-option${className ? ` ${className}` : ""}`;
        anchor.innerHTML = `${adoptedIconHtml}<span class="title">${title}</span><span class="desc">${description}</span>`;
        dropdownBody.appendChild(anchor);
        return anchor;
    }

    private _bindRowUpdateEvent(data: TopicListData, row: HTMLTableRowElement): void {
        if (data.topicStatus === 0 && (data.asrMediaStatus === 0 || data.asrMediaStatus === 1 || data.convertMediaStatus === 0 || data.convertMediaStatus === 1)) {
            const timer = setInterval(async () => {
                const response = await apiController.queryAsync(topicApiMap.getListItem, { id: data.id });
                if (response.success) {
                    const newData = <TopicListData>response.data;
                    if (Object.keys(newData).filter(key => (<any>data)[key] !== (<any>newData)[key]).length > 0) {
                        const dropdownButton = <HTMLButtonElement | null>row.querySelector(".dropdown-button");
                        if (dropdownButton) {
                            await dropdownController.removeAsync(dropdownButton);
                        }
                        
                        row.innerHTML = "";
                        this._buildRowElements(newData, row);
                    }
    
                    if (newData.topicStatus !== 0 || data.asrMediaStatus >= 1 && data.convertMediaStatus >= 1) {
                        clearInterval(timer);
                    }
                }
            }, 5000);
        }
    }

    private _checkAll(): void {
        const isAllChecked = this._isAllChecked;
        this._checkboxs.forEach(o => this._setChecked(o.id, !isAllChecked));
        this._updateCheckAllStatus();
        this._updateBatchButtonStatus();
    }

    private _setChecked(id: string, checked: boolean): void {
        const checkbox = this._checkboxs.filter(o => o.id === id)[0];
        if (checkbox) {
            checkbox.element.checked = checked;

            if (checkbox.element.checked && this._checkedIds.filter(id => id === checkbox.id).length === 0) {
                this._checkedIds.push(checkbox.id);
            } else if (!checkbox.element.checked && this._checkedIds.filter(id => id === checkbox.id).length > 0) {
                this._checkedIds = this._checkedIds.filter(id => id !== checkbox.id);
            }
        }
    }
    
    private _updateCheckStatus(): void {
        this._checkboxs.forEach(checkbox => {
            if (checkbox.element.checked && this._checkedIds.filter(id => id === checkbox.id).length === 0) {
                this._checkedIds.push(checkbox.id);
            } else if (!checkbox.element.checked && this._checkedIds.filter(id => id === checkbox.id).length > 0) {
                this._checkedIds = this._checkedIds.filter(id => id !== checkbox.id);
            }
        });

        this._updateCheckAllStatus();
        this._updateBatchButtonStatus();
    }

    private _updateCheckAllStatus(): void {
        if (this._checkAllCheckbox) {
            this._checkAllCheckbox.checked = this._isAllChecked;
        }
    }

    private _updateBatchButtonStatus(): void {
        const checked = this._checkedIds.length > 0;

        if (this._batchCommandButton) {
            this._batchCommandButton.disabled = !checked;
        }

        if (this._removeCheckedAnchor) {
            const disable = this._list.filter(item => this._checkedIds.indexOf(item.id) >= 0 && item.topicStatus === 2).length === 0;
            this._removeCheckedAnchor.classList[disable ? "add" : "remove"]("disabled");
        }

        if (this._archiveCheckedAnchor) {
            const disable = this._list.filter(item => this._checkedIds.indexOf(item.id) >= 0 && (item.topicStatus === 0 || item.topicStatus === 1)).length === 0;
            this._archiveCheckedAnchor.classList[disable ? "add" : "remove"]("disabled");
        }
    }

    private async _showMediaErrorDialogAsync(error?: string): Promise<void> {
        await dialogController.showWarningAsync("錯誤內容", error ?? "系統沒有記錄到有效的錯誤訊息。");
    }

    private async _showReExecuteDialogAsync(id: string): Promise<void> {
        if (await dialogController.confirmWarningAsync("確認", "您確定要重新執行一次此單集的流程嗎？")) {
            const loading = await dialogController.showLoadingAsync("設定中", "正在重新設定單集..");
            const response = await apiController.queryAsync(topicApiMap.reExecute, { id });
            await dialogController.closeAsync(loading);

            if (response.success) {
                const dialog = await dialogController.showSuccessAsync("成功", "已成功重新設定單集，此單集很快就會重新開始執行流程。", 5000);
                dialog!.onCloseAsync = async () => this.reload();
            } else {
                await dialogController.showErrorAsync("錯誤", `重新設定單集時發生錯誤：${response.message}`);
            }
        }
    }

    private async _showArchiveDialogAsync(id: string): Promise<void> {
        if (await dialogController.confirmWarningAsync("確認", "您確定要封存此單集嗎？")) {
            const loading = await dialogController.showLoadingAsync("封存中", "正在進行封存..");
            const response = await apiController.queryAsync(topicApiMap.archive, { id });
            await dialogController.closeAsync(loading);

            if (response.success) {
                const dialog = await dialogController.showSuccessAsync("成功", "已成功封存此單集。", 5000);
                dialog!.onCloseAsync = async () => this.reload();
            } else {
                await dialogController.showErrorAsync("錯誤", `封存單集時發生錯誤：${response.message}`);
            }
        }
    }

    private async _showRemoveDialogAsync(id: string): Promise<void> {
        if (await dialogController.confirmWarningAsync("確認", "您確定要移除此單集嗎？此動作沒辦法被還原，單集的檔案也會被刪除。")) {
            const loading = await dialogController.showLoadingAsync("移除中", "正在進行移除..");
            const response = await apiController.queryAsync(topicApiMap.remove, { id });
            await dialogController.closeAsync(loading);

            if (response.success) {
                const dialog = await dialogController.showSuccessAsync("成功", "已成功移除此單集。", 5000);
                dialog!.onCloseAsync = async () => this.reload();
            } else {
                await dialogController.showErrorAsync("錯誤", `移除單集時發生錯誤：${response.message}`);
            }
        }
    }

    private async _showSetNormalDialogAsync(id: string): Promise<void> {
        if (await dialogController.confirmWarningAsync("確認", "您確定要將此單集從封存狀態還原回「正常」嗎？")) {
            const loading = await dialogController.showLoadingAsync("修改狀態中", "正在修改單集的狀態..");
            const response = await apiController.queryAsync(topicApiMap.setNormal, { id });
            await dialogController.closeAsync(loading);

            if (response.success) {
                const dialog = await dialogController.showSuccessAsync("成功", "已成功修改單集狀態。", 5000);
                dialog!.onCloseAsync = async () => this.reload();
            } else {
                await dialogController.showErrorAsync("錯誤", `修改單集狀態時發生錯誤：${response.message}`);
            }
        }
    }

    private async _showCreateDialogAsync(): Promise<void> {
        if (this._createDialogShowing) {
            return;
        }

        this._createDialogShowing = true;

        const form = new TopicCreationForm();
        const dialog: Dialog = (await dialogController.showAsync("topic-create-dialog", {
            type: "info",
            width: 700,
            title: "建立新單集",
            body: form.element,
            headerCloseButton: true,
            buttons: [
                {
                    text: "建立",
                    type: "primary",
                    callback: async () => {
                        if (!await form.validateAsync()) {
                            return;
                        }

                        const data = await form.getValueAsync();
                        const loading = await dialogController.showLoadingAsync("建立中", "正在建立新單集..");
                        const response = await apiController.queryAsync(topicApiMap.create, data);
                        await dialogController.closeAsync(loading);

                        if (response.success) {
                            const successDialog = await dialogController.showSuccessAsync("成功", "成功建立新單集", 5000);
                            successDialog!.onCloseAsync = async () => {
                                await dialogController.closeAsync(dialog);
                                this.reload();
                            };
                        } else {
                            await dialogController.showErrorAsync("錯誤", `建立新單集時發生錯誤！錯誤資訊：${response.message}`);
                        }
                    }
                },
                {
                    text: "取消",
                    type: "default",
                    callback: () => dialogController.closeAsync(dialog)
                }
            ],
            onShowAsync: async () => await form.buildAsync(),
            onCloseAsync: async () => {
                await form.deleteAsync();
                this._createDialogShowing = false;
            }
        }))!;
    }

    private async _showUpdateDialogAsync(id: string): Promise<void> {
        if (this._updateDialogShowing) {
            return;
        }

        this._updateDialogShowing = true;

        const form = new TopicUpdateForm();
        form.id = id;

        const dialog: Dialog = (await dialogController.showAsync("topic-update-dialog", {
            type: "info",
            width: 700,
            title: "修改單集",
            body: form.element,
            headerCloseButton: true,
            buttons: [
                {
                    text: "儲存",
                    type: "primary",
                    callback: async () => {
                        if (!await form.validateAsync()) {
                            return;
                        }

                        const data = await form.getValueAsync();
                        const loading = await dialogController.showLoadingAsync("儲存中", "正在儲存單集資訊..");
                        const response = await apiController.queryAsync(topicApiMap.update, data);
                        await dialogController.closeAsync(loading);

                        if (response.success) {
                            const successDialog = await dialogController.showSuccessAsync("成功", "成功儲存單集資訊", 5000);
                            successDialog!.onCloseAsync = () => dialogController.closeAsync(dialog);
                        } else {
                            await dialogController.showErrorAsync("錯誤", "儲存單集資訊時發生錯誤！");
                        }
                    }
                },
                {
                    text: "關閉",
                    type: "default",
                    callback: () => dialogController.closeAsync(dialog)
                }
            ],
            onShowAsync: async () => {
                await form.buildAsync();

                const loading = await dialogController.showLoadingAsync("載入中", "正在載入單集資訊...");
                const response = await apiController.queryAsync(topicApiMap.get, { id });
                await dialogController.closeAsync(loading);

                if (response.success) {
                    form.setValueAsync(response.data);
                } else {
                    const errorDialog = await dialogController.showErrorAsync("錯誤", "載入單集資訊時發生錯誤！");
                    errorDialog!.onCloseAsync = () => dialogController.closeAsync(dialog);
                }
            },
            onCloseAsync: async () => {
                await form.deleteAsync();
                this._updateDialogShowing = false;
            }
        }))!;
    }

    private async _batchRemoveAsync(): Promise<void> {
        if (this._batchCommandDropdown) {
            this._batchCommandDropdown.openAsync(false);
        }

        if (!await dialogController.confirmWarningAsync("確認", "您確定要移除勾選的單集嗎？此動作沒辦法被還原，單集的檔案也會被刪除。")) {
            return;
        }
        
        const loading = await dialogController.showLoadingAsync("移除中", `正在移除 ${this._checkedIds.length} 個項目...`);
        const response = await apiController.queryAsync(topicApiMap.batchRemove, { ids: this._checkedIds });
        await dialogController.closeAsync(loading);

        if (response.success) {
            const successDialog = await dialogController.showSuccessAsync("成功", `已成功移除 ${this._checkedIds.length} 個項目。`, 5000);
            successDialog!.onCloseAsync = async () => this.reload();
        } else {
            await dialogController.showErrorAsync("錯誤", `移除時發生錯誤：${response.message}`);
        }
    }

    private async _batchArchiveAsync(): Promise<void> {
        const loading = await dialogController.showLoadingAsync("封存中", `正在封存 ${this._checkedIds.length} 個項目...`);
        const response = await apiController.queryAsync(topicApiMap.batchArchive, { ids: this._checkedIds });
        await dialogController.closeAsync(loading);

        if (response.success) {
            const successDialog = await dialogController.showSuccessAsync("成功", `已成功封存 ${this._checkedIds.length} 個項目。`, 5000);
            successDialog!.onCloseAsync = async () => this.reload();
        } else {
            await dialogController.showErrorAsync("錯誤", `封存時發生錯誤：${response.message}`);
        }
    }

    private async _showBenchmarkDialogAsync(id: string): Promise<void> {
        if (this._benchmarkDialogShowing) {
            return;
        }

        this._benchmarkDialogShowing = true;

        const form = new TopicBenchmarkForm();
        form.id = id;

        const dialog: Dialog = (await dialogController.showAsync("topic-benchmark-dialog", {
            type: "info",
            width: 700,
            title: "轉檔測試",
            body: form.element,
            headerCloseButton: true,
            buttons: [
                {
                    text: "執行",
                    type: "primary",
                    callback: async () => {
                        const data = await form.getValueAsync();
                        const request: BenchmarkRequest = { id, argumentTemplate: data.argumentTemplate };

                        await form.clearResultAsync();

                        const loading = await dialogController.showLoadingAsync("執行中", "正在執行轉檔測試，這會需要一段時間 (數分鐘到數十分鐘)..");
                        const response = await apiController.queryAsync(topicApiMap.doTopicConversionBenchmark, request);
                        await dialogController.closeAsync(loading);

                        if (response.success) {
                            await form.setResultAsync(response.data);
                        } else {
                            await form.setErrorAsync(response.message ?? "不明的錯誤");
                        }
                    }
                },
                {
                    text: "關閉",
                    type: "default",
                    callback: () => dialogController.closeAsync(dialog)
                }
            ],
            onShowAsync: () => form.buildAsync(),
            onCloseAsync: async () => {
                await form.deleteAsync();
                this._benchmarkDialogShowing = false;
            }
        }))!;
    }
}