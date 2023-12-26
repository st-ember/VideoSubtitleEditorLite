import { menuController } from "controllers/MenuController";
import TopicBatchCreationForm from "elements/forms/TopicBatchCreationForm";
import dialogController from "uform-dialog";
import { Page } from "uform-page";

export default class TopicBatchCreationPage extends Page {

    paths: string[] = ["/TopicCreation/Batch"];

    private _formBoard?: HTMLDivElement;
    private _form: TopicBatchCreationForm = undefined!;

    protected async loadAsync(): Promise<void> {
        menuController.activeMenu(this.paths);

        this._formBoard = <HTMLDivElement>document.getElementById("form-board");
        if (this._formBoard) {
            this._form = new TopicBatchCreationForm();
            this._form.onSubmit = () => this._onSubmitAsync();
            this._form.onCancel = () => this._onCancelAsync();
            this._formBoard.appendChild(this._form.element);
            await this._form.buildAsync();
        }
    }

    private async _onSubmitAsync(): Promise<void> {
        const data = await this._form.getValueAsync();
        const successDialog = await dialogController.showSuccessAsync("成功", `已成功建立 ${data.length} 筆新單集。`, 5000);
        successDialog!.onCloseAsync = async () => this._goBackToTopicListPage();
    }

    private async _onCancelAsync(): Promise<void> {
        this._goBackToTopicListPage();
    }

    private _goBackToTopicListPage(): void {
        window.location.href = "/Topic/List";
    }
}