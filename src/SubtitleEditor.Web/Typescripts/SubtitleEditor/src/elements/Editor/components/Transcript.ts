import Util from "../../Util";
import Editor from "../Editor";
import { SubtitleOptions } from "../options";
import Line from "./Line";
import Subtitle from "./Subtitle";
import TranscriptSpan from "./TranscriptSpan";

export default class Transcript {

    editor: Editor;

    transcriptContainer: HTMLDivElement = document.createElement("div");
    spanContainer: HTMLDivElement = document.createElement("div");
    wordLimitIndicator: HTMLDivElement = document.createElement("div");
    spans: TranscriptSpan[] = [];
    selectedSpan?: TranscriptSpan;

    onModeStateInitialFuncs: (() => void)[] = [];
    modeState = {
        /** 快速斷行模式，按下快速鍵後將最上面一行直接建立成新字幕。 */
        quickBreakLine: false,
        /** 是否允許使用快速段行模式 */
        quickBreakLineable: false,
        /** 是否允許直接編輯尚未建立成字幕的逐字稿 */
        modifing: false
    };

    get index(): number { return this.editor.transcripts.indexOf(this); }
    get subtitle(): Subtitle { return this.editor.subtitles[this.index]; }
    get containsSpan(): boolean { return this.spans.length > 0; }
    get content(): string { return this.spans.map(span => span.text).join(""); }

    private _options: SubtitleOptions;
    private _transcriptEditor?: HTMLDivElement;

    constructor(editor: Editor, options: SubtitleOptions) {
        this._options = { ...options };
        this.editor = editor;
        this._build();
    }

    setActive(active: boolean): void {
        if (active) {
            this.transcriptContainer.classList.add("active");
        } else {
            this.transcriptContainer.classList.remove("active");
        }
    }

    setLoaded(loaded: boolean): void {
        if (loaded) {
            this.transcriptContainer.classList.add("loaded");
        } else {
            this.transcriptContainer.classList.remove("loaded");
        }
    }

    selectSpan(span: TranscriptSpan): void {
        if (this.selectedSpan) {
            this.selectedSpan.setSelected(false);
        }

        this.selectedSpan = span;
        this.selectedSpan.setSelected(true);
        const selectedIndex = this.selectedSpan.index;
        for (let i = 0; i < this.spans.length; i++) {
            const span = this.spans[i];
            if (!span.binded && !span.covered && i < selectedIndex) {
                span.setCovered(true);
            } else if (i > selectedIndex) {
                if (span.covered) {
                    span.setCovered(false);
                } else {
                    break;
                }
            }
        }
    }

    clearSelectedSpan(): void {
        if (this.selectedSpan) {
            this.selectedSpan.setSelected(false);
            this.selectedSpan = undefined;
        }

        for (let i = 0; i < this.spans.length; i++) {
            const span = this.spans[i];
            if (!span.binded) {
                if (span.covered) {
                    span.setCovered(false);
                } else {
                    break;
                }
            }
        }
    }

    listSpan(index: number, count: number): TranscriptSpan[] {
        const spans: TranscriptSpan[] = [];
        for (let i = index; i < this.spans.length && spans.length < count; i++) {
            spans.push(this.spans[i]);
        }
        return spans;
    }

    listCoveredSpan(): TranscriptSpan[] {
        if (!this.selectedSpan) {
            return [];
        }

        const spans: TranscriptSpan[] = [];
        for (let i = 0; i < this.spans.length && i < this.selectedSpan.index + 1; i++) {
            const span = this.spans[i];
            if (!span.binded) {
                span.setBinded(true);
                spans.push(span);
            }
        }

        for (let i = this.selectedSpan.index + 1; i < this.spans.length; i++) {
            const span = this.spans[i];
            if (!span.binded && span.endOfLine) {
                span.setBinded(true);
                spans.push(span);
            } else {
                break;
            }
        }

        return spans;
    }

    listFirstLine(): TranscriptSpan[] {
        const spans: TranscriptSpan[] = [];
        for (let i = 0; i < this.spans.length; i++) {
            const span = this.spans[i];
            if (!span.binded) {
                span.setBinded(true);
                spans.push(span);

                if (span.endOfLine) {
                    break;
                }
            }
        }

        return spans;
    }

    bindSpan(spans: TranscriptSpan[]): Line {
        this.selectedSpan = undefined;
        spans.forEach(span => span.setBinded(true));
        spans[0].setBindedFirst(true);

        const notEndOfLineSpans = spans.filter(span => !span.endOfLine);
        notEndOfLineSpans[notEndOfLineSpans.length - 1].setBindedLast(true);

        const content = spans.map(span => span.text).join("");
        const subtitle = this.subtitle;
        const lastLine = subtitle.lines.length > 0 ? subtitle.lines[subtitle.lines.length - 1] : undefined;
        const line = new Line(subtitle, 
            {
                start: lastLine ? lastLine.endText : Util.DEFAULT_TIME,
                end: Util.formatTime(this.editor.time),
                content
            },
            { insertManually: true });

        line.spans = spans;
        spans.forEach(span => span.line = line);

        subtitle.append(line);
        this.editor.updateLineState();

        return line;
    }

    insertSpan(index: number, content: string): TranscriptSpan[] {
        const spans: TranscriptSpan[] = [];
        const nextSpan = index < this.spans.length ? this.spans[index] : undefined;

        for (let i = 0; i < content.length; i++) {
            const span = new TranscriptSpan(this, content[i]);
            if (nextSpan) {
                this.spanContainer.insertBefore(span.element, nextSpan.element);
            } else {
                this.spanContainer.appendChild(span.element);
            }
            
            spans.push(span);
        }

        this.spans.splice(index, 0, ...spans);
        return spans;
    }

    clearSpan(): void {
        this.spans.forEach(span => span.deleteSpan());
        this.spans = [];
    }

    updateBreakLineMode(): void {
        const lineCount = this.spans.filter(span => span.endOfLine).length + 1;
        if (lineCount > 2) {
            this.modeState.quickBreakLineable = true;
            if (lineCount > 20) {
                this.modeState.quickBreakLine = true;
            }
        } else {
            this.modeState.quickBreakLineable = false;
        }
    }

    setModifing(modifying: boolean): void {
        if (this.modeState.modifing === modifying) {
            return;
        }

        this.modeState.modifing = modifying;

        if (this.modeState.modifing) {
            const bindedSpans: TranscriptSpan[] = [];
            const unbindedSpanTexts: string[] = [];
            for (var i = 0; i < this.spans.length; i++) {
                const span = this.spans[i];
                if (span.binded) {
                    bindedSpans.push(span);
                    continue;
                }
    
                unbindedSpanTexts.push(span.text);
                span.deleteSpan();
            }
    
            this.spans = bindedSpans;

            this._transcriptEditor = document.createElement("div");
            this._transcriptEditor.className = "se-transcript-editor";
            this._transcriptEditor.contentEditable = "true";
            this._transcriptEditor.innerText = unbindedSpanTexts.join("");
            this.spanContainer.appendChild(this._transcriptEditor);

            this.editor.onHotkeyStateChange(false);
        } else {
            const content = this._transcriptEditor ? this._transcriptEditor.innerText : "";
            if (this._transcriptEditor) {
                this._transcriptEditor.remove();
            }

            for (let i = 0; i < content.length; i++) {
                const span = new TranscriptSpan(this, content[i]);
                this.spanContainer.appendChild(span.element);
                this.spans.push(span);
            }

            this.editor.onHotkeyStateChange(true);
        }
    }

    setWordLimit(wordLimit?: number): void {
        if (!wordLimit) {
            this.wordLimitIndicator.classList.add("hide");
        } else {
            this.wordLimitIndicator.classList.remove("hide");
            this.wordLimitIndicator.style.left = `${wordLimit * 16 + 10}px`;
        }
    }

    private _build(): void {
        this.transcriptContainer.className = "se-transcript-container";
        if (!this._options.transcript) {
            this.transcriptContainer.classList.add("hide");
        }

        this._buildWordLimitIndicator();
        this._buildContent();
        this._bindEvent();
    }

    private _buildWordLimitIndicator(): void {
        this.wordLimitIndicator.className = "se-word-limit-indicator";
        this.transcriptContainer.appendChild(this.wordLimitIndicator);
    }

    private _buildContent(): void {
        if (!this._options.transcript) {
            return;
        }

        this.spanContainer.className = "se-transcript-span-container";
        this.transcriptContainer.appendChild(this.spanContainer);

        for (let i = 0; i < this._options.transcript.length; i++) {
            const span = new TranscriptSpan(this, this._options.transcript[i]);
            this.spanContainer.appendChild(span.element);
            this.spans.push(span);
        }

        this.updateBreakLineMode();
        this.onModeStateInitialFuncs.forEach(func => func());
    }

    private _bindEvent(): void {
        this.transcriptContainer.addEventListener("mouseleave", () => {
            this.clearSelectedSpan();
        });
    }
}