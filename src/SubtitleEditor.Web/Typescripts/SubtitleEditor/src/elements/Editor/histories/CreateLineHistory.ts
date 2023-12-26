import { Line, Subtitle } from "../components";
import EditorActions from "../contexts";
import { ActionData } from "../models";
import HistoryBase from "./base/HistoryBase";
import History from "./base/History";

export default class CreateLineHistory extends HistoryBase<ActionData.CreateLine> implements History {

    constructor(subtitle: Subtitle, data: ActionData.CreateLine) {
        super(subtitle, { action: EditorActions.createLine, data });
    }

    getLine(): Line | undefined {
        return !this.undoExecuted ? this._subtitle.lines[this._subtitle.lines.length - 1] : undefined;
    }

    async undoActionAsync(): Promise<void> {
        await this._subtitle.deleteAsync(this.getLine());
        this._subtitle.transcript.insertSpan(this.data.startSpanIndex, this.data.content);
    }

    async redoActionAsync(): Promise<void> {
        const line = this._subtitle.transcript.bindSpan(this._subtitle.transcript.listSpan(this.data.startSpanIndex, this.data.endSpanIndex - this.data.startSpanIndex + 1));
        await line.setStartAsync(this.data.start);
        await line.setEndAsync(this.data.end);
    }
}

