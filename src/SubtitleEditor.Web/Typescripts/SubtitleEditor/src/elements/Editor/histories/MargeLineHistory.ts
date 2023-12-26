import { Line, Subtitle } from "../components";
import EditorActions from "../contexts";
import { ActionData } from "../models";
import HistoryBase from "./base/HistoryBase";
import History from "./base/History";
import { MargeLineCommand } from "../commands";

export default class MargeLineHistory extends HistoryBase<ActionData.MargeLine> implements History {

    constructor(subtitle: Subtitle, data: ActionData.MargeLine) {
        super(subtitle, { action: EditorActions.margeLine, data });
    }

    getLine(): Line | undefined {
        return !this.undoExecuted ? this._subtitle.lines[this.data.index] : undefined;
    }

    async undoActionAsync(): Promise<void> {
        const line = this.getLine()!;

        const insertCount = this.data.lineDatas.length - 1;
        const lines = [line];
        for (let i = 0; i < insertCount; i++) {
            const newLine = await line.subtitle.insertAsync(lines[lines.length - 1].index);
            lines.push(newLine);
        }

        await line.initDataAsync(this.data.lineStates[0]);
        await line.applyAsync(this.data.lineDatas[0]);

        for (let i = 1; i < this.data.lineDatas.length; i++) {
            await lines[i].initDataAsync(this.data.lineStates[i]);
            await lines[i].applyAsync(this.data.lineDatas[i]);
        }
    }

    async redoActionAsync(): Promise<void> {
        const minIndex = this.data.index;
        const maxIndex = minIndex + this.data.lineDatas.length - 1;
        const lines = this._subtitle.lines.filter(line => line.index >= minIndex && line.index <= maxIndex);
        MargeLineCommand.doMargeAsync(lines)
    }
}