import { keybindingActions } from "contexts/keybindingActions";
import UserKeybinding from "elements/models/userMeta/UserKeybinding";
import { FormElementContainerGeneric, TextElement } from "uform-form";
import { KeycodeElement } from "uform-form-keycode";

export default class KeybindingRow extends FormElementContainerGeneric<UserKeybinding> {

    private _action: string = "";

    private _actionText: TextElement = undefined!;
    private _keycodeInput: KeycodeElement = undefined!;

    async buildChildrenAsync(): Promise<void> {
        
        this._actionText = await TextElement.fromAsync({});
        await this.appendAsync(this._actionText);

        this._keycodeInput = await KeycodeElement.fromAsync({ multipleKey: true });
        await this.appendAsync(this._keycodeInput);
    }

    async setValueAsync(value: UserKeybinding): Promise<void> {
        this._action = value.action;
        await this._actionText.setValueAsync(keybindingActions[value.action]);
        await this._keycodeInput.setValueAsync(value);
    }

    async getValueAsync(): Promise<UserKeybinding> {
        return {
            action: this._action,
            ...await this._keycodeInput.getValueAsync()
        };
    }
}