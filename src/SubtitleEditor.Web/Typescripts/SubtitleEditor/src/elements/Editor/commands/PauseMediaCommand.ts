import Editor from "../Editor";

/** 暫停播放媒體。 */
export default class PauseMediaCommand {

    static async checkAsync(editor: Editor): Promise<boolean> {
        return editor.player.playing;
    }

    static async invokeAsync(editor: Editor): Promise<void> {
        if (editor.player.playing) {
            editor.pause();
        }
    }
}