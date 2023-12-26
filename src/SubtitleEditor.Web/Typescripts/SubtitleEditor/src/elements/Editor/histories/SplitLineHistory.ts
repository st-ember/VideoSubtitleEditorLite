import { Line, Subtitle } from "../components";
import EditorActions from "../contexts";
import { ActionData } from "../models";
import HistoryBase from "./base/HistoryBase";
import History from "./base/History";
import SplitLineCommand from "../commands/SplitLineCommand";

export default class SplitLineHistory extends HistoryBase<ActionData.SplitLine> implements History {

    constructor(subtitle: Subtitle, data: ActionData.SplitLine) {
        super(subtitle, { action: EditorActions.splitLine, data });
    }

    getLine(): Line | undefined {
        return !this.undoExecuted ? this._subtitle.lines[this.data.index] : undefined;
    }

    getSecondLine(): Line | undefined {
        return !this.undoExecuted ? this._subtitle.lines[this.data.index + 1] : undefined;
    }

    async undoActionAsync(): Promise<void> {
        const line = this.getLine()!;
        const secondLine = this.getSecondLine()!;
        
        if (line.segmentMode) {
            const lineData = line.inputData;
            lineData.wordSegments = lineData.wordSegments!.concat(secondLine.inputData.wordSegments!);
            lineData.content = lineData.wordSegments.map(o => o.word).join("");
            lineData.end = secondLine.inputData.end;

            await secondLine.subtitle.deleteAsync(secondLine);
            await line.applyAsync(lineData);
        } else {
            const lineData = line.inputData;
            lineData.content = `${lineData.content}${secondLine.inputData.content}`;
            lineData.end = secondLine.inputData.end;

            await secondLine.subtitle.deleteAsync(secondLine);
            await line.applyAsync(lineData);
        }
    }

    async redoActionAsync(): Promise<void> {
        SplitLineCommand.doSplitAsync(this._subtitle.lines[this.data.index], this.data.caretPosition);
    }
}