import { apiController } from "ApiController";
import { dialogController } from "DialogController";
import { fixBookApiMap } from "contexts/fixBookApiMap";
import { menuController } from "controllers/MenuController";
import FixBookForm from "elements/forms/FixBookForm";
import FixBookModel from "elements/models/fixBook/FixBookModel";
import { Page } from "uform-page";

export default class FixBookPage extends Page {

    paths: string[] = ["/FixBook", "/FixBooks", "/FixBook/List"];

    private _form: FixBookForm = undefined!;
    private _modelName: string = "";

    protected async loadAsync(): Promise<void> {
        menuController.activeMenu(this.paths);

        this._modelName = (<HTMLInputElement>document.getElementById("model-name-input"))!.value;

        const tableContainer = <HTMLDivElement>document.querySelector(".table-container");
        if (tableContainer) {
            this._form = new FixBookForm();
            tableContainer.parentElement!.appendChild(this._form.element);
            await this._form.buildAsync();
            await this._loadFormAsync();
        }

        const refreshButton = <HTMLButtonElement>document.getElementById("refresh-button");
        if (refreshButton) {
            refreshButton.addEventListener("click", () => this._loadFormAsync());
        }

        const saveButton = <HTMLButtonElement>document.getElementById("save-button");
        if (saveButton) {
            saveButton.addEventListener("click", () => this._saveAsync());
        }
    }

    private async _loadFormAsync(): Promise<void> {
        const loading = await dialogController.showLoadingAsync("載入中", "正在載入勘誤表..");
        const response = await apiController.queryAsync(fixBookApiMap.get, { modelName: this._modelName });
        loading.closeAsync();
        if (response.success) {
            const data = <FixBookModel>response.data;
            this._form.setValueAsync(data);
        } else {
            dialogController.showErrorAsync("錯誤", `取得勘誤表資料時發生錯誤：${response.message}`);
        }
    }

    private async _saveAsync(): Promise<void> {
        const loading = await dialogController.showLoadingAsync("儲存中", "正在儲存勘誤表..");
        const model = await this._form.getValueAsync();
        const response = await apiController.queryAsync(fixBookApiMap.save, { modelName: this._modelName, fixBookItems: model.items })
        await loading.closeAsync();

        if (response.success) {
            dialogController.showSuccessAsync("成功", "成功儲存勘誤表。");
        } else {
            dialogController.showErrorAsync("錯誤", `儲存勘誤表時發生錯誤：${response.message}`);
        }
    }
}