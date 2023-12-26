import { systemStatusApiMap } from "contexts/systemStatusApiMap";
import SystemStatusModel from "elements/models/status/SystemStatusModel";
import apiController from "uform-api";
import dialogController from "uform-dialog";
import { ButtonElement, Form, FormElementContainer, NoteElement, ReadonlyPickerElement, TextAreaElement, TextElement, TitleElement } from "uform-form";

export default class StatusForm extends Form {

    private _asrKernelVersion: TextElement = undefined!;
    private _captionMakerVersion: TextElement = undefined!;
    private _videoSubtitleEditorVersion: TextElement = undefined!;
    private _totalWorkers: TextElement = undefined!;
    private _availableWorkers: TextElement = undefined!;
    private _asrStatus: ReadonlyPickerElement = undefined!;
    private _licenseExpiredTime: TextElement = undefined!;

    private _storageStatus: TextElement = undefined!;
    private _streamFileStatus: TextElement = undefined!;

    private _activationKeyInput: TextAreaElement = undefined!;
    private _activationSubmitButton: ButtonElement = undefined!;
    private _activationClearButton: ButtonElement = undefined!;
    private _activationKeyStatus: NoteElement = undefined!;
    private _activationKeyPublisher: TextElement = undefined!;
    private _activatedTarget: TextElement = undefined!;
    private _calCount: TextElement = undefined!;
    private _activationEnd: TextElement = undefined!;

    private _storageLimit: number = 0;
    private _streamFileLimit: number = 0;
    private _storageLength: number = 0;
    private _streamFileLength: number = 0;

    async buildChildrenAsync(): Promise<void> {
        await this._buildVersionAsync();
        await this._buildStatusAsync();
        await this._buildstorageStatusAsync();
        await this._buildActivationAsync();
    }

    private async _buildVersionAsync(): Promise<void> {
        const versionTitle = TitleElement.fromAsync({ text: "版本資訊" });
        await this.appendAsync(versionTitle);

        this._asrKernelVersion = await TextElement.fromAsync({
            label: "ASR核心"
        });
        await this.appendAsync(this._asrKernelVersion);

        this._captionMakerVersion = await TextElement.fromAsync({
            label: "字幕模組"
        });
        await this.appendAsync(this._captionMakerVersion);

        this._videoSubtitleEditorVersion = await TextElement.fromAsync({
            label: "編輯器"
        });
        await this.appendAsync(this._videoSubtitleEditorVersion);
    }

    private async _buildStatusAsync(): Promise<void> {
        const statusTitle = TitleElement.fromAsync({ text: "ASR 服務狀態" });
        await this.appendAsync(statusTitle);

        this._totalWorkers = await TextElement.fromAsync({
            label: "可執行任務總數"
        });
        await this.appendAsync(this._totalWorkers);

        this._availableWorkers = await TextElement.fromAsync({
            label: "現可執行任務數"
        });
        await this.appendAsync(this._availableWorkers);

        this._asrStatus = await ReadonlyPickerElement.fromAsync({
            label: "服務狀態",
            options: [{ text: "異常", value: "0" }, { text: "服務就緒", value: "1" }, { text: "無法連線", value: "400" }]
        });
        await this.appendAsync(this._asrStatus);

        this._licenseExpiredTime = await TextElement.fromAsync({
            label: "授權有效時間"
        });
        await this.appendAsync(this._licenseExpiredTime);
    }

    private async _buildstorageStatusAsync(): Promise<void> {
        const storageStatusTitle = TitleElement.fromAsync({ text: "容量狀態" });
        await this.appendAsync(storageStatusTitle);

        const storageStatusContainer = FormElementContainer.fromAsync({
            label: "原始檔案容量",
            buildChildrenFunc: async group => {
                this._storageStatus = await TextElement.fromAsync({
                    text: ""
                });
                await group.appendAsync(this._storageStatus);
            }
        });
        await this.appendAsync(storageStatusContainer);

        const streamStorageStatusContainer = FormElementContainer.fromAsync({
            label: "串流檔案容量",
            buildChildrenFunc: async group => {
                this._streamFileStatus = await TextElement.fromAsync({
                    text: ""
                });
                await group.appendAsync(this._streamFileStatus);
            }
        });
        await this.appendAsync(streamStorageStatusContainer);
    }

    private async _buildActivationAsync(): Promise<void> {
        const activationTitle = TitleElement.fromAsync({ text: "系統啟用" });
        await this.appendAsync(activationTitle);

        this._activationKeyInput = await TextAreaElement.fromAsync({
            label: "輸入啟用金鑰",
            rows: 3
        });
        await this.appendAsync(this._activationKeyInput);

        const activationButtonGroup = FormElementContainer.fromAsync({
            label: "",
            inline: true,
            buildChildrenFunc: async group => {
                this._activationSubmitButton = await ButtonElement.fromAsync({
                    text: "上傳金鑰",
                    type: "primary",
                    onClick: () => this._submitActivationKeyAsync()
                });
                await group.appendAsync(this._activationSubmitButton);
        
                this._activationClearButton = await ButtonElement.fromAsync({
                    text: "刪除金鑰",
                    hide: true,
                    type: "danger",
                    onClick: () => this._clearActivationKeyAsync()
                });
                await group.appendAsync(this._activationClearButton);
            }
        });
        await this.appendAsync(activationButtonGroup);

        this._activationKeyStatus = await NoteElement.fromAsync({
            label: "啟用狀態",
            width: "300px"
        });
        await this.appendAsync(this._activationKeyStatus);

        this._activationKeyPublisher = await TextElement.fromAsync({
            label: "發行者",
            hide: true
        });
        await this.appendAsync(this._activationKeyPublisher);

        this._activatedTarget = await TextElement.fromAsync({
            label: "授權對象",
            hide: true
        });
        await this.appendAsync(this._activatedTarget);

        this._calCount = await TextElement.fromAsync({
            label: "使用者上限",
            hide: true
        });
        await this.appendAsync(this._calCount);

        this._activationEnd = await TextElement.fromAsync({
            label: "有效日期",
            hide: true
        });
        await this.appendAsync(this._activationEnd);
    }

    async setValueAsync(value: SystemStatusModel): Promise<void> {
        await Promise.all([
            this._asrKernelVersion.setValueAsync(value.asrKernelVersion ?? "未知"),
            this._captionMakerVersion.setValueAsync(value.captionMakerVersion ?? "未知"),
            this._videoSubtitleEditorVersion.setValueAsync(value.videoSubtitleEditorVersion),
            this._totalWorkers.setValueAsync(value.totalWorkers !== null && value.totalWorkers !== undefined ? String(value.totalWorkers) : "未知"),
            this._availableWorkers.setValueAsync(value.availableWorkers !== null && value.availableWorkers !== undefined ? String(value.availableWorkers) : "未知"),
            this._asrStatus.setValueAsync(value.asrStatus !== null && value.asrStatus !== undefined ? String(value.asrStatus) : "400"),
            this._licenseExpiredTime.setValueAsync(value.licenseExpiredTime !== null && value.licenseExpiredTime !== undefined ? value.licenseExpiredTime : "未知")
        ]);

        await this._updateStorageStatusAsync(value);
        await this._updateActivationStatusAsync(value);
    }

    private async _updateStorageStatusAsync(value: SystemStatusModel): Promise<void> {
        this._storageLimit = value.storageLimit;
        this._storageLength = value.storageLength;
        this._streamFileLimit = value.streamFileLimit;
        this._streamFileLength = value.streamFileLength;
        
        const storageGigibyte = this._storageLength > 1073741824 || this._storageLimit > 1073741824;
        const storageValue = storageGigibyte ? this._storageLength / 1073741824 : this._storageLength / 1048576;
        const storageLimitValue = storageGigibyte ? this._storageLimit / 1073741824 : this._storageLimit / 1048576;
        const storageText = String(Math.round(storageValue * 100) / 100);

        if (this._storageLimit === 0) {
            await this._storageStatus.setValueAsync(`已使用 ${storageText} ${storageGigibyte ? "GB" : "MB"}`);
        } else {
            const unit = storageGigibyte ? "GB" : "MB";
            const percentage = String(Math.round(10000 * this._storageLength / this._storageLimit) / 100);
            await this._storageStatus.setValueAsync(`${storageText}/${storageLimitValue} ${unit} (${percentage}%)`);
        }

        const streamFileGigibyte = this._streamFileLength > 1073741824 || this._streamFileLimit > 1073741824;
        const streamFileValue = streamFileGigibyte ? this._streamFileLength / 1073741824 : this._streamFileLength / 1048576;
        const streamFileLimitValue = streamFileGigibyte ? this._streamFileLimit / 1073741824 : this._streamFileLimit / 1048576;
        const streamFileText = String(Math.round(streamFileValue * 100) / 100);

        if (this._streamFileLimit === 0) {
            await this._streamFileStatus.setValueAsync(`已使用 ${streamFileText} ${streamFileGigibyte ? "GB" : "MB"}`);
        } else {
            const unit = streamFileGigibyte ? "GB" : "MB";
            const percentage = String(Math.round(10000 * this._streamFileLength / this._streamFileLimit) / 100);
            await this._streamFileStatus.setValueAsync(`${streamFileText}/${streamFileLimitValue} ${unit} (${percentage}%)`);
        }
    }

    private async _updateActivationStatusAsync(value: SystemStatusModel): Promise<void> {
        if (value.activated) {
            this._activationKeyStatus.type = "success";
            this._activationKeyStatus.text = "已啟用";
            await this._activationKeyStatus.rebuildAsync();
            await this._activationClearButton.showAsync();
        } else {
            this._activationKeyStatus.type = "danger";
            this._activationKeyStatus.text = "尚未啟用";
            await this._activationKeyStatus.rebuildAsync();
        }

        if (value.activationKeyPublisher) {
            await this._activationKeyPublisher.showAsync();
            this._activationKeyPublisher.setValueAsync(value.activationKeyPublisher)
        }

        if (value.activatedTarget) {
            await this._activatedTarget.showAsync();
            this._activatedTarget.setValueAsync(value.activatedTarget);
        }

        if (value.activated && (value.calCount === undefined || value.calCount === null || value.calCount === 0)) {
            await this._calCount.showAsync();
            this._calCount.setValueAsync("無限制");
        } else if (value.calCount !== undefined && value.calCount !== null && value.calCount > 0) {
            await this._calCount.showAsync();
            this._calCount.setValueAsync(value.calCount);
        }

        if (value.activationEnd) {
            await this._activationEnd.showAsync();
            this._activationEnd.setValueAsync(value.activationEnd);
        }
    }

    private async _submitActivationKeyAsync(): Promise<void> {
        const key = await this._activationKeyInput.getValueAsync();
        if (!key) {
            await dialogController.showWarningAsync("錯誤", "請提供要提交的金鑰。");
            return;
        }

        const response = await apiController.queryAsync(systemStatusApiMap.setActivationKey, { key });
        if (response.success) {
            const dialog = await dialogController.showSuccessAsync("成功", "成功啟用系統！", 5000);
            dialog.onCloseAsync = async () => window.location.reload();
        } else {
            await dialogController.showErrorAsync("錯誤", `啟用時發生錯誤：${response.message}`);
        }
    }

    private async _clearActivationKeyAsync(): Promise<void> {
        if (!await dialogController.confirmWarningAsync("警告", "清除金鑰會導致系統失去授權，您確定要執行嗎？")) {
            return;
        }

        const response = await apiController.queryAsync(systemStatusApiMap.clearActivationKey);
        if (response.success) {
            const dialog = await dialogController.showSuccessAsync("成功", "成功清除金鑰！", 5000);
            dialog.onCloseAsync = async () => window.location.reload();
        } else {
            await dialogController.showErrorAsync("錯誤", `清除時發生錯誤：${response.message}`);
        }
    }
}