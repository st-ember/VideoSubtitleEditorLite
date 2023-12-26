import { fileApiMap } from "contexts/fileApiMap";
import { FileElement, Form } from "uform-form";

export default class UploadTranscriptForm extends Form {

    private _uploader: FileElement = undefined!;

    async buildChildrenAsync(): Promise<void> {
        
        this._uploader = await FileElement.fromAsync({
            uploadApi: fileApiMap.upload,
            downloadable: false,
            required: true,
            acceptedExtensions: ["txt"]
        });
        this.appendAsync(this._uploader);
    }

    validateAsync(): Promise<boolean> {
        return this._uploader.validateAsync();
    }

    async getValueAsync(): Promise<string | undefined> {
        const result = await this._uploader.getSingleValueAsync();
        return result ? result.ticket : undefined;
    }
}