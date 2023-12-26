
export default interface UserKeybinding {
    action: string;
    keyCodes: string[];
    withCtrl: boolean;
    withShift: boolean;
    withAlt: boolean;
}