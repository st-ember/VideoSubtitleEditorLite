import { apiController } from "ApiController";
import { dataSourceApiMap } from "contexts/dataSourceApiMap";
import { fileApiMap } from "contexts/fileApiMap";
import { userMetaApiMap } from "contexts/userMetaApiMap";
import TopicCreateRequest from "elements/models/topic/TopicCreateRequest";
import UserOptionsModel from "elements/models/userMeta/UserOptionsModel";
import dialogController from "uform-dialog";
import { FileElement, Form, FormElementContainer, InputElement, NoteElement, NumberElement, RadioButtonElement, TextAreaElement } from "uform-form";
import { SelectElement } from "uform-form-selector";
import { SelectOption } from "uform-selector";

export default class TopicCreationForm extends Form {

    private _fileInput: FileElement = undefined!;
    private _nameInput: InputElement = undefined!;
    private _descInput: TextAreaElement = undefined!;
    private _createType: RadioButtonElement = undefined!;
    private _modelName: SelectElement = undefined!;
    private _subtitleFile: FileElement = undefined!;
    private _subtitleFileNote: NoteElement = undefined!;
    private _transcriptFile: FileElement = undefined!;
    private _transcriptFileNote: NoteElement = undefined!;
    private _frameRate: NumberElement = undefined!;
    private _frameRateNote: NoteElement = undefined!;
    private _wordLimit: NumberElement = undefined!;
    private _wordLimitNote: NoteElement = undefined!;

    private _modelNameOptions: SelectOption[] = [];
    private _createTypes: SelectOption[] = [];

    async buildChildrenAsync(): Promise<void> {
        const loading = await dialogController.showLoadingAsync("載入中", "正在載入可用的 Model 清單，請稍後...");
        const modelNameOptionsResponse = await apiController.queryAsync(dataSourceApiMap.listAsrModelOption);
        await dialogController.closeAsync(loading);
        
        if (modelNameOptionsResponse.success) {
            this._modelNameOptions = modelNameOptionsResponse.data;
        }

        if (this._modelNameOptions.length > 0) {
            this._createTypes = [
                { text: "辨識流程", value: "0" },
                { text: "上傳字幕", value: "1" },
                { text: "上傳逐字稿", value: "2" }
            ];
        } else {
            this._createTypes = [
                { text: "上傳字幕", value: "1" },
                { text: "上傳逐字稿", value: "2" }
            ];
        }

        this._fileInput = await FileElement.fromAsync({
            label: "影片檔",
            required: true,
            uploadApi: fileApiMap.upload,
            downloadable: false,
            acceptedExtensions: ["mp4", "mpeg", "mpg", "mov", "avi", "mxf", "mp3"]
        });
        await this.appendAsync(this._fileInput);

        this._nameInput = await InputElement.fromAsync({
            label: "名稱",
            maxLength: 256,
            required: true
        });
        await this.appendAsync(this._nameInput);

        this._descInput = await TextAreaElement.fromAsync({
            label: "說明",
            rows: 5
        });
        await this.appendAsync(this._descInput);

        this._createType = await RadioButtonElement.fromAsync({
            label: "建立方式",
            options: this._createTypes
        });
        await this.appendAsync(this._createType);

        this._modelName = await SelectElement.fromAsync({
            label: "Model",
            hide: this._modelNameOptions.length === 0,
            options: this._modelNameOptions
        });
        await this.appendAsync(this._modelName);

        const subtitleGroup = await FormElementContainer.fromAsync({
            label: "字幕檔",
            multipleLine: true,
            required: true,
            hide: this._modelNameOptions.length > 0,
            buildChildrenFunc: async group => {
                this._subtitleFile = await FileElement.fromAsync({
                    uploadApi: fileApiMap.upload,
                    downloadable: false,
                    required: true,
                    hideReqiredIcon: true,
                    acceptedExtensions: ["srt", "vtt", "txt"]
                });
                await group.appendAsync(this._subtitleFile);
        
                this._subtitleFileNote = await NoteElement.fromAsync({
                    type: "info",
                    text: "上傳字幕檔將跳過產生字幕的流程。"
                });
                await group.appendAsync(this._subtitleFileNote);
            }
        });
        await this.appendAsync(subtitleGroup);

        const transcriptGroup = await FormElementContainer.fromAsync({
            label: "逐字稿",
            multipleLine: true,
            required: true,
            hide: true,
            buildChildrenFunc: async group => {
                this._transcriptFile = await FileElement.fromAsync({
                    uploadApi: fileApiMap.upload,
                    downloadable: false,
                    required: true,
                    hideReqiredIcon: true,
                    acceptedExtensions: ["txt"]
                });
                await group.appendAsync(this._transcriptFile);
        
                this._transcriptFileNote = await NoteElement.fromAsync({
                    type: "info",
                    text: "提供逐字稿以在字幕建立功能中使用逐字稿。"
                });
                await group.appendAsync(this._transcriptFileNote);
            }
        });
        await this.appendAsync(transcriptGroup);

        this._fileInput.addChangeFunc(async () => {
            const file = await this._fileInput.getSingleValueAsync();
            const name = await this._nameInput.getValueAsync();
            if (!name) {
                await this._nameInput.setValueAsync(file?.filename ?? "");
            }
        });

        const frameRateGroup = await FormElementContainer.fromAsync({
            label: "每秒畫格數",
            multipleLine: true,
            hide: this._modelNameOptions.length > 0,
            buildChildrenFunc: async group => {
                this._frameRate = await NumberElement.fromAsync({
                    min: 1,
                    max: 1024,
                    step: 0.01,
                    required: false
                });
                await group.appendAsync(this._frameRate);

                this._frameRateNote = await NoteElement.fromAsync({
                    type: "info",
                    text: "如果在您上傳的字幕中，小於一秒的時間是以影格為單位呈現，請提供此欄位以便系統準確換算。未來從此系統下載本字幕時，也會以此欄位的數值來呈現影格編號。此欄位非必填，未來您也可以再回來修改此欄位的值。"
                });
                await group.appendAsync(this._frameRateNote);
            }
        });
        await this.appendAsync(frameRateGroup);

        const wordLimitGroup = await FormElementContainer.fromAsync({
            label: "字數限制",
            multipleLine: true,
            buildChildrenFunc: async group => {
                this._wordLimit = await NumberElement.fromAsync({
                    min: 1,
                    max: 1024,
                    step: 1,
                    required: false
                });
                await group.appendAsync(this._wordLimit);

                this._wordLimitNote = await NoteElement.fromAsync({
                    type: "info",
                    text: "設定字數限制時會在編輯字幕時見到標示限制的紅色線條，超過此線條即表示字數以超過限制，但系統不會硬性要求字數不得超過限制。此欄位非必填，未來您也可以再回來修改此欄位的值。"
                });
                await group.appendAsync(this._wordLimitNote);
            }
        });
        await this.appendAsync(wordLimitGroup);

        const userOptionsResponse = await apiController.queryAsync(userMetaApiMap.getSelfOptions);
        if (userOptionsResponse.success) {
            const userOptions: UserOptionsModel = userOptionsResponse.data;
            if (userOptions.frameRate) {
                await this._frameRate.setValueAsync(userOptions.frameRate);
            }

            if (userOptions.wordLimit) {
                await this._wordLimit.setValueAsync(userOptions.wordLimit);
            }
        }

        this._createType.addChangeFunc(async () => {
            const createType = await this._createType.getValueAsync();
            if (createType === "0") {
                await this._modelName.showAsync();
                await subtitleGroup.hideAsync();
                await transcriptGroup.hideAsync();
                await frameRateGroup.hideAsync();
            } else if (createType === "1") {
                await this._modelName.hideAsync();
                await subtitleGroup.showAsync();
                await transcriptGroup.hideAsync();
                await frameRateGroup.showAsync();
            } else if (createType === "2") {
                await this._modelName.hideAsync();
                await subtitleGroup.hideAsync();
                await transcriptGroup.showAsync();
                await frameRateGroup.hideAsync();
            }
        });
    }

    async validateAsync(): Promise<boolean> {
        const validedName = await this._nameInput.validateAsync();
        const validedFile = await this._fileInput.validateAsync();
        const createType = await this._createType.getValueAsync();
        
        if (createType === "1") {
            const validedSubtitle = await this._subtitleFile.validateAsync();
            return validedName && validedFile && validedSubtitle && await this._frameRate.validateAsync();
        } else if (createType === "2") {
            const validedTranscript = await this._transcriptFile.validateAsync();
            return validedName && validedFile && validedTranscript;
        }

        return validedName && validedFile;
    }

    async getValueAsync(): Promise<TopicCreateRequest> {
        const file = await this._fileInput.getSingleValueAsync();
        const desc = await this._descInput.getValueAsync();
        return {
            filename: file!.filename,
            ticket: file!.ticket,
            name: await this._nameInput.getValueAsync(),
            description: desc ? desc : undefined,
            createType: (await this._createType.getNumberValueAsync())!,
            subtitleTicket: (await this._subtitleFile.getSingleValueAsync())?.ticket,
            transcriptTicket: (await this._transcriptFile.getSingleValueAsync())?.ticket,
            frameRate: await this._frameRate.getValueAsync(),
            wordLimit: await this._wordLimit.getValueAsync(),
            modelName: await this._modelName.getSingleValueAsync()
        };
    }
}