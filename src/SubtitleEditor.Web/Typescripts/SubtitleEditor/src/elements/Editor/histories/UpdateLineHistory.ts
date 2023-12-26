import { Line, Subtitle } from "../components";
import EditorActions from "../contexts";
import { ActionData } from "../models";
import HistoryBase from "./base/HistoryBase";
import History from "./base/History";

export default class UpdateLineHistory extends HistoryBase<ActionData.UpdateLine> implements History {

    constructor(subtitle: Subtitle, data: ActionData.UpdateLine) {
        super(subtitle, { action: EditorActions.updateLine, data });
    }

    getLine(): Line {
        return this._subtitle.lines[this.data.index];
    }

    async undoActionAsync(): Promise<void> {
        const line = this.getLine();
        await line.applyAsync(this.data.beforeLineData);
    }

    async redoActionAsync(): Promise<void> {
        const line = this.getLine();
        await line.applyAsync(this.data.afterLineData);
    }
}
