import { Line } from "../components";
import { WordSegment } from "../models";
import SplitLineHistory from "../histories/SplitLineHistory";
import Editor from "../Editor";
import Util from "elements/Util";

export default class SplitLineCommand {

    static async invokeAsync(editor: Editor): Promise<void>;
    static async invokeAsync(line: Line): Promise<void>;
    static async invokeAsync(input: Line | Editor): Promise<void> {
        const line = input instanceof Line ? input : input.editingLine;
        if (!line || line.caretPosition === undefined) {
            return;
        }

        const caretPosition = line.caretPosition;
        await line.subtitle.editor.cancelLineAsync();

        if (await SplitLineCommand.doSplitAsync(line, caretPosition)) {
            await line.subtitle.addHistoryAsync(new SplitLineHistory(line.subtitle, { index: line.index, caretPosition }));
        }
    }

    static async doSplitAsync(line: Line, caretPosition: number): Promise<boolean> {
        if (line.segmentMode && line.segments.length > 0) {
            const segments = <WordSegment[]>JSON.parse(JSON.stringify(line.segments));
            const secondLineSegments = caretPosition < segments.length - 1 ? segments.splice(caretPosition + 1, segments.length - caretPosition) : [];
            if (secondLineSegments.length === 0) {
                return false;
            }
    
            const firstLineSegments = segments;
            const firstLineInputData = { ...line.inputData };
            const originalEnd = firstLineInputData.end;
            
            firstLineInputData.content = firstLineSegments.map(o => o.word).join("");
            firstLineInputData.wordSegments = firstLineSegments;
            firstLineInputData.end = secondLineSegments[0].start;
    
            const secondLineInputData = {
                start: secondLineSegments[0].start,
                end: originalEnd,
                content: secondLineSegments.map(o => o.word).join(""),
                originalContent: undefined,
                wordSegments: secondLineSegments,
                originalWordSegments: undefined
            };
    
            await line.applyAsync(firstLineInputData);
            const secondLine = await line.subtitle.insertAsync(line.index);
            await secondLine.applyAsync(secondLineInputData);

            return true;
        } else if (!line.segmentMode && line.inputContent) {
            const totalLength = line.length;

            const firstLineInputData = { ...line.inputData };
            const firstContent = firstLineInputData.content.substring(0, caretPosition);
            const rateOfFirstContent = firstContent.length / firstLineInputData.content.length;
            const firstEndTime = line.startTime + Math.floor(totalLength * rateOfFirstContent * 100) / 100;

            const secondContent = firstLineInputData.content.substring(caretPosition);

            firstLineInputData.content = firstContent;
            firstLineInputData.end = Util.formatTime(firstEndTime);

            const secondLineInputData = {
                start: firstLineInputData.end,
                end: line.inputData.end,
                content: secondContent,
                originalContent: undefined,
                wordSegments: undefined,
                originalWordSegments: undefined
            };

            const secondLine = await line.subtitle.insertAsync(line.index);
            await line.applyAsync(firstLineInputData);
            await secondLine.applyAsync(secondLineInputData);

            return true;
        }

        return false;
    }
}
