import { Line } from "../../components";
import { ModifiedState } from "../../models";

export default interface History {

    saved: boolean;
    undoExecuted: boolean;

    get action(): string;

    deleteHistory(): void;
    getLine(): Line | undefined;
    undoAsync(): Promise<void>;
    redoAsync(): Promise<void>;
    getModifiedState(): ModifiedState;
}