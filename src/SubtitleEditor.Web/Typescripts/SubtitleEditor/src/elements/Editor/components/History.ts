// import EditorActions from "../contexts";
// import Editor from "../Editor";
// import { ModifiedState, ActionData } from "../models";
// import Line from "./Line";
// import Subtitle from "./Subtitle";

// export default interface History {

//     saved: boolean;
//     undoExecuted: boolean;

//     get action(): string;

//     deleteHistory(): void;
//     getLine(): Line | undefined;
//     undoAsync(): Promise<void>;
//     redoAsync(): Promise<void>;
//     getModifiedState(): ModifiedState;
// }

// abstract class HistoryBase<T> implements History {

//     protected _subtitle: Subtitle;
//     protected get _editor(): Editor { return this._subtitle.editor; }

//     actionWithData: ActionData.ActionWithData<T>;

//     deleted: boolean = false;
//     saved: boolean = false;
//     undoExecuted: boolean = false;

//     get action(): string { return this.actionWithData.action; }
//     get data(): T { return this.actionWithData.data; }

//     constructor(subtitle: Subtitle, actionWithData: ActionData.ActionWithData<T>);
//     constructor(subtitle: Subtitle, input: ActionData.ActionWithData<T>) {
//         this._subtitle = subtitle;
//         this.actionWithData = { ...input };
//     }

//     deleteHistory(): void {
//         this.deleted = true;
//     }

//     abstract getLine(): Line | undefined;

//     async undoAsync(): Promise<void> {
//         if (this.deleted || this.undoExecuted) {
//             return;
//         }

//         await this.undoActionAsync();
//         this.undoExecuted = true;
//     }

//     async redoAsync(): Promise<void> {
//         if (this.deleted || !this.undoExecuted) {
//             return;
//         }

//         await this.redoActionAsync();
//         this.undoExecuted = false;
//     }

//     getModifiedState(): ModifiedState {
//         return {
//             action: this.action,
//             data: this.data,
//             undoExecuted: this.undoExecuted
//         };
//     }

//     protected abstract undoActionAsync(): Promise<void>;
//     protected abstract redoActionAsync(): Promise<void>;
// }

// export class HistoryFactory {

//     static produce(subtitle: Subtitle, modifiedState: ModifiedState): History | undefined {
//         let history: History | undefined;
//         if (modifiedState.action === EditorActions.createLine) {
//             history = new CreateLineHistory(subtitle, modifiedState.data);
//         } else if (modifiedState.action === EditorActions.insertLine) {
//             history = new InsertLineHistory(subtitle, modifiedState.data);
//         } else if (modifiedState.action === EditorActions.deleteLine) {
//             history = new DeleteLineHistory(subtitle, modifiedState.data);
//         } else if (modifiedState.action === EditorActions.updateLine) {
//             history = new UpdateLineHistory(subtitle, modifiedState.data);
//         } else if (modifiedState.action === EditorActions.updateLines) {
//             history = new UpdateLinesHistory(subtitle, modifiedState.data);
//         }

//         if (history && modifiedState.undoExecuted) {
//             history.undoExecuted = true;
//         }

//         return history;
//     }
// }

// export class CreateLineHistory extends HistoryBase<ActionData.CreateLine> implements History {

//     constructor(subtitle: Subtitle, data: ActionData.CreateLine) {
//         super(subtitle, { action: EditorActions.createLine, data });
//     }

//     getLine(): Line | undefined {
//         return !this.undoExecuted ? this._subtitle.lines[this._subtitle.lines.length - 1] : undefined;
//     }

//     async undoActionAsync(): Promise<void> {
//         await this._subtitle.deleteAsync(this.getLine());
//         this._subtitle.transcript.insertSpan(this.data.startSpanIndex, this.data.content);
//     }

//     async redoActionAsync(): Promise<void> {
//         const line = this._subtitle.transcript.bindSpan(this._subtitle.transcript.listSpan(this.data.startSpanIndex, this.data.endSpanIndex - this.data.startSpanIndex + 1));
//         await line.setStartAsync(this.data.start);
//         await line.setEndAsync(this.data.end);
//     }
// }

// export class InsertLineHistory extends HistoryBase<ActionData.InsertLine> implements History {

//     constructor(subtitle: Subtitle, data: ActionData.InsertLine) {
//         super(subtitle, { action: EditorActions.insertLine, data });
//     }

//     getLine(): Line | undefined {
//         return !this.undoExecuted ? this._subtitle.lines[this.data.index] : undefined;
//     }

//     async undoActionAsync(): Promise<void> {
//         await this._subtitle.deleteAsync(this.getLine());
//     }

//     async redoActionAsync(): Promise<void> {
//         await this._subtitle.insertAsync(this.data.index - 1);
//     }
// }

// export class DeleteLineHistory extends HistoryBase<ActionData.DeleteLine> implements History {

//     constructor(subtitle: Subtitle, data: ActionData.DeleteLine) {
//         super(subtitle, { action: EditorActions.deleteLine, data });
//     }

//     getLine(): Line | undefined {
//         return this.undoExecuted ? this._subtitle.lines[this.data.index] : undefined;
//     }

//     async undoActionAsync(): Promise<void> {
//         const line = await this._subtitle.insertAsync(this.data.index - 1);
//         await line.initDataAsync(this.data.lineData);
//         line.edited = this.data.edited;
//         await line.updateAsync();
//     }

//     async redoActionAsync(): Promise<void> {
//         await this._subtitle.deleteAsync(this.getLine());
//     }
// }

// export class UpdateLineHistory extends HistoryBase<ActionData.UpdateLine> implements History {

//     constructor(subtitle: Subtitle, data: ActionData.UpdateLine) {
//         super(subtitle, { action: EditorActions.updateLine, data });
//     }

//     getLine(): Line {
//         return this._subtitle.lines[this.data.index];
//     }

//     async undoActionAsync(): Promise<void> {
//         const line = this.getLine();
//         await line.applyAsync(this.data.beforeLineData);
//     }

//     async redoActionAsync(): Promise<void> {
//         const line = this.getLine();
//         await line.applyAsync(this.data.afterLineData);
//     }
// }

// export class UpdateLinesHistory extends HistoryBase<ActionData.UpdateLines> implements History {

//     constructor(subtitle: Subtitle, data: ActionData.UpdateLines) {
//         super(subtitle, { action: EditorActions.updateLines, data });
//     }

//     getLine(): Line | undefined {
//         return undefined;
//     }

//     getLineByIndex(index: number): Line {
//         return this._subtitle.lines[index];
//     }

//     async undoActionAsync(): Promise<void> {
//         const tasks = this.data.beforeLineDatas.map(async ({ index, lineData }) => {
//             const line = this.getLineByIndex(index);
//             await line.setStartAsync(lineData.start);
//             await line.setEndAsync(lineData.end);
//         });
//         await Promise.all(tasks);
//     }

//     async redoActionAsync(): Promise<void> {
//         const tasks = this.data.afterLineDatas.map(async ({ index, lineData }) => {
//             const line = this.getLineByIndex(index);
//             await line.setStartAsync(lineData.start);
//             await line.setEndAsync(lineData.end);
//         });
//         await Promise.all(tasks);
//     }
// }