import { apiController } from "ApiController";
import { dataSourceApiMap } from "contexts/dataSourceApiMap";
import { fileApiMap } from "contexts/fileApiMap";
import { topicApiMap } from "contexts/topicApiMap";
import { userMetaApiMap } from "contexts/userMetaApiMap";
import TopicCreationRow from "elements/formElements/TopicCreationRow";
import TopicBatchCreationRowData from "elements/models/topic/TopicBatchCreationRowData";
import TopicCreateRequest from "elements/models/topic/TopicCreateRequest";
import UserOptionsModel from "elements/models/userMeta/UserOptionsModel";
import dialogController from "uform-dialog";
import { ButtonElement, ElementExpander, FileElement, Form, FormElementContainer, NoteElement } from "uform-form";
import { SelectOption } from "uform-selector";

export default class TopicBatchCreationForm extends Form {

    private _fileInput: FileElement = undefined!;
    private _fileNote: NoteElement = undefined!;
    private _table: ElementExpander<TopicCreationRow, TopicBatchCreationRowData> = undefined!;
    private _submitButton: ButtonElement = undefined!;

    private _frameRate?: number;
    private _wordLimit?: number;
    private _modelNameOptions: SelectOption[] = [];

    onSubmit?: () => Promise<void>;
    onCancel?: () => Promise<void>;

    async buildChildrenAsync(): Promise<void> {

        const userOptionsResponse = await apiController.queryAsync(userMetaApiMap.getSelfOptions);
        if (userOptionsResponse.success) {
            const userOptions: UserOptionsModel = userOptionsResponse.data;
            if (userOptions.frameRate) {
                this._frameRate = userOptions.frameRate;
            }

            if (userOptions.wordLimit) {
                this._wordLimit = userOptions.wordLimit;
            }
        }

        const modelNameOptionsResponse = await apiController.queryAsync(dataSourceApiMap.listAsrModelOption);
        if (modelNameOptionsResponse.success) {
            this._modelNameOptions = modelNameOptionsResponse.data;
        }

        const fileInputGroup = FormElementContainer.fromAsync({
            buildChildrenFunc: async group => {
                this._fileInput = await FileElement.fromAsync({
                    required: true,
                    uploadApi: fileApiMap.upload,
                    showFilename: false,
                    downloadable: false,
                    multiple: true,
                    autoUpload: false,
                    ignoreEmptyTicketError: true,
                    acceptedExtensions: ["mp4", "mpeg", "mpg", "mov", "avi", "mxf", "mp3"]
                });
                await group.appendAsync(this._fileInput);
            }
        });
        await this.appendAsync(fileInputGroup);

        this._fileNote = await NoteElement.fromAsync({
            type: "info",
            text: "請先選擇要上傳的影片，完成選擇後您可以自由設定每部影片的建立選項，或是繼續增加更多影片。"
        });
        await this.appendAsync(this._fileNote);

        this._table = await ElementExpander.fromAsync<TopicCreationRow, TopicBatchCreationRowData>({
            formElementCreationFunc: () => new TopicCreationRow(this._frameRate, this._wordLimit, this._modelNameOptions),
            required: true,
            showExpandButtons: true,
            buttonAhead: true,
            hide: true,
            tableOptions: {
                columns: [
                    { head: "檔案", width: 188 }, 
                    { head: "名稱", width: 150 }, 
                    { head: "說明", width: 150 }, 
                    { head: "建立方式", width: 315 }, 
                    { head: "", flex: 1 }
                ]
            }
        });
        await this.appendAsync(this._table);

        const buttonGroup = await FormElementContainer.fromAsync({
            buildChildrenFunc: async group => {
                this._submitButton = await ButtonElement.fromAsync({
                    text: "提交所有檔案",
                    type: "primary",
                    disabled: true,
                    onClick: () => this._onSubmitAsync()
                });
                await group.appendAsync(this._submitButton);

                const cancelButton = ButtonElement.fromAsync({
                    text: "取消",
                    type: "default",
                    onClick: () => this._onCancelAsync()
                });
                await group.appendAsync(cancelButton);
            }
        });
        await this.appendAsync(buttonGroup);

        this._fileInput.addChangeFunc(() => this._updateTableByFilesAsync());
    }

    async validateAsync(): Promise<boolean> {
        return await this._fileInput.validateAsync() && await this._table.validateAsync();
    }

    async getValueAsync(): Promise<TopicCreateRequest[]> {
        const tableValues = await this._table.getValueAsync();
        return <any>tableValues.map(value => {
            return { ...value, file: undefined };
        });
    }

    private async _updateTableByFilesAsync(): Promise<void> {
        const rawFiles = this._fileInput.rawFiles;
        if (rawFiles.length > 0) {
            await this._submitButton.setDisableAsync(false);
            await this._fileInput.hideAsync();
            await this._fileNote.hideAsync();
            await this._table.showAsync();
            await this._table.setValueAsync(rawFiles
                .map(rawFile => {
                    return {
                        file: rawFile,
                        name: rawFile.name,
                        createType: 0,
                        frameRate: this._frameRate,
                        wordLimit: this._wordLimit
                    };
                }));
        }
    }

    private async _onSubmitAsync(): Promise<void> {
        if (!await this._table.validateAsync()) {
            await dialogController.showWarningAsync("錯誤", "請修正被標示為紅色的欄位。");
            return;
        }

        const uploadingBody = document.createElement("div");
        uploadingBody.style.whiteSpace = "break-spaces";
        uploadingBody.style.overflowWrap = "break-word";
        uploadingBody.innerHTML = `<i class=\"fa fa-gear fa-spin\"></i>&nbsp;正在上傳檔案，請不要關閉瀏覽器及本視窗..`;

        const uploading = (await dialogController.showAsync("creating-topics", {
            type: "info",
            title: "上傳並建立單集中",
            body: uploadingBody,
            headerCloseButton: false,
            buttons: []
        }))!;

        const totalProgress = this._table.rows.length;
        let currentProgresses: number[] = [];

        const updateProgress = (index: number, totale: number) => {
            let currentProgress = 0;
            for (let i = 0; i < currentProgresses.length; i++) {
                currentProgress += currentProgresses[i];
            }

            const percentage = Math.round(100 * currentProgress / totalProgress);
            uploadingBody.innerHTML = `<i class=\"fa fa-gear fa-spin\"></i>&nbsp;正在上傳 ${totale} 筆之中的第 ${index} 筆檔案(${String(percentage)}%)，請不要關閉瀏覽器及本視窗！`;
        };

        const updateCreation = (index: number, totale: number) => {
            uploadingBody.innerHTML = `<i class=\"fa fa-gear fa-spin\"></i>&nbsp;正在建立 ${totale} 筆之中的第 ${index} 筆單集，請不要關閉瀏覽器及本視窗！`;
        };

        let succeed = true;
        for (let index = 0; index < this._table.rows.length; index++) {
            const row = this._table.rows[index];
            const uploadSucceed = await row.formElement.uploadAsync(p => {
                currentProgresses[index] = p;
                updateProgress(index + 1, this._table.rows.length);
            });

            if (!uploadSucceed) {
                succeed = false;
                await uploading.closeAsync();
                break;
            }

            updateCreation(index + 1, this._table.rows.length);

            const tableValue = await this._table.rows[index].formElement.getValueAsync();
            const response = await apiController.queryAsync(topicApiMap.create, tableValue);
            if (!response.success) {
                succeed = false;
                await uploading.closeAsync();
                await dialogController.showErrorAsync("錯誤", `建立新單集時發生錯誤！批次工作已終止。錯誤訊息：${response.message}`);
                break;
            }
        }

        if (succeed && this.onSubmit) {
            await uploading.closeAsync();
            await this.onSubmit();
        }
    }

    private async _onCancelAsync(): Promise<void> {
        const tableValues = await this._table.getValueAsync();
        if (tableValues.filter(o => !!o.name || !!o.file || !!o.filename).length > 0 && !await dialogController.confirmWarningAsync("確認", "您確定要離開嗎？已輸入的資料將不會保留！")) {
            return;
        }

        if (this.onCancel) {
            await this.onCancel();
        }
    }
}