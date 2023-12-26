import { SubtitleOptions } from "../options";
import Line from "./Line";
import Util from "../../Util";
import Editor from "../Editor";
import { LineData, SubtitleData } from "../models";
import Transcript from "./Transcript";
import History from "../histories/base/History";
import HistoryFactory from "../histories/HistoryFactory";
import InsertLineCommand from "../commands/InsertLineCommand";

export default class Subtitle {

    editor: Editor;

    lineContainer: HTMLDivElement = document.createElement("div");
    inserter: HTMLDivElement = document.createElement("div");
    wordLimitIndicator: HTMLDivElement = document.createElement("div");
    lines: Line[] = [];

    get index(): number { return this.editor.subtitles.indexOf(this); }
    get transcript(): Transcript { return this.editor.transcripts[this.index]; }
    get selectedLines(): Line[] { return this.lines.filter(line => line.selected); }

    histories: History[] = [];
    historyIndex: number = -1;
    onHistoryChanged: () => Promise<void> = async () => {};
    onInserted: () => Promise<void> = async () => {};
    onDeleted: () => Promise<void> = async () => {};

    private _options: SubtitleOptions;
    private _modifiedStateLoaded: boolean = false;
    private _showAnimation: boolean = true;
    private get _animationLength(): number { return this._showAnimation ? 400 : 0; }

    get data(): SubtitleData {
        return {
            lines: this.lines.map(line => line.data),
            srt: Util.convertToSrt(this.lines.map(line => line.data)),
            modifiedStates: this.histories.map(history => history.getModifiedState())
        };
    }

    constructor(editor: Editor, options: SubtitleOptions) {
        this._options = { ...options };
        this.editor = editor;
        this._build();
    }

    async setActiveAsync(active: boolean): Promise<void> {
        if (active) {
            this.lineContainer.classList.add("active");

            if (!this._modifiedStateLoaded) {
                this._modifiedStateLoaded = true;
                await this._buildHistoriesAsync();
            }
        } else {
            this.lineContainer.classList.remove("active");
        }
    }

    setLoaded(loaded: boolean): void {
        if (loaded) {
            this.lineContainer.classList.add("loaded");
        } else {
            this.lineContainer.classList.remove("loaded");
        }
    }

    setShowAnimation(show: boolean): void {
        this._showAnimation = show;
        if (show) {
            this.lineContainer.classList.remove("se-no-animation");
        } else {
            this.lineContainer.classList.add("se-no-animation");
        }
    }

    async saveAsync(): Promise<void> {
        for (let i = 0; i < this.lines.length; i++) {
            await this.lines[i].saveAsync();
        }
    }

    async resetAsync(): Promise<void> {
        for (let i = 0; i < this.lines.length; i++) {
            const line = this.lines[i];
            line.setPlaying(false);
            line.cancelAsync();
        }
    }

    async insertAsync(index: number): Promise<Line> {
        const prev = index >= 0 && index < this.lines.length ? this.lines[index] : undefined;
        const next = index + 1 < this.lines.length ? this.lines[index + 1] : undefined;
        const line = new Line(
            this, 
            {
                start: prev ? prev.endText : Util.DEFAULT_TIME,
                end: next ? next.startText : (prev ? prev.endText : Util.DEFAULT_TIME),
                content: ""
            }, 
            { insertManually: true }
            );

        if (next) {
            this.lines.splice(index + 1, 0, line);
            this.lineContainer.insertBefore(line.element, next.element);
        } else {
            this.lines.push(line);
            this.lineContainer.appendChild(line.element);
        }

        setTimeout(() => line.setInserting(false), this._animationLength);
        await this.onInserted();

        return line;
    }

    append(line: Line): Line {
        this.lines.push(line);
        this.lineContainer.appendChild(line.element);
        setTimeout(() => line.setInserting(false), this._animationLength);
        this.onInserted();
        return line;
    }

    async deleteAsync(line?: Line): Promise<void> {
        if (!line) {
            return;
        }
        
        await line.setDeletingAsync(this._animationLength);
        this.lines.splice(line.index, 1);
        await line.deleteAsync();
        await this.onDeleted();
    }

    async addHistoryAsync(history: History): Promise<void> {
        if (this.historyIndex < this.histories.length - 1) {
            this.histories
                .splice(this.historyIndex + 1, (this.histories.length - 1) - this.historyIndex)
                .forEach(removed => removed.deleteHistory());
        }

        this.histories.push(history);
        this.historyIndex++;
        
        this.onHistoryChanged();
    }

    setWordLimit(wordLimit?: number): void {
        if (!wordLimit) {
            this.wordLimitIndicator.classList.add("hide");
        } else {
            this.wordLimitIndicator.classList.remove("hide");
            this.wordLimitIndicator.style.left = `${wordLimit * 16 + 30}px`;
        }
    }

    private _build(): void {
        this.lineContainer.className = "se-line-container";

        this._buildWordLimitIndicator();
        this._buildInserter();
        this._buildLines();
    }

    private _buildWordLimitIndicator(): void {
        this.wordLimitIndicator.className = "se-word-limit-indicator";
        this.lineContainer.appendChild(this.wordLimitIndicator);
    }

    private _buildInserter(): void {
        this.inserter.className = "se-inserter";
        this.inserter.title = "在這裡插入一行新的字幕";
        this.lineContainer.appendChild(this.inserter);
        this.inserter.addEventListener("click", () => {
            InsertLineCommand.invokeAsync(this, -1);
        });
    }

    private _buildLines(): void {
        if (!!this._options.transcript) {
            return;
        }

        let lineDatas: LineData[] = [];
        if (this._options.lines) {
            lineDatas = this._options.lines;
        } else if (this._options.srt) {
            try {
                lineDatas = Util.parseSrt(this._options.srt);
            } catch (error) {
                console.error(`解析 SRT 資料時發生錯誤：${error}`);
            }
        }

        this.lines = lineDatas.map(lineData => {
            const line = new Line(this, lineData, { insertManually: false });
            this.lineContainer.appendChild(line.element);
            return line;
        });
    }

    private async _buildHistoriesAsync(): Promise<void> {
        if (!this._options.modifiedStates) {
            return;
        }

        this.setShowAnimation(false);

        for (let i = 0; i < this._options.modifiedStates.length; i++) {
            const state = this._options.modifiedStates[i];
            const history = HistoryFactory.produce(this, state);
            if (history) {
                this.histories.push(history);
                if (!!this._options.transcript) {
                    if (!history.undoExecuted) {
                        history.undoExecuted = true;
                        await history.redoAsync();
                        this.historyIndex++;
                    }
                } else if (!history.undoExecuted) {
                    this.historyIndex++;
                }
            }
        }

        this.setShowAnimation(true);
    }
}