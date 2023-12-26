import Editor, { ModifiedState } from "elements/Editor";
import { Line, Subtitle } from "elements/Editor/components";
import { ActionData } from "elements/Editor/models";
import History from "./History";

export default abstract class HistoryBase<T> implements History {

    protected _subtitle: Subtitle;
    protected get _editor(): Editor { return this._subtitle.editor; }

    actionWithData: ActionData.ActionWithData<T>;

    deleted: boolean = false;
    saved: boolean = false;
    undoExecuted: boolean = false;

    get action(): string { return this.actionWithData.action; }
    get data(): T { return this.actionWithData.data; }

    constructor(subtitle: Subtitle, actionWithData: ActionData.ActionWithData<T>);
    constructor(subtitle: Subtitle, input: ActionData.ActionWithData<T>) {
        this._subtitle = subtitle;
        this.actionWithData = { ...input };
    }

    deleteHistory(): void {
        this.deleted = true;
    }

    abstract getLine(): Line | undefined;

    async undoAsync(): Promise<void> {
        if (this.deleted || this.undoExecuted) {
            return;
        }

        await this.undoActionAsync();
        this.undoExecuted = true;
    }

    async redoAsync(): Promise<void> {
        if (this.deleted || !this.undoExecuted) {
            return;
        }

        await this.redoActionAsync();
        this.undoExecuted = false;
    }

    getModifiedState(): ModifiedState {
        return {
            action: this.action,
            data: this.data,
            undoExecuted: this.undoExecuted
        };
    }

    protected abstract undoActionAsync(): Promise<void>;
    protected abstract redoActionAsync(): Promise<void>;
}