import { Line, Subtitle } from "../components";
import EditorActions from "../contexts";
import { ActionData } from "../models";
import HistoryBase from "./base/HistoryBase";
import History from "./base/History";
import { RecreateTimeCommand } from "../commands";

export default class RecreateTimeHistory extends HistoryBase<ActionData.RecreateTime> implements History {

    constructor(subtitle: Subtitle, data: ActionData.RecreateTime) {
        super(subtitle, { action: EditorActions.recreateTime, data });
    }

    getLine(): Line | undefined {
        return undefined;
    }

    async undoActionAsync(): Promise<void> {
        if (!this._editor.currentTranscript) { return; }

        this._editor.currentTranscript.clearSpan();
        this._editor.currentTranscript.insertSpan(0, this.data.transcript);

        this._subtitle.setShowAnimation(false);
        for (let i = 0; i < this.data.lines.length; i++) {
            const data = this.data.lines[i];
            const line = await this._subtitle.insertAsync(data.index - 1);
            await line.initDataAsync(data.lineState);
            await line.applyAsync(data.lineData);
        }

        this._subtitle.setShowAnimation(true);

        this._editor.updateTranscriptState();
        this._editor.currentTranscript.updateBreakLineMode();
        this._editor.updateTranscriptModeGroup();
    }

    async redoActionAsync(): Promise<void> {
        await this._editor.cancelLineAsync();
        await RecreateTimeCommand.doRecreateTimeAsync(this._subtitle.editor);
    }
}