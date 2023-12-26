import { Line } from "../components";

export default class SelectLineCommand {

    static async invokeAsync(line: Line): Promise<void> {
        line.selectAsync(true);
        line.subtitle.editor.latestSelectedLineIndex = line.index;

        const allSelectedLines = line.subtitle.selectedLines;
        if (allSelectedLines.length > 0) {
            if (line.subtitle.editor.keyState.shift) {
                const currentMin = allSelectedLines[0].index;
                const currentMax = allSelectedLines[allSelectedLines.length - 1].index;

                line.subtitle.lines.forEach(o => {
                    if (o.index > currentMin && o.index < currentMax) {
                        o.selectAsync(true);
                    }
                });
            } else if (!line.subtitle.editor.keyState.control) {
                allSelectedLines.filter(o => o !== line).forEach(o => o.selectAsync(false));
            }
        }

        line.subtitle.editor.updateMultipleSelectionButton();
    }
}