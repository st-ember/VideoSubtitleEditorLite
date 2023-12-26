import { Line, Subtitle } from "../components";
import EditorActions from "../contexts";
import { ActionData } from "../models";
import HistoryBase from "./base/HistoryBase";
import History from "./base/History";


export default class InsertLineHistory extends HistoryBase<ActionData.InsertLine> implements History {

    constructor(subtitle: Subtitle, data: ActionData.InsertLine) {
        super(subtitle, { action: EditorActions.insertLine, data });
    }

    getLine(): Line | undefined {
        return !this.undoExecuted ? this._subtitle.lines[this.data.index] : undefined;
    }

    async undoActionAsync(): Promise<void> {
        await this._subtitle.deleteAsync(this.getLine());
    }

    async redoActionAsync(): Promise<void> {
        await this._subtitle.insertAsync(this.data.index - 1);
    }
}
