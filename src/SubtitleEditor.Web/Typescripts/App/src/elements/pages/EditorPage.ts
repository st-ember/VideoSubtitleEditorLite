import { apiController } from "ApiController";
import { dialogController } from "DialogController";
import SubtitleEditor, { RecreateTimeCommand, SaveCommand } from "SubtitleEditor";
import { keybindingCheckCommandMap, keybindingCommandMap } from "contexts/keybindingCommandMap";
import { systemActions } from "contexts/systemActions";
import { topicApiMap } from "contexts/topicApiMap";
import { userMetaApiMap } from "contexts/userMetaApiMap";
import { fileController } from "controllers/FileController";
import { menuController } from "controllers/MenuController";
import { permissionController } from "controllers/PermissionController";
import { EncodingSelectionForm } from "elements/forms/EncodingSelectionForm";
import UploadSubtitleForm from "elements/forms/UploadSubtitleForm";
import UploadTranscriptForm from "elements/forms/UploadTranscriptForm";
import TopicSubtitleData from "elements/models/topic/TopicSubtitleData";
import UserKeybinding from "elements/models/userMeta/UserKeybinding";
import { Dialog } from "uform-dialog";
import dropdownController, { Dropdown } from "uform-dropdown";
import { KeycodeElement } from "uform-form-keycode";
import { Page } from "uform-page";

export default class EditorPage extends Page {

    paths = ["/Editor"];

    private _id: string = "";
    private _subtitleEditor: SubtitleEditor = new SubtitleEditor();
    private _topicSubtitleData?: TopicSubtitleData;

    private _controlBar: HTMLDivElement = document.createElement("div");
    private _editorContainer: HTMLDivElement = document.createElement("div");

    private _keybindings: UserKeybinding[] = [];
    private _listenStates: { key: string }[] = [];
    private _inKeyboardEvent: boolean = false;
    private _keyboardListening: boolean = true;
    private _keybindingCommandKeys: string[] = Object.keys(keybindingCommandMap);

    private _downloadCommandDropdown?: Dropdown;
    private _advanceCommandDropdown?: Dropdown;

    private _showKeyboardLog: boolean = false;

    protected async loadAsync(): Promise<void> {
        menuController.activeMenu(this.paths);

        const queryArray = window.location.search.split("?")[1].split("&");
        const idQueries = queryArray.filter(text => text.split("=")[0].toLowerCase() === "id");
        const id = idQueries.length > 0 ? idQueries[0].split("=")[1] : undefined;
        if (!id || id.length < 32) {
            window.location.href = "/";
            return;
        }

        const loading = await dialogController.showLoadingAsync("載入中", "正在載入字幕資料..");
        const response = await apiController.queryAsync(topicApiMap.getSubtitle, { id });
        await dialogController.closeAsync(loading);

        if (response.success) {
            this._id = id;
            this._topicSubtitleData = <TopicSubtitleData>response.data;
            this._buildControlBar();
            this._buildEditor(id);
            await this._bindKeybindingAsync();
            this._replaceEditorKeyCodeSpans();
        } else {
            const errorDialog = await dialogController.showErrorAsync("錯誤", `載入字幕時發生錯誤！錯誤訊息：${response.message}`);
            errorDialog!.onCloseAsync = async () => { window.location.href = "/"; }
        }
    }

    private _buildControlBar(): void {
        this.pageContainer!.appendChild(this._controlBar);
        this._controlBar.className = "control-bar";

        const topicNameGroup = document.createElement("div");
        topicNameGroup.className = "control-group";
        topicNameGroup.innerHTML = `<span class="title">單集名稱</span>`;
        this._controlBar.appendChild(topicNameGroup);

        const name = document.createElement("span");
        name.className = "control-item";
        name.innerText = this._topicSubtitleData!.name;
        name.title = this._topicSubtitleData!.name;
        name.style.maxWidth = "600px";
        topicNameGroup.appendChild(name);
        
        const basicCommandGroup = document.createElement("div");
        basicCommandGroup.className = "control-group";
        this._controlBar.appendChild(basicCommandGroup);

        const saveButton = document.createElement("button");
        saveButton.className = "small-button primary";
        saveButton.title = "儲存逐字稿";
        saveButton.innerHTML = `<i class="fa fa-save"></i><span>儲存</span>`;
        basicCommandGroup.appendChild(saveButton);
        saveButton.addEventListener("click", () => this._saveAsync());

        this._buildDownloadCommandGroup();
        this._buildAdvanceCommandGroup();
    }

    private _buildDownloadCommandGroup(): void {
        let downloadCommandGroupCount = 0;

        const downloadCommandGroup = document.createElement("div");
        downloadCommandGroup.className = "control-group";
        this._controlBar.appendChild(downloadCommandGroup);

        const dropdownButton = document.createElement("button");
        dropdownButton.type = "button";
        dropdownButton.className = "small-button dark";
        dropdownButton.innerHTML = `<i class="fa-solid fa-caret-down"></i>下載/匯出`;
        downloadCommandGroup.appendChild(dropdownButton);

        const dropdownBody = document.createElement("div");
        dropdownBody.className = "dropdown-button-container";

        if (permissionController.contains(systemActions.ExportSubtitle)) {
            this._addDropdownOption(dropdownBody, "", "SRT 字幕", "", `<i class="fa-solid fa-arrow-down"></i>`)
                .addEventListener("click", () => this._exportSubtitleAsync("srt"));

            this._addDropdownOption(dropdownBody, "", "VTT 字幕", "", `<i class="fa-solid fa-arrow-down"></i>`)
                .addEventListener("click", () => this._exportSubtitleAsync("vtt"));

            this._addDropdownOption(dropdownBody, "", "Inline 字幕", "時間戳記與文字內容位於同一行內，時間戳記小於一秒的部分以分號分隔。", `<i class="fa-solid fa-arrow-down"></i>`)
                .addEventListener("click", () => this._exportSubtitleAsync("inline"));

            this._addDropdownOption(dropdownBody, "", "無時戳字幕", "每一句話一行，但未加上時戳。", `<i class="fa-solid fa-arrow-down"></i>`)
                .addEventListener("click", () => this._exportSubtitleAsync("noTime"));

            downloadCommandGroupCount++;
        }

        if (this._topicSubtitleData!.transcript && permissionController.contains(systemActions.ExportTranscript)) {
            this._addDropdownOption(dropdownBody, "", "逐字稿", "", `<i class="fa-solid fa-arrow-down"></i>`)
                .addEventListener("click", () => this._exportTranscriptAsync());
            downloadCommandGroupCount++;
        }

        if (permissionController.contains(systemActions.DownloadRawFile)) {
            this._addDropdownOption(dropdownBody, "", "原始影片", "", `<i class="fa-solid fa-arrow-down"></i>`)
                .addEventListener("click", () => this._downloadRawFile());
            downloadCommandGroupCount++;
        }

        if (downloadCommandGroupCount === 0) {
            downloadCommandGroup.remove();
        } else {
            this._downloadCommandDropdown = dropdownController.add({
                target: dropdownButton,
                content: dropdownBody
            });
        }
    }

    private _buildAdvanceCommandGroup(): void {
        let advCommandGroupCount = 0;

        const advCommandGroup = document.createElement("div");
        advCommandGroup.className = "control-group";
        this._controlBar.appendChild(advCommandGroup);

        const dropdownButton = document.createElement("button");
        dropdownButton.type = "button";
        dropdownButton.className = "small-button dark";
        dropdownButton.innerHTML = `<i class="fa-solid fa-caret-down"></i>進階動作`;
        advCommandGroup.appendChild(dropdownButton);

        const dropdownBody = document.createElement("div");
        dropdownBody.className = "dropdown-button-container";

        if (permissionController.contains(systemActions.ReproduceSubtitle)) {
            this._addDropdownOption(dropdownBody, "danger-hover", "重新辨識", "將單集音檔送回辨識流程重新辨識一次。", `<i class="fa-solid fa-arrow-rotate-left"></i>`)
                .addEventListener("click", () => this._reproduceSubtitleAsync());

            this._addDropdownOption(dropdownBody, "danger-hover", "重新載入", "重新載入一次辨識結果。", `<i class="fa-solid fa-arrow-rotate-left"></i>`)
                .addEventListener("click", () => this._reloadSubtitleAsync());

            advCommandGroupCount++;
        }

        if (permissionController.contains(systemActions.RecreateTimecode)) {
            this._addDropdownOption(dropdownBody, "danger-hover", "重製時間", "將現有的字幕時間去除並以人工方式重新加上時間戳記。", `<i class="fa-solid fa-clock-rotate-left"></i>`)
                .addEventListener("click", () => this._recreateTimeAsync());
            advCommandGroupCount++;
        }

        if (permissionController.contains(systemActions.RecoverToOriginal)) {
            this._addDropdownOption(dropdownBody, "danger-hover", "還原", "將單集字幕與逐字稿還原成一開始建立的樣子。", `<i class="fa-solid fa-arrow-rotate-left"></i>`)
                .addEventListener("click", () => this._recoverToOriginalAsync());
            advCommandGroupCount++;
        }

        if (permissionController.contains(systemActions.ReuploadSubtitle)) {
            this._addDropdownOption(dropdownBody, "danger-hover", "上傳字幕", "上傳一份字幕來取代現有的字幕及逐字稿。")
                .addEventListener("click", () => this._showReuploadSubtitleDialogAsync());
            advCommandGroupCount++;
        }

        if (permissionController.contains(systemActions.ReuploadTranscript)) {
            this._addDropdownOption(dropdownBody, "danger-hover", "上傳逐字稿", "上傳一份逐字稿來取代現有的，並清空現有的字幕。")
                .addEventListener("click", () => this._showReuploadTranscriptDialogAsync());
            advCommandGroupCount++;
        }

        if (advCommandGroupCount === 0) {
            advCommandGroup.remove();
        } else {
            this._advanceCommandDropdown = dropdownController.add({
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

    private async _bindKeybindingAsync(): Promise<void> {
        const response = await apiController.queryAsync(userMetaApiMap.getKeybinding);
        if (!response.success) {
            await dialogController.showErrorAsync("錯誤", "載入使用者快速鍵時發生錯誤！");
            return;
        }

        this._keybindings = (<UserKeybinding[]>response.data.keybindings)
            .map(({ action, keyCodes, withCtrl, withShift, withAlt }) => { 
                return { action, keyCodes: keyCodes.map(k => k.toUpperCase()), withCtrl, withShift, withAlt };
            });

        keybindingCommandMap["Save"] = () => this._saveAsync();

        this._onKeyDown = this._onKeyDown.bind(this);
        this._onKeyUp = this._onKeyUp.bind(this);
        this._onWindowBlur = this._onWindowBlur.bind(this);
        window.addEventListener("keydown", this._onKeyDown);
        window.addEventListener("keyup", this._onKeyUp);
        window.addEventListener("blur", this._onWindowBlur);
    }

    private async _onKeyDown(keyboardEvent: KeyboardEvent): Promise<void> {
        if (dialogController.anyDialogShowing || !this._keyboardListening) {
            return;
        }

        if (keyboardEvent.key === ' ' && keyboardEvent.altKey) {
            alert("test")
        }

        if (keyboardEvent.key === "Enter" || keyboardEvent.key === "Alt") {
            keyboardEvent.preventDefault();
        }

        if (this._showKeyboardLog) {
            console.log(`KeyDown= ${keyboardEvent.key}`);
        }

        if (keyboardEvent.key === "Meta") {
            this._listenStates = [];
            this._inKeyboardEvent = false;
            return;
        }

        if (!this._inKeyboardEvent) {
            this._inKeyboardEvent = true;
            this._listenStates = [];
        }

        const matchs = this._listenStates.filter(o => o.key === keyboardEvent.key);
        if (matchs.length === 0) {
            let adoptedKey = keyboardEvent.key.toUpperCase();
            if (adoptedKey === ' ' || adoptedKey === 'PROCESS') {
                adoptedKey = "SPACE";
            }

            this._listenStates.push({ key: adoptedKey });
        }

        if (this._showKeyboardLog) {
            console.log(`ListenStates= ${JSON.stringify(this._listenStates)}`);
        }

        await this._checkKeybindingMappingAsync(keyboardEvent);
    }

    private async _onKeyUp(keyboardEvent: KeyboardEvent): Promise<void> {
        if (dialogController.anyDialogShowing || !this._keyboardListening || keyboardEvent.key === "Meta") {
            this._listenStates = [];
            this._inKeyboardEvent = false;
            return;
        }

        if (keyboardEvent.key === "Enter" || keyboardEvent.key === "Alt") {
            keyboardEvent.preventDefault();
        }

        if (this._showKeyboardLog) {
            console.log(`KeyUp= ${keyboardEvent.key}`);
        }

        let adoptedKey = keyboardEvent.key.toUpperCase();
        if (adoptedKey === ' ') {
            adoptedKey = "SPACE";
        }

        const matchs = this._listenStates.filter(o => o.key === adoptedKey);
        if (matchs.length > 0) {
            this._listenStates = this._listenStates.filter(o => o.key !== adoptedKey);
        }

        if (this._listenStates.length === 0) {
            this._inKeyboardEvent = false;
        }

        if (this._showKeyboardLog) {
            console.log(`ListenStates= ${JSON.stringify(this._listenStates)}`);
        }
    }

    private _onWindowBlur(): void {
        this._listenStates = [];
        this._inKeyboardEvent = false;
    }

    private async _checkKeybindingMappingAsync(keyboardEvent: KeyboardEvent): Promise<void> {
        const keycodeData = {
            keyCodes: this._listenStates.filter(o => o.key !== "CONTROL" && o.key !== "SHIFT" && o.key !== "ALT").map(o => o.key.toUpperCase()),
            withCtrl: this._listenStates.filter(o => o.key === "CONTROL").length > 0,
            withShift: this._listenStates.filter(o => o.key === "SHIFT").length > 0,
            withAlt: this._listenStates.filter(o => o.key === "ALT").length > 0
        };
        
        const matchedKeybindings = this._keybindings.filter(keybinding => {
            return keybinding.keyCodes.length > 0 &&
                keybinding.keyCodes.map(o => o.toUpperCase()).filter(o => keycodeData.keyCodes.indexOf(o) >= 0).length === keybinding.keyCodes.length &&
                keybinding.withCtrl === keycodeData.withCtrl &&
                keybinding.withShift === keycodeData.withShift &&
                keybinding.withAlt === keycodeData.withAlt;
        });

        if (this._showKeyboardLog) {
            console.log(`Matched keys= ${JSON.stringify(matchedKeybindings)}`);
        }

        const orderedKeybindings = matchedKeybindings.sort((a, b) => {
            return this._keybindingCommandKeys.indexOf(a.action) - this._keybindingCommandKeys.indexOf(b.action);
        });

        for (let i = 0; i < orderedKeybindings.length; i++) {
            const keybinding = orderedKeybindings[i];

            const action = keybinding.action;
            const checkFunc = keybindingCheckCommandMap[action];
            if (checkFunc && !await checkFunc(this._subtitleEditor)) {
                continue;
            }

            keyboardEvent.preventDefault();
            
            const invokeFunc = keybindingCommandMap[action];
            if (invokeFunc) {
                await invokeFunc(this._subtitleEditor);
                break;
            }
        }
    }

    private _replaceEditorKeyCodeSpans(): void {
        const quickCreateLineKeybindings = this._keybindings.filter(keybinding => keybinding.action === "QuickCreateLine");
        if (quickCreateLineKeybindings.length > 0) {
            const keycodeHtml = KeycodeElement.convertKeycodeDataToHtml(quickCreateLineKeybindings[0]);
            const elems = document.querySelectorAll(".quick-create-line-key");
            for (let i = 0; i < elems.length; i++) {
                const elem = elems.item(i);
                elem.innerHTML = keycodeHtml;
            }
        }
    }

    private _buildEditor(id: string): void {
        const topicSubtitleData = this._topicSubtitleData!;

        this.pageContainer!.appendChild(this._editorContainer);
        this._editorContainer.className = "editor-container";

        this._subtitleEditor.buildAsync({
            container: this._editorContainer,
            subtitles: [
                {
                    video: {
                        source: `/stream/get/${id}`,
                        name: topicSubtitleData.name
                    },
                    lines: topicSubtitleData.subtitle.lines,
                    transcript: topicSubtitleData.transcript,
                    modifiedStates: topicSubtitleData.subtitle.modifiedStates
                }
            ],
            topic: {
                frameRate: topicSubtitleData.frameRate,
                wordLimit: topicSubtitleData.wordLimit
            },
            hooks: {
                onHotkeyStateChange: async (listensin) => this._setHotkeyListeningState(listensin),
                onLimitationChange: () => this._saveWordLimitAsync()
            }
        });
    }

    private _setHotkeyListeningState(listensing: boolean): void {
        if (this._keyboardListening !== listensing) {
            this._keyboardListening = listensing;
        }
    }

    private async _saveAsync(): Promise<void> {
        await SaveCommand.invokeAsync(this._subtitleEditor);
        const subtitleData = this._subtitleEditor.currentSubtitle?.data;
        if (subtitleData) {
            const request = {
                id: this._id,
                lines: subtitleData.lines,
                modifiedStates: subtitleData.modifiedStates
            };

            const loading = await dialogController.showLoadingAsync("儲存中", "正在儲存中..");
            const response = await apiController.queryAsync(topicApiMap.updateSubtitle, request);
            await dialogController.closeAsync(loading);

            if (response.success) {
                await dialogController.showSuccessAsync("成功", "已成功儲存資料！", 5000);
            } else {
                await dialogController.showErrorAsync("錯誤", `儲存時發生錯誤：${response.message}`);
            }
        }
    }

    private async _saveWordLimitAsync(): Promise<void> {
        const wordLimit = this._subtitleEditor.wordLimit;
        await apiController.queryAsync(topicApiMap.updateWordLimit, { id: this._id, wordLimit });
    }

    private async _exportSubtitleAsync(format: string): Promise<void> {
        if (this._downloadCommandDropdown) {
            this._downloadCommandDropdown.openAsync(false);
        }

        const encoding = await this._selectEncodingAsync();
        if (!encoding) {
            return;
        }

        const topicSubtitleData = this._topicSubtitleData!;

        const filenameArray = topicSubtitleData.filename.split(".");
        if (filenameArray.length > 1) {
            filenameArray.splice(filenameArray.length - 1, 1);
        }

        const filenameWithoutExtension = filenameArray.join(".");
        const adoptedFormat = format.toLowerCase().trim();
        const extension = adoptedFormat === "srt" ? "srt" : adoptedFormat === "vtt" ? "vtt" : "txt";
        await fileController.generateFileAsync(topicApiMap.exportSubtitle, { id: this._id, format, encoding }, `${filenameWithoutExtension}.${extension}`);
    }

    private async _exportTranscriptAsync(): Promise<void> {
        if (this._downloadCommandDropdown) {
            this._downloadCommandDropdown.openAsync(false);
        }

        const encoding = await this._selectEncodingAsync();
        if (!encoding) {
            return;
        }

        const topicSubtitleData = this._topicSubtitleData!;

        const filenameArray = topicSubtitleData.filename.split(".");
        if (filenameArray.length > 1) {
            filenameArray.splice(filenameArray.length - 1, 1);
        }

        const filenameWithoutExtension = filenameArray.join(".");
        await fileController.generateFileAsync(topicApiMap.exportTranscript, { id: this._id, encoding }, `${filenameWithoutExtension}.txt`);
    }

    private _downloadRawFile(): void {
        if (this._downloadCommandDropdown) {
            this._downloadCommandDropdown.openAsync(false);
        }

        const topicSubtitleData = this._topicSubtitleData!;
        window.location.href = `${topicApiMap.downloadRawFile.url}?id=${this._id}&filename=${encodeURIComponent(topicSubtitleData.filename)}`;
    }

    private async _reproduceSubtitleAsync(): Promise<void> {
        if (this._advanceCommandDropdown) {
            this._advanceCommandDropdown.openAsync(false);
        }

        if (await dialogController.confirmWarningAsync("確認", "您確定要重新辨識此單集嗎？所有現有的資料將被清除，系統會使用當初上傳的媒體檔案重新執行一次辨識流程。此作業無法被復原！")) {
            const loading = await dialogController.showLoadingAsync("處理中", "正在重新取得檔案及修改狀態..");
            const response = await apiController.queryAsync(topicApiMap.reproduceSubtitle, { id: this._id });
            await dialogController.closeAsync(loading);

            if (response.success) {
                const dialog = await dialogController.showSuccessAsync("成功", "成功將此單集重新加入辨識佇列中，畫面將回到單集清單。", 5000);
                dialog!.onCloseAsync = async () => { window.location.href = "/" };
            } else {
                await dialogController.showErrorAsync("錯誤", `嘗試將此單集加入重新辨識佇列時發生錯誤：${response.message}`);
            }
        }
    }

    private async _reloadSubtitleAsync(): Promise<void> {
        if (this._advanceCommandDropdown) {
            this._advanceCommandDropdown.openAsync(false);
        }

        if (await dialogController.confirmWarningAsync("確認", "您確定要重新載入此單集的辨識結果嗎？所有現有的資料將被清除，此作業無法被復原！")) {
            const loading = await dialogController.showLoadingAsync("處理中", "正在取得辨識結果並重新套用中..");
            const response = await apiController.queryAsync(topicApiMap.reloadSubtitle, { id: this._id });
            await dialogController.closeAsync(loading);

            if (response.success) {
                const dialog = await dialogController.showSuccessAsync("成功", "成功重新載入此單集的辨識結果。", 5000);
                dialog!.onCloseAsync = async () => { window.location.reload(); };
            } else {
                await dialogController.showErrorAsync("錯誤", `嘗試重新載入此單集的辨識結果時發生錯誤：${response.message}`);
            }
        }
    }

    private async _recreateTimeAsync(): Promise<void> {
        if (this._advanceCommandDropdown) {
            this._advanceCommandDropdown.openAsync(false);
        }

        if (await dialogController.confirmWarningAsync("確認", "您確定要重製時間嗎？所有字幕的時間戳記將被移除，您需要從逐字稿重新建立所有的字幕。此作業可透過「上一步」復原。")) {
            const loading = await dialogController.showLoadingAsync("進行中", "正在移除時間戳記..");
            await RecreateTimeCommand.invokeAsync(this._subtitleEditor);
            await dialogController.closeAsync(loading);
        }
    }

    private async _recoverToOriginalAsync(): Promise<void> {
        if (this._advanceCommandDropdown) {
            this._advanceCommandDropdown.openAsync(false);
        }

        if (await dialogController.confirmWarningAsync("確認", "您確定要將此單集的字幕與逐字稿還原成一開始建立的樣子嗎？現在的資料將被清除，且此作業無法被復原！")) {
            const loading = await dialogController.showLoadingAsync("處理中", "正在還原..");
            const response = await apiController.queryAsync(topicApiMap.recoverToOriginal, { id: this._id });
            await dialogController.closeAsync(loading);

            if (response.success) {
                const dialog = await dialogController.showSuccessAsync("成功", "成功還原此單集。", 5000);
                dialog!.onCloseAsync = async () => { window.location.reload(); };
            } else {
                await dialogController.showErrorAsync("錯誤", `還原此單集時發生錯誤：${response.message}`);
            }
        }
    }

    private async _showReuploadSubtitleDialogAsync(): Promise<void> {
        if (this._advanceCommandDropdown) {
            this._advanceCommandDropdown.openAsync(false);
        }

        const form = new UploadSubtitleForm();

        const dialog: Dialog = (await dialogController.showAsync("reupload-subtitle", {
            type: "info",
            title: "重新上傳字幕",
            width: 500,
            body: form.element,
            headerCloseButton: true,
            buttons: [
                {
                    text: "提交新字幕",
                    type: "warning",
                    callback: async () => {
                        if (!await form.validateAsync()) {
                            return;
                        }

                        if (!await dialogController.confirmWarningAsync("確認", "您確定要將目前單集的字幕取代成新上傳的這份嗎？現在的所有資料將被清除，且此作業無法被還原！")) {
                            return;
                        }

                        const data = await form.getValueAsync();
                        if (!data) {
                            return;
                        }

                        const loading = await dialogController.showLoadingAsync("處理中", "正在處理上傳的字幕資訊..");
                        const response = await apiController.queryAsync(topicApiMap.reuploadSubtitle, { id: this._id, ticket: data.ticket, frameRate: data.frameRate });
                        await dialogController.closeAsync(loading);

                        if (response.success) {
                            const successDialog = await dialogController.showSuccessAsync("成功", "成功上傳並更新字幕", 5000);
                            successDialog!.onCloseAsync = async () => window.location.reload();
                        } else {
                            await dialogController.showErrorAsync("錯誤", "上傳並更新字幕時發生錯誤！");
                        }
                    }
                },
                {
                    text: "取消",
                    type: "default",
                    callback: () => dialogController.closeAsync(dialog)
                }
            ],
            onShowAsync: async () => {
                await form.buildAsync();
                await form.setValueAsync(this._topicSubtitleData?.frameRate);
            },
            onCloseAsync: () => form.deleteAsync()
        }))!
    }

    private async _showReuploadTranscriptDialogAsync(): Promise<void> {
        if (this._advanceCommandDropdown) {
            this._advanceCommandDropdown.openAsync(false);
        }

        const form = new UploadTranscriptForm();

        const dialog: Dialog = (await dialogController.showAsync("reupload-transcript", {
            type: "info",
            title: "重新上傳逐字稿",
            width: 500,
            body: form.element,
            headerCloseButton: true,
            buttons: [
                {
                    text: "提交新逐字稿",
                    type: "warning",
                    callback: async () => {
                        if (!await form.validateAsync()) {
                            return;
                        }

                        if (!await dialogController.confirmWarningAsync("確認", "您確定要將目前單集的逐字稿取代成新上傳的這份嗎？現在的所有資料將被清除，且此作業無法被還原！")) {
                            return;
                        }

                        const data = await form.getValueAsync();
                        const loading = await dialogController.showLoadingAsync("處理中", "正在處理上傳的逐字稿資訊..");
                        const response = await apiController.queryAsync(topicApiMap.reuploadTranscript, { id: this._id, ticket: data });
                        await dialogController.closeAsync(loading);

                        if (response.success) {
                            const successDialog = await dialogController.showSuccessAsync("成功", "成功上傳並更新逐字稿", 5000);
                            successDialog!.onCloseAsync = async () => window.location.reload();
                        } else {
                            await dialogController.showErrorAsync("錯誤", "上傳並更新逐字稿時發生錯誤！");
                        }
                    }
                },
                {
                    text: "取消",
                    type: "default",
                    callback: () => dialogController.closeAsync(dialog)
                }
            ],
            onShowAsync: () => form.buildAsync(),
            onCloseAsync: () => form.deleteAsync()
        }))!
    }

    private _selectEncodingAsync(): Promise<string | undefined> {
        let encoding: string | undefined = undefined;
        return new Promise<string | undefined>(async resolve => {
            const form = new EncodingSelectionForm();
            const encodingSelectionDialog: Dialog = (await dialogController.showAsync("encoding-selection", {
                type: "info",
                title: "選擇下載編碼",
                width: 500,
                body: form.element,
                headerCloseButton: true,
                buttons: [
                    {
                        text: "下載",
                        type: "primary",
                        callback: async () => {
                            encoding = (await form.getValueAsync()).encoding;
                            await dialogController.closeAsync(encodingSelectionDialog);
                        }
                    },
                    {
                        text: "取消",
                        type: "default",
                        callback: () => dialogController.closeAsync(encodingSelectionDialog)
                    }
                ],
                onShowAsync: () => form.buildAsync(),
                onCloseAsync: async () => {
                    await form.deleteAsync();
                    resolve(encoding);
                }
            }))!;
        });
    }
}