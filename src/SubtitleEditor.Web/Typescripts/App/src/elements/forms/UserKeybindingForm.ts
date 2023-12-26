import KeybindingRow from "elements/formElements/KeybindingRow";
import UserKeybinding from "elements/models/userMeta/UserKeybinding";
import { ElementExpander, FormGeneric, NoteElement } from "uform-form";

export default class UserKeybindingForm extends FormGeneric<UserKeybinding[]> {

    private _keybindingTable: ElementExpander<KeybindingRow, UserKeybinding> = undefined!;

    async buildChildrenAsync(): Promise<void> {

        const note = NoteElement.fromAsync({
            type: "info",
            text: "您可以在這個畫面重新設定字幕編輯器內的鍵盤快速鍵。透過點選「設定」按鈕後，直接在鍵盤上輸入想要的案件組合，完成輸入後點選「完成」按鈕，或點選「取消」按鈕來還原案件組合。完成設定後，需要重新整理畫面才能使設定生效。"
        });
        await this.appendAsync(note);
        
        this._keybindingTable = await ElementExpander.fromAsync<KeybindingRow, UserKeybinding>({
            showExpandButtons: false,
            formElementCreationFunc: () => new KeybindingRow(),
            tableOptions: {
                columns: [
                    { head: "指令", width: 150 }, { head: "快速鍵", flex: 1 }
                ]
            }
        });
        await this.appendAsync(this._keybindingTable);
    }

    async setValueAsync(value: UserKeybinding[]): Promise<void> {
        await this._keybindingTable.setValueAsync(value);
    }

    async getValueAsync(): Promise<UserKeybinding[]> {
        return await this._keybindingTable.getValueAsync();
    }
}