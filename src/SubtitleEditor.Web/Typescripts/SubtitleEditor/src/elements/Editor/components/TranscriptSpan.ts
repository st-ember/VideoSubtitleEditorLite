import CreateLineCommand from "../commands/CreateLineCommand";
import CreateLineHistory from "../histories/CreateLineHistory";
import Line from "./Line";
import Transcript from "./Transcript";

export default class TranscriptSpan {

    transcript: Transcript;

    text: string = "";
    element: HTMLSpanElement | HTMLBRElement = undefined!;

    line?: Line;
    playing: boolean = false;
    binded: boolean = false;
    covered: boolean = false;
    selected: boolean = false;
    endOfLine: boolean = false;

    get index(): number { return this.transcript.spans.indexOf(this); }

    constructor(transcript: Transcript, text: string) {
        this.transcript = transcript;
        this.text = text;
        this._build();
        this._bindEvent();
    }

    setCovered(covered: boolean): void {
        this.covered = covered;
        this._editClass("covered", covered);
    }

    setSelected(selected: boolean): void {
        this.selected = selected;
        this._editClass("selected", selected);

        if (selected) {
            this.covered = false;
            this._editClass("covered", false);
        }
    }

    setBinded(binded: boolean): void {
        this.binded = binded;
        this._editClass("binded", binded);

        if (binded) {
            this.covered = false;
            this.selected = false;
            this._editClass("covered", false);
            this._editClass("selected", false);
            this._editClass("first", false);
            this._editClass("last", false);
        }
    }

    setBindedFirst(first: boolean): void {
        this._editClass("first", first);
    }

    setBindedLast(last: boolean): void {
        this._editClass("last", last);
    }

    setHover(hover: boolean): void {
        this._editClass("hover", hover);
    }

    setPlaying(playing: boolean): void {
        this._editClass("playing", playing);
    }

    deleteSpan(): void {
        this.line = undefined;
        this.element.remove();
    }

    private _build(): void {
        if (this.text !== "\n") {
            this.element = document.createElement("span");
            this.element.className = "se-transcript-span";
            this.element.innerText = this.text;
    
            if (this.text.match(/[a-zA-Z,\.\+\=\-\~\!\$\^\*\(\)\[\]\{\}\/\\\?\>\<\#\@\&\%\`\'\"\:\;\d\s]/g)) {
                this.element.classList.add("half");
            }
        } else {
            this.element = document.createElement("br");
            this.element.className = "se-transcript-span end-of-line";
            this.endOfLine = true;
        }
    }

    private _bindEvent(): void {
        this.element.addEventListener("mouseenter", () => {
            if (!this.binded) {
                this.transcript.selectSpan(this);
            } else {
                this.transcript.clearSelectedSpan();
                if (this.line && this.line.spans) {
                    this.line.spans.forEach(span => span.setHover(true));
                }
            }
        });

        this.element.addEventListener("mouseleave", () => {
            if (this.binded && this.line && this.line.spans) {
                this.line.spans.forEach(span => span.setHover(false));
            }
        });

        this.element.addEventListener("click", () => {
            if (!this.binded) {
                const spans = this.transcript.listCoveredSpan();
                CreateLineCommand.invokeAsync(this.transcript, spans);
            } else if (this.line) {
                this.line.triggerTimestamp();
            }
        });
    }

    private _editClass(className: string, add: boolean): void {
        if (add) {
            this.element.classList.add(className);
        } else {
            this.element.classList.remove(className);
        }
    }
}