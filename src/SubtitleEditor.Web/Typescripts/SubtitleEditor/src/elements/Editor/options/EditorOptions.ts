import SubtitleOptions from "./SubtitleOptions";

export default interface EditorOptions {
    subtitles: SubtitleOptions[];
    /** 編輯器要渲染的父階層物件。 */
    container?: HTMLElement;
    topic?: {
        /** 影格速率，指該單集每秒的畫格數，如設定此值會使時間戳記小於一秒的數字改以影格數來呈現，系統會自動進行換算。 */
        frameRate?: number;
        /** 字數限制，指全形的文字長度限制，如設定此限制將會讓畫面出現紅色線條強調限制的長度，但不會實際限制字數。 */
        wordLimit?: number;
    },
    hooks?: {
        beforeBuild?: () => Promise<void>;
        afterBuild?: () => Promise<void>;
        beforeLoad?: () => Promise<void>;
        afterLoad?: () => Promise<void>;
        onError?: (error: string) => Promise<void>;
        /** 編輯器內的某些行為會需要停止全域的快速鍵監視，當這樣的情形發生時此 Hooker 將被觸發。 */
        onHotkeyStateChange?: (hotkeyListening: boolean) => Promise<void>;
        /** 當字數限制從編輯器內被修改時，此 Hooker 將被觸發。 */
        onLimitationChange?: () => Promise<void>;
    }
}