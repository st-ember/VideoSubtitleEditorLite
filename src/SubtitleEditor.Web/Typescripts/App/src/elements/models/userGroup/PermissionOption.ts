import { OptionItem } from "uform-form";

export default interface PermissionOption extends OptionItem {
    isGroup: boolean;
    isAllOption: boolean;
    children: PermissionOption[];
}