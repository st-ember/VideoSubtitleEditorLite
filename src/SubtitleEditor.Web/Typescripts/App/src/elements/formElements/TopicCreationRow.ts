import { fileApiMap } from "contexts/fileApiMap";
import TopicBatchCreationRowData from "elements/models/topic/TopicBatchCreationRowData";
import TopicCreateRequest from "elements/models/topic/TopicCreateRequest";
import { ButtonElement, FileElement, FormElementContainer, FormElementContainerGeneric, InputElement, NoteElement, NumberElement, RadioButtonElement, TextAreaElement, TextElement } from "uform-form";
import { SelectElement } from "uform-form-selector";
import { SelectOption } from "uform-selector";

export default class TopicCreationRow extends FormElementContainerGeneric<TopicBatchCreationRowData> {

    private _fileInput: FileElement = undefined!;
    private _nameInput: InputElement = undefined!;
    private _descInput: InputElement = undefined!;
    private _createType: RadioButtonElement = undefined!;
    private _subtitleFile: FileElement = undefined!;
    private _transcriptFile: FileElement = undefined!;
    private _frameRate: NumberElement = undefined!;
    private _wordLimit: NumberElement = undefined!;
    private _modelName: SelectElement = undefined!;

    private _userOptionFrameRate?: number;
    private _userOptionWordLimit?: number;
    private _modelNameOptions: SelectOption[] = [];
    private _createTypes: SelectOption[] = [];

    get uploaded(): boolean { return this._fileInput.files.length > 0 && !!this._fileInput.files[0].ticket; }

    constructor(frameRate?: number, wordLimit?: number, modelNameOptions?: SelectOption[]) {
        super();

        this._userOptionFrameRate = frameRate;
        this._userOptionWordLimit = wordLimit;

        if (modelNameOptions) {
            this._modelNameOptions = modelNameOptions;
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
    }

    async buildChildrenAsync(): Promise<void> {

        this._fileInput = await FileElement.fromAsync({
            required: true,
            hideReqiredIcon: true,
            uploadApi: fileApiMap.upload,
            showFilename: false,
            downloadable: false,
            autoUpload: false,
            ignoreEmptyTicketError: true,
            uploadedText: "(已選擇)",
            acceptedExtensions: ["mp4", "mpeg", "mpg", "mov", "avi", "mxf", "mp3"]
        });
        await this.appendAsync(this._fileInput);

        this._nameInput = await InputElement.fromAsync({
            maxLength: 256,
            required: true,
            hideReqiredIcon: true
        });
        await this.appendAsync(this._nameInput);

        this._descInput = await InputElement.fromAsync({});
        await this.appendAsync(this._descInput);

        this._createType = await RadioButtonElement.fromAsync({
            options: this._createTypes
        });
        await this.appendAsync(this._createType);

        const optionGroup = await FormElementContainer.fromAsync({
            required: true,
            hideReqiredIcon: true,
            inline: true,
            buildChildrenFunc: async group => {
                this._subtitleFile = await FileElement.fromAsync({
                    uploadApi: fileApiMap.upload,
                    emptyMessage: "選擇字幕檔",
                    downloadable: false,
                    required: true,
                    hideReqiredIcon: true,
                    acceptedExtensions: ["srt", "vtt", "txt"],
                    hide: this._modelNameOptions.length > 0
                });
                await group.appendAsync(this._subtitleFile);

                this._transcriptFile = await FileElement.fromAsync({
                    uploadApi: fileApiMap.upload,
                    emptyMessage: "選擇逐字稿",
                    downloadable: false,
                    required: true,
                    hideReqiredIcon: true,
                    acceptedExtensions: ["txt"],
                    hide: true
                });
                await group.appendAsync(this._transcriptFile);

                this._frameRate = await NumberElement.fromAsync({
                    placeholder: "每秒畫格數",
                    title: "每秒畫格數",
                    min: 1,
                    max: 1024,
                    step: 0.01,
                    width: 110,
                    value: this._userOptionFrameRate,
                    hide: this._modelNameOptions.length > 0,
                    required: false
                });
                await group.appendAsync(this._frameRate);

                this._wordLimit = await NumberElement.fromAsync({
                    placeholder: "字數限制",
                    title: "字數限制",
                    min: 1,
                    max: 1024,
                    step: 1,
                    width: 80,
                    value: this._userOptionWordLimit,
                    required: false
                });
                await group.appendAsync(this._wordLimit);

                this._modelName = await SelectElement.fromAsync({
                    label: "Model",
                    width: 300,
                    options: this._modelNameOptions,
                    hide: this._modelNameOptions.length === 0
                });
                await group.appendAsync(this._modelName);
            }
        });
        await this.appendAsync(optionGroup);

        this._createType.addChangeFunc(async () => {
            const createType = await this._createType.getValueAsync();
            if (createType === "0") {
                await this._subtitleFile.hideAsync();
                await this._transcriptFile.hideAsync();
                await this._frameRate.hideAsync();
                await this._modelName.showAsync();
            } else if (createType === "1") {
                await this._subtitleFile.showAsync();
                await this._transcriptFile.hideAsync();
                await this._frameRate.showAsync();
                await this._modelName.hideAsync();
            } else if (createType === "2") {
                await this._subtitleFile.hideAsync();
                await this._transcriptFile.showAsync();
                await this._frameRate.hideAsync();
                await this._modelName.hideAsync();
            }
        });

        this._fileInput.addChangeFunc(async () => {
            if (this._fileInput.rawFiles.length > 0) {
                await this._nameInput.setValueAsync(this._fileInput.rawFiles[0].name);
            }
        });
    }

    async validateAsync(): Promise<boolean> {
        if (!await this._fileInput.validateAsync()) {
            return false;
        }

        if (!await this._nameInput.validateAsync()) {
            return false;
        }

        const createType = await this._createType.getValueAsync();
        if (createType === "1") {
            return await this._subtitleFile.validateAsync();
        } else if (createType === "2") {
            return await this._transcriptFile.validateAsync();
        }

        return true;
    }

    async getValueAsync(): Promise<TopicBatchCreationRowData> {
        const createType = await this._createType.getValueAsync();
        const file = await this._fileInput.getSingleValueAsync();
        return {
            filename: file?.filename,
            ticket: file?.ticket,
            name: await this._nameInput.getValueAsync(),
            description: await this._descInput.getValueAsync(),
            createType: createType ? Number(createType) : 0,
            subtitleTicket: this._subtitleFile ? (await this._subtitleFile.getSingleValueAsync())?.ticket : undefined,
            transcriptTicket: this._transcriptFile ? (await this._transcriptFile.getSingleValueAsync())?.ticket : undefined,
            frameRate: this._frameRate ? (await this._frameRate.getValueAsync()) : undefined,
            modelName: createType === "0" ? await this._modelName.getSingleValueAsync() : undefined
        }
    }

    async setValueAsync(value: TopicBatchCreationRowData): Promise<void> {
        if (value.file) {
            this._fileInput.rawFiles = [value.file];
            await this._fileInput.setValueAsync([{ filename: value.file.name, ticket: "", size: value.file.size }]);
        }

        await this._frameRate.setValueAsync(value.frameRate);
    }

    async uploadAsync(onProgress: (progress: number) => void): Promise<boolean> {
        if (!this.uploaded && this._fileInput.rawFiles.length > 0) {
            const rawFile = this._fileInput.rawFiles[0];
            this._fileInput.onProgress = p => onProgress(p);
            return await this._fileInput.uploadAsync([rawFile]);
        }

        return false;
    }
}