
export default interface VideoState {
    mode: "relative" | "url";
    state: "empty" | "checking" | "checked" | "error";
    name: string;
    url: string;
    type: string;
    error?: string;
    onCompleted?: () => void;
}