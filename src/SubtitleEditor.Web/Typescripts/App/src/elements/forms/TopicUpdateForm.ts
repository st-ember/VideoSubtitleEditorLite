import { apiController } from "ApiController";
import { dataSourceApiMap } from "contexts/dataSourceApiMap";
import { streamMediaStatus } from "contexts/streamMediaStatus";
import { topicStatus } from "contexts/topicStatus";
import TopicPreviewData from "elements/models/topic/TopicPreviewData";
import TopicUpdateRequest from "elements/models/topic/TopicUpdateRequest";
import dialogController from "uform-dialog";
import { Form, InputElement, NoteElement, NumberElement, ReadonlyInputElement, ReadonlyPickerElement, ReadonlyTextAreaElement, TextAreaElement } from "uform-form";
import { SelectElement } from "uform-form-selector";
import { SelectOption } from "uform-selector";

export default class TopicUpdateForm extends Form {

    id: string = "";

    private _nameInput: InputElement = undefined!;
    private _descInput: TextAreaElement = undefined!;
    private _frameRate: NumberElement = undefined!;
    private _wordLimit: NumberElement = undefined!;
    private _modelName: SelectElement = undefined!;
    private _modelNameNote: NoteElement = undefined!;
    private _filename: ReadonlyInputElement = undefined!;
    private _originalSizeInput: ReadonlyInputElement = undefined!;
    private _sizeInput: ReadonlyInputElement = undefined!;
    private _lengthInput: ReadonlyInputElement = undefined!;
    private _processTimeInput: ReadonlyInputElement = undefined!;
    private _topicStatus: ReadonlyPickerElement = undefined!;
    private _mediaStatus: ReadonlyPickerElement = undefined!;
    private _errorText: ReadonlyTextAreaElement = undefined!;

    private _originalModelName?: string;
    private _modelNameOptions: SelectOption[] = [];

    async buildChildrenAsync(): Promise<void> {
        const loading = await dialogController.showLoadingAsync("載入中", "正在載入可用的 Model 清單，請稍後...");
        const modelNameOptionsResponse = await apiController.queryAsync(dataSourceApiMap.listAsrModelOption);
        await dialogController.closeAsync(loading);
        
        if (modelNameOptionsResponse.success) {
            this._modelNameOptions = modelNameOptionsResponse.data;
        }
        
        this._nameInput = await InputElement.fromAsync({
            label: "名稱",
            maxLength: 256,
            required: true,
            autocomplete: false
        });
        await this.appendAsync(this._nameInput);

        this._descInput = await TextAreaElement.fromAsync({
            label: "說明",
            rows: 4
        });
        await this.appendAsync(this._descInput);

        this._frameRate = await NumberElement.fromAsync({
            label: "每秒畫格數",
            min: 1,
            max: 1024,
            step: 0.01,
            required: false
        });
        await this.appendAsync(this._frameRate);

        const frameRateNote = NoteElement.fromAsync({
            type: "info",
            text: "設定每秒畫格數會讓系統在下載字幕時，將小於一秒的部分自動換算成影格編號。",
            label: ""
        });
        await this.appendAsync(frameRateNote);

        this._wordLimit = await NumberElement.fromAsync({
            label: "字數限制",
            min: 1,
            max: 1024,
            step: 1,
            required: false
        });
        await this.appendAsync(this._wordLimit);

        const wordLimitNote = NoteElement.fromAsync({
            type: "info",
            text: "設定字數限制時會在編輯字幕時見到標示限制的紅色線條，超過此線條即表示字數以超過限制，但系統不會硬性要求字數不得超過限制。",
            label: ""
        });
        await this.appendAsync(wordLimitNote);

        this._modelName = await SelectElement.fromAsync({
            label: "Model",
            disabled: this._modelNameOptions.length === 0,
            options: this._modelNameOptions.length > 0 ? [{ text: "未選擇", value: "-1" }].concat(<any>this._modelNameOptions) : [{ text: "無法使用", value: "-1" }]
        });
        await this.appendAsync(this._modelName);

        this._modelNameNote = await NoteElement.fromAsync({
            type: "info",
            text: "完成 Model 設定的修改後，需要進入字幕編輯器，並對單集進行「重新辨識」才會正式生效。如果未選擇 Model，則重新辨識時會採用辨識系統的預設值。",
            label: ""
        });
        await this.appendAsync(this._modelNameNote);

        this._filename = await ReadonlyInputElement.fromAsync({
            label: "檔案名稱"
        });
        await this.appendAsync(this._filename);

        this._originalSizeInput = await ReadonlyInputElement.fromAsync({
            label: "原始檔案大小"
        });
        await this.appendAsync(this._originalSizeInput);

        this._sizeInput = await ReadonlyInputElement.fromAsync({
            label: "檔案大小"
        });
        await this.appendAsync(this._sizeInput);

        this._lengthInput = await ReadonlyInputElement.fromAsync({
            label: "媒體長度"
        });
        await this.appendAsync(this._lengthInput);

        this._processTimeInput = await ReadonlyInputElement.fromAsync({
            label: "花費時間"
        });
        await this.appendAsync(this._processTimeInput);

        this._topicStatus = await ReadonlyPickerElement.fromAsync({
            label: "狀態",
            options: Object.keys(topicStatus).map(key => { return { value: String(key), text: <string>(topicStatus[Number(key)]) } })
        });
        await this.appendAsync(this._topicStatus);

        this._mediaStatus = await ReadonlyPickerElement.fromAsync({
            label: "媒體狀態",
            options: Object.keys(streamMediaStatus).map(key => { return { value: String(key), text: <string>(streamMediaStatus[Number(key)]) } })
        });
        await this.appendAsync(this._mediaStatus);

        this._errorText = await ReadonlyTextAreaElement.fromAsync({
            label: "錯誤訊息",
            hide: true
        });
        await this.appendAsync(this._errorText);
    }

    async validateAsync(): Promise<boolean> {
        return await this._nameInput.validateAsync();
    }

    async getValueAsync(): Promise<TopicUpdateRequest> {
        const modelName = await this._modelName.getSingleValueAsync();
        const adoptedModelName = modelName && modelName !== "-1" ? modelName : undefined;
        return {
            id: this.id,
            name: await this._nameInput.getValueAsync(),
            description: await this._descInput.getValueAsync(),
            frameRate: await this._frameRate.getValueAsync(),
            wordLimit: await this._wordLimit.getValueAsync(),
            modelName: this._modelNameOptions.length > 0 ? adoptedModelName : this._originalModelName
        };
    }

    async setValueAsync(value: TopicPreviewData): Promise<void> {
        const originalSize = value.originalSize > 0 ? Math.round(value.originalSize / 1048576 * 10) / 10 : 0;
        const size = value.size > 0 ? Math.round(value.size / 1048576 * 10) / 10 : 0;

        this._originalModelName = value.modelName;

        await Promise.all([
            this._nameInput.setValueAsync(value.name),
            this._descInput.setValueAsync(value.description),
            this._filename.setValueAsync(value.filename),
            this._originalSizeInput.setValueAsync(`${String(originalSize)} MB`),
            this._sizeInput.setValueAsync(`${String(size)} MB`),
            this._lengthInput.setValueAsync(value.lengthText),
            this._processTimeInput.setValueAsync(value.processTimeText),
            this._topicStatus.setValueAsync(String(value.status)),
            this._mediaStatus.setValueAsync(String(value.mediaStatus)),
            this._modelName.setValueAsync(value.modelName ?? ""),
            this._frameRate.setValueAsync(value.frameRate),
            this._wordLimit.setValueAsync(value.wordLimit)
        ]);

        if (value.error) {
            await this._errorText.setValueAsync(value.error);
            await this._errorText.showAsync();
        }
    }
}