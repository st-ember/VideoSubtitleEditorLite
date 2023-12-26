import { fileApiMap } from "contexts/fileApiMap";
import { FileElement, Form, NoteElement, NumberElement } from "uform-form";

export default class UploadSubtitleForm extends Form {

    private _uploader: FileElement = undefined!;
    private _frameRate: NumberElement = undefined!;
    private _frameRateNote: NoteElement = undefined!;

    async buildChildrenAsync(): Promise<void> {
        
        this._uploader = await FileElement.fromAsync({
            uploadApi: fileApiMap.upload,
            downloadable: false,
            required: true,
            acceptedExtensions: ["srt", "vtt", "txt"]
        });
        this.appendAsync(this._uploader);

        this._frameRate = await NumberElement.fromAsync({
            min: 1,
            max: 1024,
            required: false,
            placeholder: "每秒畫格數"
        });
        await this.appendAsync(this._frameRate);

        this._frameRateNote = await NoteElement.fromAsync({
            type: "info",
            text: "如果在您上傳的字幕中，小於一秒的時間是以影格為單位呈現，請提供此欄位以便系統準確換算。未來從此系統下載本字幕時，也會以此欄位的數值來呈現影格編號。"
        });
        await this.appendAsync(this._frameRateNote);
    }

    async validateAsync(): Promise<boolean> {
        return await this._uploader.validateAsync() && await this._frameRate.validateAsync();
    }

    async getValueAsync(): Promise<{ ticket: string, frameRate?: number } | undefined> {
        const fileData = await this._uploader.getSingleValueAsync();
        const frameRate = await this._frameRate.getValueAsync();
        return fileData ? { ticket: fileData.ticket, frameRate } : undefined;
    }

    async setValueAsync(value?: number): Promise<void> {
        if (value !== undefined) {
            await this._frameRate.setValueAsync(value);
        }
    }
}