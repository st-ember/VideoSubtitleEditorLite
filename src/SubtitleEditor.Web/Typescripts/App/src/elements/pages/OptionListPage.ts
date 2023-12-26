import { apiController } from "ApiController";
import { dialogController } from "DialogController";
import { optionApiMap } from "contexts/optionApiMap";
import { menuController } from "controllers/MenuController";
import OptionForm from "elements/forms/OptionForm";
import SystemOptionModel from "elements/models/option/SystemOptionModel";
import { Page } from "uform-page";

export default class OptionListPage extends Page {

    paths: string[] = ["/Option", "/Options", "/Option/List"];

    private form: OptionForm = undefined!;

    protected async loadAsync(): Promise<void> {
        menuController.activeMenu(this.paths);
        menuController.activeMenu("/Management");

        const formContainer = <HTMLDivElement>document.getElementById("form-container");
        if (formContainer) {
            this.form = new OptionForm(formContainer);
            await this.form.buildAsync();
            await this.form.setValueAsync();
            this.form.saveButton.onClick = async () => {
                const options = await this.form.getValueAsync();
                await this._saveAsync(options);
            }
        }
    }

    private async _saveAsync(items: SystemOptionModel[]): Promise<void> {
        const loading = await dialogController.showLoadingAsync("儲存中", "正在儲存系統設定..");
        const response = await apiController.queryAsync(optionApiMap.update, { items });
        await dialogController.closeAsync(loading);

        if (response.success) {
            await dialogController.showSuccessAsync("成功", "成功儲存系統設定！");
        } else {
            await dialogController.showErrorAsync("錯誤", `儲存系統設定時發生錯誤！${response.message}`);
        }
    }
}