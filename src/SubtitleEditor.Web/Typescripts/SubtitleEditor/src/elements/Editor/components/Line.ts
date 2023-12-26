import { LineData, LineDataState, WordSegment } from "../models";
import LineElement from "./LineElement";
import Subtitle from "./Subtitle";
import Util from "../../Util";
import { LineOptions } from "../options";
import TranscriptSpan from "./TranscriptSpan";
import Transcript from "./Transcript";
import InsertLineCommand from "../commands/InsertLineCommand";
import UpdateLineCommand from "../commands/UpdateLineCommand";
import { SelectLineCommand } from "../commands";

export default class Line {

    subtitle: Subtitle;

    get element(): HTMLDivElement { return this._lineElement.element; }
    get caretPosition(): number | undefined { return this._lineElement.caretPosition; }
    get caretAtFirstPosition(): boolean { return this._lineElement.caretAtFirstPosition; }
    get caretAtLastPosition(): boolean { return this._lineElement.caretAtLastPosition; }
    get editingTime(): boolean { return this._lineElement.editingTime; }
    get selected(): boolean { return this._lineElement.selectbox.checked; }
    get segmentMode(): boolean { return this._segmentMode; }

    get startText(): string { return this.datas.start.last; }
    get endText(): string { return this.datas.end.last; }
    get lengthText(): string { return Util.formatTime(this.length); }
    get startTime(): number { return Util.parseTime(this.startText); }
    get endTime(): number { return Util.parseTime(this.endText); }
    get length(): number { return this.endTime - this.startTime; }
    get content(): string { return this.datas.value.last; }
    set content(value: string) { this.datas.value.last = value; }
    get segments(): WordSegment[] { return this._segments; }

    get originalStartText(): string { return this.datas.start.recover !== undefined ? this.datas.start.recover : this.datas.start.init; }
    get originalEndText(): string { return this.datas.end.recover !== undefined ? this.datas.end.recover : this.datas.end.init; }
    get originalLengthText(): string { return Util.formatTime(this.length); }
    get originalStartTime(): number { return Util.parseTime(this.originalStartText); }
    get originalEndTime(): number { return Util.parseTime(this.originalEndText); }
    get originalLength(): number { return this.originalEndTime - this.originalStartTime; }
    get originalContent(): string { return this.datas.value.recover !== undefined ? this.datas.value.recover : this.datas.value.init; }
    get originalSegments(): WordSegment[] { return this._originalSegments; }

    get startInputText(): string { return this._lineElement.startInputText; }
    get endInputText(): string { return this._lineElement.endInputText; }
    get startInputTime(): number { return Util.parseTime(this.startInputText); }
    get endInputTime(): number { return Util.parseTime(this.endInputText); }
    get inputContent(): string { return this._lineElement.content; }
    get inputSegments(): WordSegment[] { return this._lineElement.segments; }
    set inputSegments(value: WordSegment[]) { this._lineElement.segments = value; }

    get saved(): boolean { return this.datas.value.recover !== undefined && this.datas.value.recover !== this.datas.value.last; }
    get index(): number { return this.subtitle.lines.indexOf(this); }
    get prev(): Line | undefined { const index = this.index; return index - 1 >= 0 ? this.subtitle.lines[index - 1] : undefined; }
    get next(): Line | undefined { const index = this.index; return this.subtitle.lines.length > index + 1 ? this.subtitle.lines[index + 1] : undefined; }
    get prevLines(): Line[] { const index = this.index; return index - 1 >= 0 ? this.subtitle.lines.filter((_, i) => i < index) : []; }
    get nextLines(): Line[] { const index = this.index; return this.subtitle.lines.length > index + 1 ? this.subtitle.lines.filter((_, i) => i > index) : []; }

    get data(): LineData {
        return {
            start: this.startText,
            end: this.endText,
            content: this.content,
            originalContent: this.originalContent,
            wordSegments: this.segments,
            originalWordSegments: this._originalSegments
        };
    }

    get originalData(): LineData {
        return {
            start: this.originalStartText,
            end: this.originalEndText,
            content: this.originalContent,
            originalContent: this.originalContent,
            wordSegments: this._originalSegments,
            originalWordSegments: this._originalSegments
        };
    }

    get inputData(): LineData {
        return {
            start: this.startInputText,
            end: this.endInputText,
            content: this.inputContent,
            originalContent: this.originalContent,
            wordSegments: this.inputSegments,
            originalWordSegments: this._originalSegments
        };
    }

    edited: boolean = false;
    playing: boolean = false;
    deleted: boolean = false;

    spans?: TranscriptSpan[];
    
    datas: { start: LineDataState; end: LineDataState; value: LineDataState; } = {
        start: { init: "", saved: "", recover: "", last: "" },
        end: { init: "", saved: "", recover: "", last: "" },
        value: { init: "", saved: "", recover: "", last: "" }
    };

    private _options: LineOptions;
    private _lineElement: LineElement;
    private _segmentMode: boolean = false;
    private _segments: WordSegment[] = [];
    private _originalSegments: WordSegment[] = [];

    constructor(subtitle: Subtitle, data: LineData, options: LineOptions) {
        this.subtitle = subtitle;
        this._options = { ...options };
        this._lineElement = new LineElement(this._options, subtitle.editor);
        this.initDataAsync({
            start: { init: data.start, saved: data.start, last: data.start },
            end: { init: data.end, saved: data.end, last: data.end },
            value: { init: data.content, saved: data.content, recover: data.originalContent, last: data.content }
        }).then(async () => {
            await this.initSegmentAsync(data);
            await this.updateAsync();
        });
        this._initEvents();
    }

    setEditable(editable: boolean): void {
        this._lineElement.setEditable(editable);
    }

    setCaretToStart(): void {
        this._lineElement.setCaretToStart();
    }

    setCaretToEnd(): void {
        this._lineElement.setCaretToEnd();
    }

    setPlaying(playing: boolean): void {
        if (this.playing !== playing) {
            this.playing = playing;
            this._lineElement.setPlaying(playing);

            if (this.spans) {
                this.spans.forEach(span => span.setPlaying(playing));
            }
        }
    }

    setInserting(inserting: boolean): void {
        this._lineElement.setInserting(inserting);
    }

    setDeletingAsync(animationLength: number): Promise<void> {
        return this._lineElement.setDeletingAsync(animationLength);
    }

    setHighlight(highlight: boolean): void {
        this._lineElement.setHighlight(highlight);
    }

    searchText(text: string): boolean {
        return this._lineElement.searchText(text);
    }

    setSearchHighlight(highlight: boolean): void {
        this._lineElement.setSearchHighlight(highlight);
    }

    clearSearch(): void {
        this._lineElement.setSearchHighlight(false);
        this._lineElement.clearSearch();
    }

    replaceText(target: string, text: string): void {
        this._lineElement.replaceText(target, text);
    }

    async initDataAsync(data: { start: LineDataState; end: LineDataState; value: LineDataState; }): Promise<void> {
        this.datas = { ...data };
        this._lineElement.startInputText = this.startText;
        this._lineElement.endInputText = this.endText;
        this._lineElement.content = this.content;
        this._lineElement.setRecoverable(!!this.datas.value.recover);
        this._updateSpans();
    }

    async initSegmentAsync(data: LineData): Promise<void> {
        if (!!data.wordSegments && data.wordSegments.length > 0) {
            this._segmentMode = true;
            this.inputSegments = data.wordSegments;
            this._segments = data.wordSegments;
            this._originalSegments = data.originalWordSegments ?? JSON.parse(JSON.stringify(data.wordSegments));

            const content = this._segments.map(o => o.word).join("");
            this.datas.value.init = content
            this.datas.value.saved = content
            this.datas.value.last = content
            this.datas.value.recover = this._originalSegments.map(o => o.word).join("");
        }
    }

    async applyAsync(data: LineData): Promise<{ success: boolean; edited: boolean; }> {
        let edited = false;
        let newStartText = data.start;
        let newEndText = data.end;
        if (Util.isValidTime(newStartText) && Util.isValidTime(newEndText)) {
            let newStartTime = Util.parseTime(newStartText);
            let newEndTime = Util.parseTime(newEndText);

            if (newEndTime < newStartTime) {
                console.error(`開始時間不可以晚於結束時間 (${this.startInputText} > ${this.endInputText})`);
                this._lineElement.startInputText = this.startText;
                this._lineElement.endInputText = this.endText;
                return { success: false, edited: false };
            }

            const prev = this.prev;
            const next = this.next;

            if (prev && newStartTime < prev.endTime) {
                newStartTime = prev.endTime;
            }

            if (next && newEndTime > next.startTime) {
                newEndTime = next.startTime;
            }

            this._updateSegmentByDuration(data.wordSegments ?? [], newStartTime, newEndTime);

            newStartText = Util.formatTime(newStartTime);
            newEndText = Util.formatTime(newEndTime);

            this.datas.start.last = newStartText;
            this._lineElement.startInputText = newStartText;
            this.datas.end.last = newEndText;
            this._lineElement.endInputText = newEndText;
            
            if (newStartText !== this.startText || newEndText !== this.endText) {
                edited = true;
            }

        } else {
            this._lineElement.startInputText = this.startText;
            this._lineElement.endInputText = this.endText;
        }
        
        this._segmentMode = data.wordSegments !== undefined && data.wordSegments.length > 0;

        if (this._segmentMode && this._applySegments(data.wordSegments ?? [], data.content) || !this._segmentMode && this._applyContent(data.content)) {
            edited = true;
        }

        await this.updateAsync();

        return { success: true, edited };
    }

    async finishAsync(options?: { blurTextarea: boolean }): Promise<{ success: boolean; edited: boolean; }> {
        const { success, edited } = await UpdateLineCommand.invokeAsync(this, { ...this.inputData });
        if (options?.blurTextarea !== false) {
            this.setEditable(false);
        }

        return { success, edited };
    }

    async cancelAsync(): Promise<void> {
        if (this.subtitle.editor.alwaysFinishLine) {
            await this.finishAsync();
        } else {
            this._lineElement.startInputText = this.startText;
            this._lineElement.endInputText = this.endText;
    
            if (this._segmentMode) {
                this.inputSegments = this.segments;
            } else {
                this._lineElement.content = this.content;
            }
            
            this.setEditable(false);
        }
    }

    async selectAsync(selected: boolean): Promise<void> {
        this._lineElement.setSelected(selected);
    }

    async saveAsync(): Promise<void> {
        this.datas.value.saved = this.content;
        this.datas.start.saved = this.startText;
        this.datas.end.saved = this.endText;
        await this.updateAsync();
    }

    async recoverAsync(): Promise<void> {
        if (this.originalContent !== undefined) {
            await UpdateLineCommand.invokeAsync(this, {
                start: this.startInputText,
                end: this.endInputText,
                content: this.originalContent,
                originalContent: this.originalContent,
                wordSegments: [...this._originalSegments],
                originalWordSegments: this._originalSegments
            });
        }
    }

    async setStartAsync(text: string): Promise<void>;
    async setStartAsync(time: number): Promise<void>;
    async setStartAsync(value: string | number): Promise<void> {
        if (!Util.isValidTime(value)) { return; }

        const text = typeof value === "number" ? Util.formatTime(value) : value;
        if (text === this.startText) { return; }

        const time = typeof value === "number" ? value : Util.parseTime(value);
        this._updateSegmentByDuration(this._segments, time, this.endTime);
        this.inputSegments = this._segments;

        this._lineElement.startInputText = text;
        this.datas.start.last = text;
        this._lineElement.startTimeInput.classList.remove("invalid");
        await this.updateAsync();
    }

    async setEndAsync(text: string): Promise<void>;
    async setEndAsync(time: number): Promise<void>;
    async setEndAsync(value: string | number): Promise<void> {
        if (!Util.isValidTime(value)) { return; }

        const text = typeof value === "number" ? Util.formatTime(value) : value;
        if (text === this.startText) { return; }
        
        const time = typeof value === "number" ? value : Util.parseTime(value);
        this._updateSegmentByDuration(this._segments, this.startTime, time);

        if (this.segmentMode) {
            this.inputSegments = this._segments;
        }
        
        this._lineElement.endInputText = text;
        this.datas.end.last = text;
        this._lineElement.endTimeInput.classList.remove("invalid");
        await this.updateAsync();
    }

    private _updateSegmentByDuration(segments: WordSegment[], start: number, end: number): void {
        const startDiff = start - this.startTime;
        
        segments.forEach(segment => {
            const segmentTime = Util.parseTime(segment.start) + startDiff;
            segment.start = Util.formatTime(segmentTime > end ? end : segmentTime);
        });
    }

    async updateAsync(): Promise<void> {
        this.edited =
            this.content !== this.datas.value.saved ||
            this.startText !== this.datas.start.saved ||
            this.endText !== this.datas.end.saved;
        
        if (this.edited) {
            this._lineElement.setEdited();
        } else if (this.saved) {
            this._lineElement.setSaved();
        } else {
            this._lineElement.setUnchanged();
        }
    }

    async deleteAsync(): Promise<void> {
        this.deleted = true;
        this._lineElement.element.remove();

        if (this.spans && this.spans.length > 0) {
            this.subtitle.transcript.spans.splice(this.spans[0].index, this.spans.length);
            this.spans.forEach(span => span.deleteSpan());
            this.spans = undefined;
        }
    }

    triggerTimestamp(): void {
        this.subtitle.editor.play(this.startTime);
    }

    private _initEvents(): void {
        this._lineElement.onTriggerInserter = async () => {
            InsertLineCommand.invokeAsync(this.subtitle, this.index);
        };

        this._lineElement.onTriggerFinish = async () => this.subtitle.editor.finishLineAsync();
        this._lineElement.onTriggerUpdate = async () => this.subtitle.editor.finishLineAsync({ blurTextarea: false });
        this._lineElement.onTriggerEdit = async () => this.subtitle.editor.editLineAsync(this);
        this._lineElement.onTriggerCancel = async () => this.subtitle.editor.cancelLineAsync();
        this._lineElement.onTriggerRecover = async () => this.recoverAsync();

        this._lineElement.onTriggerPlay = async () => this.subtitle.editor.play(this.startTime);
        this._lineElement.onTriggerPlayOnce = async () => this.subtitle.editor.playPeriod(this.startTime, this.endTime);

        this._lineElement.onTriggerTimestamp = async () => this.triggerTimestamp();
        this._lineElement.onTimestampChange = async () => {
            const { invalidStart, invalidEnd } = this._checkTimestamp();
            this._lineElement.setStartTimeInvalid(invalidStart);
            this._lineElement.setEndTimeInvalid(invalidEnd);
            this.subtitle.editor.finishLineAsync({ blurTextarea: false });
        };

        this._lineElement.onSelectChange = async () => {
            if (this._lineElement.selectbox.checked) {
                SelectLineCommand.invokeAsync(this);
            } else {
                this.selectAsync(false);
            }
        };
    }

    private _checkTimestamp(): { invalidStart: boolean; invalidEnd: boolean; } {
        let invalidStart = !Util.isValidTime(this.startInputText);
        let invalidEnd = !Util.isValidTime(this.endInputText);

        if (!invalidStart && !invalidEnd) {
            const startTime = this.startInputTime;
            const endTime = this.endInputTime;
            const prev = this.prev;
            const next = this.next;

            if (prev && startTime < prev.endTime) {
                invalidStart = true;
            }

            if (next && endTime > next.startTime) {
                invalidEnd = true;
            }

            if (startTime > endTime) {
                invalidStart = true;
                invalidEnd = true;
            }
        }

        return { invalidStart, invalidEnd };
    }

    private _applyContent(content: string): boolean {
        if (this.content === content) {
            return false;
        }
        
        this.content = content;

        if (this._lineElement.content !== content) {
            this._lineElement.content = content;
        }

        this._updateSpans();

        return true;
    }

    private _applySegments(segments: WordSegment[], content: string): boolean {
        let segmentIsChanged = segments.length !== this._segments.length;
        let contentIsChanged = content !== this.content;

        if (segments.length === this._segments.length) {
            for (let i = 0; i < segments.length; i++) {
                const adoptedSegment = segments[i];
                const segment = this._segments[i];
                if (adoptedSegment.start !== segment.start || adoptedSegment.word !== segment.word) {
                    segmentIsChanged = true;
                    break;
                }
            }
        }

        if (!segmentIsChanged && !contentIsChanged) {
            return false;
        }

        if (segments.map(o => o.word).join("") !== content) {
            let charIndex = content.length - 1;
            for (let i = segments.length - 1; i >= 0; i--) {
                const segmentText = segments[i].word;
                const newSegmentChars: string[] = [];
                for (let segmentTextIndex = segmentText.length - 1; segmentTextIndex >= 0; segmentTextIndex--) {
                    for (let contentIndex = charIndex; contentIndex >= 0; contentIndex--) {
                        const char = content[charIndex];
                        newSegmentChars.push(char);
                        charIndex--;

                        if (segmentText[segmentTextIndex] === char) {
                            break;
                        }
                    }
                }

                segments[i].word = newSegmentChars.reverse().join("");
            }

            if (charIndex >= 0) {
                const additionChars: string[] = [];
                for (let contentIndex = charIndex; contentIndex >= 0; contentIndex--) {
                    additionChars.push(content[contentIndex]);
                }

                segments[0].word = `${additionChars.join("")}${segments[0].word}`;
            }
        }
        
        this._segments = [...segments];
        this.inputSegments = [...segments];
        this.content = content;
        this._updateSpans();
        return true;
    }

    private _updateSpans(): void {
        const transcript = this.subtitle.transcript;
        if (!transcript || !transcript.containsSpan) {
            return;
        }
        
        const content = this.content;
        if (this.spans) {
            const breakLine = this.spans[this.spans.length - 1].endOfLine === true;
            const lastSpanIndex = this.spans[this.spans.length - 1].index + 1;
            const nextSpan = lastSpanIndex < transcript.spans.length ? transcript.spans[lastSpanIndex] : undefined;
            
            transcript.spans.splice(this.spans[0].index, this.spans.length);
            this.spans.forEach(span => span.deleteSpan());
            this.spans = undefined;

            const index = nextSpan ? nextSpan.index : transcript.spans.length;
            this.spans = this._buildTranscriptSpans(transcript, breakLine && content[content.length - 1] !== "\n" ? `${content}\n` : content, index);
        } else if (content && this.index >= 0) {
            let index = 0;
            const prevLines = this.prevLines;
            
            if (prevLines.length > 0) {
                for (let i = prevLines.length - 1; i >= 0; i--) {
                    const prevLine = prevLines[i];
                    if (prevLine.spans) {
                        index = prevLine.spans[prevLine.spans.length - 1].index + 1;
                        break;
                    }
                }
            }

            this.spans = this._buildTranscriptSpans(transcript, content, index);
        }
    }

    private _buildTranscriptSpans(transcript: Transcript, content: string, index: number): TranscriptSpan[] {
        const spans = transcript.insertSpan(index, content);
        spans.forEach(span => {
            span.line = this;
            span.setBinded(true);
            span.setPlaying(this.playing);
        });

        if (spans.length > 0) {
            spans[0].setBindedFirst(true);
            const notEndOfLineSpans = spans.filter(span => !span.endOfLine);
            if (notEndOfLineSpans.length > 0) {
                notEndOfLineSpans[notEndOfLineSpans.length - 1].setBindedLast(true);
            }
        }

        return spans;
    }
}
