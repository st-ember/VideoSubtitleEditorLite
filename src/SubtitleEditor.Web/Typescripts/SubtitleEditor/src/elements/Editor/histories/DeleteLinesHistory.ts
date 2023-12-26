import { Line, Subtitle } from "../components";
import EditorActions from "../contexts";
import { ActionData } from "../models";
import HistoryBase from "./base/HistoryBase";
import History from "./base/History";

export default class DeletesLineHistory extends HistoryBase<ActionData.DeleteLines> implements History {

    constructor(subtitle: Subtitle, data: ActionData.DeleteLines) {
        super(subtitle, { action: EditorActions.deleteLines, data });
    }

    getLine(): Line | undefined {
        return undefined;
    }

    async undoActionAsync(): Promise<void> {
        for (let i = 0; i < this.data.lines.length; i++) {
            const data = this.data.lines[i];
            const line = await this._subtitle.insertAsync(data.index - 1);
            await line.initDataAsync(data.lineState);
            await line.applyAsync(data.lineData);
        }
    }

    async redoActionAsync(): Promise<void> {
        for (let i = 0; i < this.data.lines.length; i++) {
            const line = this._subtitle.lines[this.data.lines[i].index];
            await this._subtitle.deleteAsync(line);
        }
    }
}
