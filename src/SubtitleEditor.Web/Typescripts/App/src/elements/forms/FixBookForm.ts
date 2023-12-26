import FixBookRow from "elements/formElements/FixBookRow";
import FixBookItem from "elements/models/fixBook/FixBookItem";
import FixBookModel from "elements/models/fixBook/FixBookModel";
import { ElementExpander, FormGeneric, TextElement } from "uform-form";

export default class FixBookForm extends FormGeneric<FixBookModel> {

    private _modelName: string = "";
    private _maxFixbookSize: number = 0;

    private _maxSize: TextElement = undefined!;
    private _fixBookItems: ElementExpander<FixBookRow, FixBookItem> = undefined!;

    async buildChildrenAsync(): Promise<void> {
        this._maxSize = await TextElement.fromAsync({});
        await this.appendAsync(this._maxSize);

        this._fixBookItems = await ElementExpander.fromAsync<FixBookRow, FixBookItem>({
            showExpandButtons: true,
            formElementCreationFunc: () => new FixBookRow(),
            tableOptions: {
                columns: [
                    { head: "修正前文字", flex: 1 }, { head: "修正後文字", flex: 1 }
                ]
            }
        });
        await this.appendAsync(this._fixBookItems);

        this._fixBookItems.addChangeFunc(async () => {
            const items = await this._fixBookItems.getValueAsync();
            await this._maxSize.setValueAsync(this._maxFixbookSize ? `數量 ${items.length}/${this._maxFixbookSize}` : `數量 ${items.length}`);
        });
    }

    async getValueAsync(): Promise<FixBookModel> {
        const items = await this._fixBookItems.getValueAsync();
        return {
            modelName: this._modelName,
            maxFixbookSize: this._maxFixbookSize,
            items: items.filter(item => !!item.original)
        }
    }

    async setValueAsync(value: FixBookModel): Promise<void> {
        this._modelName = value.modelName;
        this._maxFixbookSize = value.maxFixbookSize;

        if (value.maxFixbookSize) {
            await this._maxSize.setValueAsync(`數量 ${value.items.length}/${value.maxFixbookSize}`);
        }

        await this._fixBookItems.setValueAsync(value.items);
    }
}