import { apiController } from "ApiController";
import { dialogController } from "DialogController";
import { systemStatusApiMap } from "contexts/systemStatusApiMap";
import { menuController } from "controllers/MenuController";
import StatusForm from "elements/forms/StatusForm";
import { Page } from "uform-page";

export default class StatusPage extends Page {
    paths: string[] = ["/Status", "/Status/Index"];

    private _form?: StatusForm;

    protected async loadAsync(): Promise<void> {
        menuController.activeMenu(this.paths);
        menuController.activeMenu("/Management");

        const formContainer = <HTMLDivElement>document.getElementById("form-container");
        if (formContainer) {
            this._form = new StatusForm();
            formContainer.appendChild(this._form.element);

            await this._form.buildAsync();

            const loading = await dialogController.showLoadingAsync("載入中", "正在載入服務狀態..");
            const response = await apiController.queryAsync(systemStatusApiMap.getStatus);
            await loading.closeAsync();
            if (response.success) {
                await this._form.setValueAsync(response.data);
            } else {
                await dialogController.showErrorAsync("錯誤", `取得服務狀態發生錯誤：${response.message}`);
            }
        }
    }
}