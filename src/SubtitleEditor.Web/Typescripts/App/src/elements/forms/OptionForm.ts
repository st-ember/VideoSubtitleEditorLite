import { apiController } from "ApiController";
import { dialogController } from "DialogController";
import { fileApiMap } from "contexts/fileApiMap";
import { optionApiMap } from "contexts/optionApiMap";
import { systemActions } from "contexts/systemActions";
import { permissionController } from "controllers/PermissionController";
import ListOptionModel from "elements/models/option/ListOptionModel";
import SystemOptionModel from "elements/models/option/SystemOptionModel";
import { ButtonElement, FileElement, Form, FormElementContainer, InputElement, TextElement } from "uform-form";

export default class OptionForm extends Form {

    options: SystemOptionModel[] = [];
    optionElements: { name: string, input: InputElement | FileElement }[] = [];

    saveButton: ButtonElement = null!;

    async buildChildrenAsync(): Promise<void> {
        const response = await apiController.queryAsync(optionApiMap.list);
        if (!response.success) {
            await dialogController.showErrorAsync("錯誤", "列出系統設定時發生錯誤。");
        }

        const model: ListOptionModel = response.success ? response.data : { items: [] };
        this.options = model.items;

        for (let i = 0; i < this.options.length; i++) {
            const option = this.options[i];
            const group = FormElementContainer.fromAsync({
                multipleLine: true,
                buildChildrenFunc: async group => {
                    const text = TextElement.fromAsync({ text: option.description ?? option.name });
                    await group.appendAsync(text);

                    if (!option.type) {
                        const input = await InputElement.fromAsync({
                            autocomplete: false,
                            type: option.encrypted ? "password" : "text"
                        });
                        await group.appendAsync(input);

                        this.optionElements.push({ name: option.name, input });
                    } else if (option.type === "file-ticket") {
                        const input = await FileElement.fromAsync({
                            deletable: false,
                            downloadable: false,
                            uploadApi: fileApiMap.uploadToStorage,
                            acceptedExtensions: ["png", "svg", "jpg", "jpeg", "gif"]
                        });
                        await group.appendAsync(input);

                        this.optionElements.push({ name: option.name, input });
                    }
                }
            });
            await this.appendAsync(group);
        }

        this.saveButton = await ButtonElement.fromAsync({ 
            text: "儲存", 
            type: "primary",
            hide: !permissionController.contains(systemActions.UpdateOptions)
        });
        await this.appendAsync(this.saveButton);
    }

    async setValueAsync(): Promise<void> {
        for (let i = 0; i < this.optionElements.length; i++) {
            const { name, input } = this.optionElements[i];
            const matchs = this.options.filter(o => o.name === name);
            if (matchs.length > 0) {
                if (input instanceof InputElement) {
                    await input.setValueAsync(matchs[0].content ?? "");
                } else if (input instanceof FileElement) {
                    await input.setValueAsync(JSON.parse(matchs[0].content ?? "[]"));
                }
            }
        }
    }

    async getValueAsync(): Promise<SystemOptionModel[]> {
        const options: SystemOptionModel[] = [];
        for (let i = 0; i < this.options.length; i++) {
            const option = this.options[i];
            const { name, input } = this.optionElements.filter(o => o.name === option.name)[0];

            const content = input instanceof InputElement ?
                await input.getValueAsync() :
                input instanceof FileElement ?
                    JSON.stringify(await input.getValueAsync()) : "";

            options.push({ name, content, encrypted: option.encrypted });
        }

        return options;
    }
}