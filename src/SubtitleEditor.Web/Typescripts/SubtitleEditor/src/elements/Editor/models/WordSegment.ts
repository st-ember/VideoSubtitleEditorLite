/** 字幕的區塊資料 */
export default interface WordSegment {
    /** 區塊的文字 */
    word: string;
    /** 區塊的時間，格式為 hh:mm:ss.fff */
    start: string;
}