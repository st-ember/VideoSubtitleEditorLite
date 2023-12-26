import Editor from "../Editor";
import RecreateTimeHistory from "../histories/RecreateTimeHistory";

export default class RecreateTimeCommand {

    static async invokeAsync(editor: Editor): Promise<void> {
        if (!editor.currentSubtitle || !editor.currentTranscript) { return; }

        await editor.cancelLineAsync();

        const subtitle = editor.currentSubtitle;
        const transcript = editor.currentTranscript;
        const transcriptContent = transcript.content;
        const backupDatas = subtitle.lines.map(line => {
            return {
                index: line.index,
                lineState: line.datas,
                lineData: line.data
            };
        });

        await RecreateTimeCommand.doRecreateTimeAsync(editor);
        await subtitle.addHistoryAsync(new RecreateTimeHistory(subtitle, { lines: backupDatas, transcript: transcriptContent }));
    }

    static async doRecreateTimeAsync(editor: Editor): Promise<void> {
        if (!editor.currentSubtitle || !editor.currentTranscript) { return; }

        const subtitle = editor.currentSubtitle;
        const transcript = editor.currentTranscript;
        const transcriptContent = transcript.content ? transcript.content : subtitle.lines.map(line => line.content).join("\n");

        subtitle.setShowAnimation(false);
        await Promise.all(subtitle.lines.map(line => subtitle.deleteAsync(line)));

        subtitle.setShowAnimation(true);

        transcript.insertSpan(0, transcriptContent);
        editor.updateTranscriptState();

        transcript.updateBreakLineMode();
        editor.updateTranscriptModeGroup();
    }
}