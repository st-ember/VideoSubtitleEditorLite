import { Subtitle, Line } from "../components";
import EditorActions from "../contexts";
import { ActionData } from "../models";
import HistoryBase from "./base/HistoryBase";
import History from "./base/History";

export default class UpdateLinesHistory extends HistoryBase<ActionData.UpdateLines> implements History {

    constructor(subtitle: Subtitle, data: ActionData.UpdateLines) {
        super(subtitle, { action: EditorActions.updateLines, data });
    }

    getLine(): Line | undefined {
        return undefined;
    }

    getLineByIndex(index: number): Line {
        return this._subtitle.lines[index];
    }

    async undoActionAsync(): Promise<void> {
        const tasks = this.data.beforeLineDatas.map(async ({ index, lineData }) => {
            const line = this.getLineByIndex(index);
            await line.applyAsync(lineData);
        });
        await Promise.all(tasks);
    }

    async redoActionAsync(): Promise<void> {
        const tasks = this.data.afterLineDatas.map(async ({ index, lineData }) => {
            const line = this.getLineByIndex(index);
            await line.applyAsync(lineData);
        });
        await Promise.all(tasks);
    }
}
