import { CreateLineCommand, DeleteLinesCommand, InsertLineCommand, RecreateTimeCommand, SaveCommand, ShiftAllLinesCommand, ShiftLinesCommand, UpdateLineCommand } from "elements/Editor/commands";
import SubtitleEditor, { LineData, SubtitleData, ModifiedState } from "./elements/Editor";

export default SubtitleEditor;
export {
    SubtitleEditor,
    SubtitleData,
    LineData,
    ModifiedState,

    CreateLineCommand,
    DeleteLinesCommand,
    InsertLineCommand,
    SaveCommand,
    ShiftAllLinesCommand,
    ShiftLinesCommand,
    UpdateLineCommand,
    RecreateTimeCommand
}

// (<any>window).SubtitleEditor = SubtitleEditor;