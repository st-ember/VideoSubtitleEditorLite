import { Subtitle } from "../components";
import EditorActions from "../contexts";
import { ModifiedState } from "../models";
import CreateLineHistory from "./CreateLineHistory";
import DeletesLineHistory from "./DeleteLinesHistory";
import InsertLineHistory from "./InsertLineHistory";
import SplitLineHistory from "./SplitLineHistory";
import UpdateLineHistory from "./UpdateLineHistory";
import ShiftLinesHistory from "./ShiftLinesHistory";
import History from "./base/History";

export default class HistoryFactory {

    static produce(subtitle: Subtitle, modifiedState: ModifiedState): History | undefined {
        let history: History | undefined;
        if (modifiedState.action === EditorActions.createLine) {
            history = new CreateLineHistory(subtitle, modifiedState.data);
        } else if (modifiedState.action === EditorActions.insertLine) {
            history = new InsertLineHistory(subtitle, modifiedState.data);
        } else if (modifiedState.action === EditorActions.deleteLines) {
            history = new DeletesLineHistory(subtitle, modifiedState.data);
        } else if (modifiedState.action === EditorActions.updateLine) {
            history = new UpdateLineHistory(subtitle, modifiedState.data);
        } else if (modifiedState.action === EditorActions.shiftLines) {
            history = new ShiftLinesHistory(subtitle, modifiedState.data);
        } else if (modifiedState.action === EditorActions.splitLine) {
            history = new SplitLineHistory(subtitle, modifiedState.data);
        }

        if (history && modifiedState.undoExecuted) {
            history.undoExecuted = true;
        }

        return history;
    }
}