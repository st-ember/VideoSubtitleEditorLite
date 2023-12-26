import icons from "../../../icons";
import Util from "../../Util";
import Editor from "../Editor";
import { WordSegment } from "../models";
import { LineOptions } from "../options";

/** 包含實際 Span 元件及資料的區塊物件 */
interface SegmentElement {
    /** 網頁上實際的 Span 元件 */
    span: HTMLSpanElement;
    /** 區塊資料 */
    segment: WordSegment;
}

/** 字幕元件，負責直接控制每一行字幕在網頁上的元件行為。 */
export default class LineElement {

    get element(): HTMLDivElement { return this._element; }
    get selectbox(): HTMLInputElement { return this._selectbox; }
    get startTimeInput(): HTMLSpanElement { return this._startTimeInput; }
    get endTimeInput(): HTMLSpanElement { return this._endTimeInput; }

    onTriggerInserter: () => Promise<void> = async () => { };
    onTriggerTimestamp: () => Promise<void> = async () => { };
    onTriggerFinish: () => Promise<void> = async () => { };
    onTriggerUpdate: () => Promise<void> = async () => { };
    onTriggerEdit: () => Promise<void> = async () => { };
    onTriggerRecover: () => Promise<void> = async () => { };
    onTriggerCancel: () => Promise<void> = async () => { };
    onTriggerPlay: () => Promise<void> = async () => { };
    onTriggerPlayOnce: () => Promise<void> = async () => { };
    onTimestampChange: () => Promise<void> = async () => { };
    onSelectChange: () => Promise<void> = async () => { };

    get startInputText(): string { return this._getTimeInputValue(this._startTimeInput); }
    set startInputText(value: string) { this._setTimeInput(this._startTimeInput, value, true); }
    get endInputText(): string { return this._getTimeInputValue(this._endTimeInput); }
    set endInputText(value: string) { this._setTimeInput(this._endTimeInput, value, false); }
    get content(): string { return this._getTextAreaValue(); }
    set content(value: string) { this._setTextAreaValue(value); }
    get segments(): WordSegment[] { return this._getSegments(); }
    set segments(value: WordSegment[]) { this._setTextAreaSegments(value); }
    get caretPosition(): number | undefined { return this._caretPosition; }
    get caretAtFirstPosition(): boolean { return this._caretAtFirstPosition; }
    get caretAtLastPosition(): boolean { return this._caretAtLastPostion; }
    get editingTime(): boolean { return this._editingStartTime || this._editingEndTime; }

    private _element: HTMLDivElement = document.createElement("div");
    private _actionGroup: HTMLDivElement = document.createElement("div");
    private _selectbox: HTMLInputElement = document.createElement("input");
    private _timestamp: HTMLDivElement = document.createElement("div");
    private _timestampActionGroup: HTMLDivElement = document.createElement("div");
    private _startTimeInput: HTMLSpanElement = document.createElement("span");
    private _endTimeInput: HTMLSpanElement = document.createElement("span");
    private _textarea: HTMLDivElement = document.createElement("div");
    private _finishButton?: HTMLButtonElement;
    private _editButton?: HTMLButtonElement;
    private _recoverButton?: HTMLButtonElement;
    private _playButton?: HTMLButtonElement;
    private _playOnceButton?: HTMLButtonElement;
    private _inserter: HTMLDivElement = document.createElement("div");

    private _options: LineOptions;
    private _editor: Editor;
    
    /** 整個字幕元件是否處於編輯狀態。 */
    private _editable: boolean = false;
    /** 是否正在編輯起始時間。 */
    private _editingStartTime: boolean = false;
    /** 是否正在編輯結束時間。 */
    private _editingEndTime: boolean = false;
    /** 使用者是否將焦點放置於起始時間輸入框中。 */
    private _focusOnStartTime: boolean = false;
    /** 使用者是否將焦點放置於結束時間輸入框中。 */
    private _focusOnEndTime: boolean = false;
    /** 使用者是否將焦點放置於文字輸入框中。 */
    private _focusTextArea: boolean = false;
    /** 是否正在處理文字輸入框的更新事件。當此值為 true 時不可再度觸發文字更新，以避免發生無限迴圈。 */
    private _textAreaUpdating: boolean = false;
    /** 每秒畫格數。此值為 undefined 時，時間編輯框小於一秒的部分會以百分之一秒來呈現；反之將換算成最接近的影格編號。 */
    private _frameRate: number | undefined;

    /** 是否啟用"區塊模式"，此模式下文字是由區塊組成，每個區塊都會有獨立的時間。 */
    private _segmentMode: boolean = false;
    /** 每一個區塊的內容，包含實際的 Span 元件和存放時間資料的 WordSegment 物件。 */
    private _segmentElements: SegmentElement[] = [];

    /** 正在編輯此字幕時，輸入指標圈選的區域物件。 */
    private _currentSelection?: Selection = undefined;
    /** 正在編輯此字幕時，輸入指標觸發的位置。如果在區塊模式，代表觸發的區塊 index；如果是文字模式則代表指標在文字的位置。 */
    private _caretPosition?: number = undefined;
    /** 正在編輯此字幕時，輸入指標圈選的起始位置。如果在區塊模式，代表觸發的區塊 index；如果是文字模式則代表指標在文字的位置。 */
    private _caretStartPosition: number = 0;
    /** 正在編輯此字幕時，輸入指標圈選的結束位置。如果在區塊模式，代表觸發的區塊 index，且會和 _caretPosition 相同；如果是文字模式則代表指標在文字的位置。 */
    private _caretEndPosition: number = 0;
    /** 正在編輯此字幕且處於區塊模式時，輸入指標在已觸發區塊內的圈選起始位置。 */
    private _caretStartPositionInSpan: number = 0;
    /** 正在編輯此字幕且處於區塊模式時，輸入指標在已觸發區塊內的圈選結束位置。 */
    private _caretEndPositionInSpan: number = 0;
    /** 正在編輯此字幕時，輸入指標是否放在整句字幕的最前面。 */
    private _caretAtFirstPosition: boolean = false;
    /** 正在編輯此字幕時，輸入指標是否放在整句字幕的最後面。 */
    private _caretAtLastPostion: boolean = false;

    constructor(options: LineOptions, editor: Editor) {
        this._options = { ...options };
        this._editor = editor;
        this._frameRate = editor.options.topic?.frameRate;
        this._build();
    }

    /**
     * 更新字幕元件的播放狀態，這會讓元件的底色有一些變化，提示使用者此字幕正在被播放/已不再播放。
     * @param playing 是否正在播放。
     */
    setPlaying(playing: boolean): void {
        this._element.classList[playing ? "add" : "remove"]("playing");
    }

    /**
     * 更新字幕元件的編輯狀態。編輯狀態中的元件可讓時間及文字輸入框呈現可編輯狀態。
     * @param editable 是否進入編輯狀態
     */
    setEditable(editable: boolean): void {
        // 如果和現有狀態相同就不用費心再跑一次
        if (this._editable === editable) {
            return;
        }

        this._editable = editable;
        if (editable) {
            // 編輯時，起始時間和結束時間輸入框可被使用者直接修改。
            this._startTimeInput.contentEditable = "true";
            this._endTimeInput.contentEditable = "true";
            this._textarea.contentEditable = "true";

            // 加上 Class 讓樣式改變以提示使用者目前的狀態已變更為編輯中。
            this._element.classList.add("editing");

            // 如果啟用區塊模式，將每一個區塊都設定為可編輯。
            if (this._segmentMode) {
                this._segmentElements.forEach(o => o.span.contentEditable = "true");
            }

            // 如果使用者不是透過點選時間輸入框來進入編輯狀態，需要將使用者焦點放置於文字輸入框。
            if (!this._focusOnStartTime && !this._focusOnEndTime) {
                this._textarea.focus();
            }
        } else {
            this._startTimeInput.contentEditable = "false";
            this._endTimeInput.contentEditable = "false";
            this._textarea.contentEditable = "false";

            this._element.classList.remove("editing");

            if (this._segmentMode) {
                this._segmentElements.forEach(o => o.span.contentEditable = "false");
            }

            // 離開編輯狀態時，所有的輸入框都不該繼續成為焦點。
            this._startTimeInput.blur();
            this._endTimeInput.blur();
            this._textarea.blur();

            // 起始與結束時間有可能會呈現紅色的無效狀態，離開編輯狀態時需要隱藏這樣的效果。
            this._startTimeInput.classList.remove("invalid");
            this._endTimeInput.classList.remove("invalid");

            // 將區塊的觸發效果取消，並清除輸入指標的相關資訊。
            this._clearFocusSegmentSpan();
        }
    }

    /** 在編輯狀態下，將輸入指標移動到文字輸入框的最前面。 */
    setCaretToStart(): void {
        if (!this._editable) {
            return;
        }

        if (this._currentSelection) {
            const range = document.createRange();
            const node = this._textarea.childNodes[0];

            if (this._segmentMode) {
                range.setStart(node.childNodes[0], 0);
                range.setEnd(node.childNodes[0], 0);
                
            } else {
                range.setStart(node, 0);
                range.setEnd(node, 0);
            }

            this._currentSelection.removeAllRanges();
            this._currentSelection.addRange(range);
        }
    }

    /** 在編輯狀態下，將輸入指標移動到文字輸入框的最後面。 */
    setCaretToEnd(): void {
        if (!this._editable) {
            return;
        }

        if (this._currentSelection) {
            const range = document.createRange();
            if (this._segmentMode) {
                const node = this._textarea.childNodes[this._textarea.childNodes.length - 1];
                const content = node.textContent ?? "";
                range.setStart(node.childNodes[0], content.length);
                range.setEnd(node.childNodes[0], content.length);
            } else {
                const node = this._textarea.childNodes[0];
                const content = node.textContent ?? "";
                range.setStart(node, content.length);
                range.setEnd(node, content.length);
            }

            this._currentSelection.removeAllRanges();
            this._currentSelection.addRange(range);
        }
    }

    setEdited(): void {
        this._element.classList.add("edited");
        this._element.classList.remove("saved");
    }

    setSaved(): void {
        this._element.classList.add("saved");
        this._element.classList.remove("edited");
    }

    setUnchanged(): void {
        this._element.classList.remove("saved");
        this._element.classList.remove("edited");
    }

    setRecoverable(recoverable: boolean): void {
        if (!this._recoverButton) { return; }
        this._recoverButton.style.display = recoverable ? "" : "none";
    }

    setStartTimeInvalid(invalid: boolean): void {
        if (invalid) {
            this._startTimeInput.classList.add("invalid");
        } else {
            this._startTimeInput.classList.remove("invalid");
        }
    }

    setEndTimeInvalid(invalid: boolean): void {
        if (invalid) {
            this._endTimeInput.classList.add("invalid");
        } else {
            this._endTimeInput.classList.remove("invalid");
        }
    }

    setInserting(inserting: boolean): void {
        if (inserting) {
            this._element.classList.add("inserting");
        } else {
            this._element.classList.remove("inserting");
        }
    }

    setDeletingAsync(animationLength: number): Promise<void> {
        this._element.style.height = `${this._element.clientHeight}px`;
        this._element.classList.add("deleting");
        return new Promise<void>(resolve => {
            setTimeout(() => {
                this._element.style.height = "0";
                setTimeout(() => resolve(), animationLength);
            }, 10);
        });
    }

    setHighlight(highlight: boolean): void {
        if (highlight) {
            this._element.classList.add("highlight");
        } else {
            this._element.classList.remove("highlight");
        }
    }

    setSelected(selected: boolean): void {
        if (selected) {
            this._selectbox.checked = true;
            this._element.classList.add("selected");
        } else {
            this._selectbox.checked = false;
            this._element.classList.remove("selected");
        }
    }

    /**
     * 在文字輸入框中尋找指定的文字是否出現。
     * @param text 要尋找的文字
     * @returns 指定的文字是否出現
     */
    searchText(text: string): boolean {
        this.clearSearch();

        const content = this.content;
        const index = content.indexOf(text);
        if (index < 0) { return false; }

        let temp = "";
        let indexs: number[] = [];
        let fragments: string[] = [];

        for (let i = 0; i < content.length; i++) {
            const char = content[i];
            temp = `${temp}${char}`;

            if (temp.indexOf(text) >= 0) {
                fragments.push(temp.substring(0, temp.length - text.length));
                temp = "";
                indexs.push(i - (text.length - 1));
            } else if (i === content.length - 1) {
                fragments.push(temp);
            }
        }

        if (!this._segmentMode) {
            if (fragments.length > 1) {
                this._textarea.innerHTML = fragments.join(`<span class="se-search-result">${text}</span>`);
            } else {
                this._textarea.innerHTML = content.indexOf(text) === 0 ? `<span class="se-search-result">${text}</span>${fragments[0]}` : `${fragments[0]}<span class="se-search-result">${text}</span>`;
            }
        } else {
            let textIndex = 0;
            let inResultMode = false;
            let resultLength = 0;
            for (let i = 0; i < this._segmentElements.length; i++) {
                const elem = this._segmentElements[i];
                const array: string[] = [];

                for (let j = 0; j < elem.segment.word.length; j++) {
                    if (indexs.indexOf(textIndex) >= 0) {
                        inResultMode = true;
                    }

                    if (inResultMode) {
                        array.push(`<span class="se-search-result">${elem.segment.word[j]}</span>`);
                        resultLength++;
                    } else {
                        array.push(elem.segment.word[j]);
                    }

                    if (resultLength === text.length) {
                        inResultMode = false;
                        resultLength = 0;
                    }

                    textIndex++;
                }

                elem.span.innerHTML = array.join("");
            }
        }

        return true;
    }

    /**
     * 設定是否顯示搜尋結果的文字效果。
     * @param highlight 是否顯示
     */
    setSearchHighlight(highlight: boolean): void {
        this._textarea.classList[highlight ? "add" : "remove"]("se-search-focus");
    }

    /** 清除所有被標註為搜尋結果的文字效果。 */
    clearSearch(): void {
        if (!this._segmentMode) {
            this._textarea.innerHTML = this._textarea.innerText;
        } else {
            // 區塊模式下，只能透過重設 Span 元件的內文來做到。
            for (let i = 0; i < this._segmentElements.length; i++) {
                const elem = this._segmentElements[i];
                elem.span.innerText = elem.segment.word;
            }
        }
    }

    /**
     * 將文字輸入框內的特定文字取代成新輸入的文字。
     * @param target 要被取代的文字
     * @param text 取代後的新文字字串
     */
    replaceText(target: string, text: string): void {
        this.clearSearch();

        const content = this.content;
        const index = content.indexOf(target);
        if (index < 0) { return; }

        // 首先要先找出 target 位在完整文字的那些位置，因為 target 可能會重複多次，所以使用 number[] 來儲存所有符合的位置。
        // 檢測方法：依序走過每個字，把每個字逐一放到 temp 內，每次放文字進 temp 就檢查 temp 的字尾是否符合 text。
        // 一旦 temp 字尾符合 text，就可以算出這次的 index，將該 index 放入 indexs 內，再將 temp 放入 fragments 內，接著把 temp 歸零並繼續迴圈直到所有文字走完。

        let temp = "";
        let indexs: number[] = []; // 字串中所有 target 第一個字的 index
        let fragments: string[] = []; // 字串中所有被 target 分割後的字串。

        for (let i = 0; i < content.length; i++) {
            const char = content[i];
            temp = `${temp}${char}`;

            if (temp.indexOf(target) >= 0) {
                fragments.push(temp.substring(0, temp.length - target.length));
                temp = "";
                indexs.push(i - (target.length - 1));
            } else if (i === content.length - 1) {
                fragments.push(temp);
            }
        }

        // 再來要使用找出的 indexs 和 fragments 來實際取代文字。

        if (!this._segmentMode) {
            // 在沒有 segment 的狀況下，單純將 fragments 用 text 合併成新的句子就能完成取代。
            this.content = fragments.length > 1 ? fragments.join(text) :
                content.indexOf(text) === 0 ? `${text}${fragments[0]}` : `${fragments[0]}${text}`;
        } else {
            // 在有 segment 的狀況就會複雜很多。
            // 具體作法：逐一走過每個 segment 內的每個字，每走一個字就將 targetIndex 加 1，並把那個字加入 segment 的 array 中，直到碰到 indexs 內的數字。
            // 如果 segment 內沒有文字碰到 indexs，他的 array 會和原本的 word 一模一樣。
            // 如果碰到 indexs，inResultMode 需要設定為 true，接下來每次往前走一個字時，就將 text 對應位置的文字放到 array 中，這樣一來該 segment 內的 word 就可以依序被取代。
            // 等到 text 用完，inResultMode 設定回 false，就會繼續走下去直到 segment 用光或是又碰到 indexs 內的數字。

            let targetIndex = 0; // 目前走到的字數
            let textIndex = 0; // 當碰到 indexs 內的數字後，目前在 text 內要取代的文字 index。
            let inResultMode = false; // 是否已碰到 indexs 內的數字且正在進行取代。
            let resultLength = 0; // 當碰到 indexs 後，每走一個字就會加 1，用來確認 target 的字數是否還夠用。一旦 target 的字數用光就會離開 isResultMode
            const newSegments: WordSegment[] = [];
            for (let i = 0; i < this._segmentElements.length; i++) {
                const word = this._segmentElements[i].segment.word;
                const array: string[] = [];

                for (let charIndex = 0; charIndex < word.length; charIndex++) {
                    if (indexs.indexOf(targetIndex) >= 0) {
                        inResultMode = true;
                    }

                    if (inResultMode) {
                        if (textIndex < text.length) {
                            array.push(text[textIndex]);
                            textIndex++;
                        }

                        if (textIndex === target.length && text.length > target.length) {
                            // 如果要被取代的字串長度小於新字串的長度，多出來的字通通都要被放到要被取代的最後一個 segment 內。
                            array.push(text.substring(textIndex));
                            textIndex += text.length - target.length;
                        }

                        resultLength++;
                    } else {
                        array.push(word[charIndex]);
                    }

                    if (resultLength >= target.length) {
                        inResultMode = false;
                        resultLength = 0;
                        textIndex = 0;
                    }

                    targetIndex++;
                }

                newSegments.push({
                    start: this._segmentElements[i].segment.start,
                    word: array.join("")
                });
            }

            this.segments = newSegments.filter(o => o.word && o.word.length > 0);
        }
    }

    /** 建立此元件內所有的網頁元件，此方法應只被執行一次。 */
    private _build(): void {
        this._element.className = "se-line";

        if (this._options.insertManually) {
            // 如果此字幕是透過人工插入的方式建立，先加上 inserting 來賦予一些特效。
            this._element.classList.add("inserting");
        }

        this._inserter.className = "se-inserter";
        this._inserter.title = "在這裡插入一行新的字幕";
        this._element.appendChild(this._inserter);

        const checkboxGroup = document.createElement("div");
        checkboxGroup.className = "se-checkbox-group";
        this._element.appendChild(checkboxGroup);

        this._selectbox.type = "checkbox";
        this._selectbox.title = "勾選此項目，按住 Ctrl 以多選，按住 Shift 以連續多選。";
        checkboxGroup.appendChild(this._selectbox);

        const group = document.createElement("div");
        group.setAttribute("role", "group");
        this._element.appendChild(group);

        this._actionGroup.className = "se-actions";
        this._element.appendChild(this._actionGroup);

        this._timestamp.className = "se-timestamp";
        group.appendChild(this._timestamp);

        this._timestampActionGroup.className = "se-timestamp-action-group";
        group.appendChild(this._timestampActionGroup);

        const content = document.createElement("div");
        content.className = "se-line-content";
        group.appendChild(content);

        this._textarea.className = "se-line-textarea";
        this._textarea.contentEditable = "false";
        content.appendChild(this._textarea);

        this._buildButtons();
        this._buildTimestampInputs();
        this._buildEvents();
    }

    private _buildButtons(): void {
        this._finishButton = LineElement._buildButton("finish", "success", icons.Finish, "完成", () => this.onTriggerFinish());
        this._editButton = LineElement._buildButton("edit", "primary", icons.Edit, "編輯", () => this.onTriggerEdit());
        this._recoverButton = LineElement._buildButton("recover", "dark primary-hover", icons.Reset, "還原至原始", () => this.onTriggerRecover());
        this._playButton = LineElement._buildButton("play", "dark primary-hover", icons.Play, "從這句開始播放", () => this.onTriggerPlay());
        this._playOnceButton = LineElement._buildButton("play-once", "dark primary-hover", icons.PlayOnce, "播放一次這句", () => this.onTriggerPlayOnce());
        this._actionGroup.appendChild(this._finishButton);
        this._actionGroup.appendChild(this._editButton);
        this._actionGroup.appendChild(this._recoverButton);

        this._timestampActionGroup.appendChild(this._playButton);
        this._timestampActionGroup.appendChild(this._playOnceButton);
    }

    private static _buildButton(name: string, buttonClass: string, html: string, title: string, onclick: () => Promise<void>): HTMLButtonElement {
        const button = document.createElement("button");
        button.className = `small-button icon-button ${buttonClass} se-${name}-button`;
        button.type = "button";
        button.title = title;
        button.innerHTML = html;
        button.addEventListener("click", () => {
            button.blur();
            onclick();
        });
        return button;
    }

    /** 建立起始和結束時間的輸入框。 */
    private _buildTimestampInputs(): void {
        this._startTimeInput.contentEditable = "false";
        this._startTimeInput.className = "se-start-time-input";
        this._timestamp.appendChild(this._startTimeInput);

        this._endTimeInput.contentEditable = "false";
        this._endTimeInput.className = "se-end-time-input";
        this._timestamp.appendChild(this._endTimeInput);

        const dash = document.createElement("span");
        dash.innerText = "-";
        this._timestamp.insertBefore(dash, this._endTimeInput);

        // 建立尚未包含資料的時間輸入框。
        this._setTimeInput(this._startTimeInput, Util.DEFAULT_TIME, true);
        this._setTimeInput(this._endTimeInput, Util.DEFAULT_TIME, false);
    }

    /**
     * 更新指定的時間輸入框。
     * @param element 要更新的時間輸入框元件。
     * @param text 要更新的新文字，此文字必須是 0:00:00.000 這樣的格式。
     * @param isStartTime 此輸入框是否是起始時間輸入框，這會影響到事件的 binding。
     */
    private _setTimeInput(element: HTMLSpanElement, text: string, isStartTime: boolean): void {
        // 刪掉輸入框元件內所有既有子元件。
        while (element.lastChild) {
            element.removeChild(element.lastChild);
        }

        // 拆解輸入的文字成各時間單位的數字，在這邊輸入的 text 一定是 0:00:00.000 這樣的正確格式。
        const textArray = text.split(':');
        const hour = Number(textArray[0]);
        const minute = Number(textArray[1]);
        const secondArray = textArray[2].split('.');
        const second = Number(secondArray[0]);
        const miniSecond = Number(secondArray[1]);

        // 針對每一個時間單位建立對應的 Span 元件。
        let modified = false;
        const hourTextSpan = this._buildTimeDigitalSpan(hour, 1, false, () => modified = true);
        const minuteTextSpan = this._buildTimeDigitalSpan(minute, 2, false, () => modified = true);
        const secondTextSpan = this._buildTimeDigitalSpan(second, 2, false, () => modified = true);
        const miniSecondTextSpan = this._buildTimeDigitalSpan(miniSecond, 3, !!this._frameRate, () => modified = true);

        /** 清除所有變更監視器。 */
        const clearObserver = () => {
            hourTextSpan.observer.disconnect();
            minuteTextSpan.observer.disconnect();
            secondTextSpan.observer.disconnect();
            miniSecondTextSpan.observer.disconnect();
        };

        /** 重建目前這個時間輸入框。 */
        const reset = () => {
            const newText = element.innerText;
            element.removeEventListener("mouseleave", onMouseLeave);
            clearObserver();

            let adoptedNewText = newText;
            if (this._frameRate) {
                const secondArray = newText.split(",");
                if (secondArray.length === 2) {
                    const miniSecond = Math.ceil(Number(secondArray[1]) * 1000 / this._frameRate!);
                    secondArray[1] = miniSecond < 10 ? `00${miniSecond}` : miniSecond < 100 ? `0${miniSecond}` : String(miniSecond);
                    adoptedNewText = secondArray.join(".");
                }
            }

            if (adoptedNewText !== text) {
                this.onTimestampChange();
            }

            // 用新的值來重建。
            this._setTimeInput(element, Util.isValidTime(adoptedNewText) ? adoptedNewText : text, isStartTime);
        };

        /** 滑鼠離開輸入框時的行為，如果文字有被改變則重建整個輸入框。 */
        const onMouseLeave = () => {
            if (modified) {
                reset();
            }
        };

        element.addEventListener("mouseleave", onMouseLeave);

        // 如果使用者將焦點放在此輸入框，將字幕設定為正在編輯時間，只會執行一次。
        element.addEventListener("focus", () => {
            this[isStartTime ? "_editingStartTime" : "_editingEndTime"] = true;
        }, { once: true });

        // 如果使用者將焦點移出此輸入框，將字幕設定為不再編輯時間，只會執行一次。一旦被觸發則重建輸入框。
        element.addEventListener("blur", () => {
            this[isStartTime ? "_editingStartTime" : "_editingEndTime"] = false;
            reset();
        }, { once: true });

        // 組織畫面上要顯示的網頁元件。
        const hourColon = document.createElement("span");
        hourColon.innerText = ":";
        const minuteColon = document.createElement("span");
        minuteColon.innerText = ":";
        const miniSecondPoint = document.createElement("span");
        miniSecondPoint.innerText = !this._frameRate ? "." : ",";

        element.appendChild(hourTextSpan.element);
        element.appendChild(hourColon);
        element.appendChild(minuteTextSpan.element);
        element.appendChild(minuteColon);
        element.appendChild(secondTextSpan.element);
        element.appendChild(miniSecondPoint);
        element.appendChild(miniSecondTextSpan.element);
    }

    /**
     * 建立可編輯與顯示時間單位數字的元件。
     * @param value 此元件目前的數字。
     * @param width 此元件要顯示的位數，不足的位數會補零。
     * @param onChange 當此元件被使用者變更後要觸發的方法。
     * @returns 元件以及變更觀察器。
     */
    private _buildTimeDigitalSpan(value: number, width: number, isFrameRate: boolean, onChange: () => void): { element: HTMLSpanElement, observer: MutationObserver } {
        // 首先要先確保輸入的數字是有效的，如果無效的話先歸零。
        let currentValue = !isNaN(value) ? 
            (!isFrameRate ? value : Math.floor(this._frameRate! * value / 1000)) : 
            0;

        // 根據顯示的位數來設定數字的最大值。
        const max = !isFrameRate ?
            (width === 1 ? 9 : width === 2 ? 59 : 999) :
            (this._frameRate! - 1);

        const digitalLength = !isFrameRate ? width : String(this._frameRate!).length;

        const span = document.createElement("span");
        span.className = "digital-span";
        span.title = String(value);
        
        /** 取得一個必定在最大值內且有效的值。 */
        const getAdoptedValue = () => currentValue < 0 ? max : currentValue > max ? 0 : currentValue;

        /**
         * 將數字型別的值依照顯示位數轉換成文字。
         * @param value 數字型別的值
         * @returns 轉換後的文字
         */
        const getText = (value: number) => {
            const valueTextLength = String(value).length;
            const array: string[] = [];
            for (let i = 0; i < digitalLength - valueTextLength; i++) {
                array.push("0");
            }

            array.push(String(value));
            return array.join("");
        };

        /** 當使用者將滑鼠放在此元件上並滾動滑鼠滾輪時的行為。 */
        const onScroll = (event: WheelEvent) => {
            // 如果此字幕並未處於編輯模式，則忽略事件。
            if (!this._editable) {
                return;
            }

            // 禁止事件繼續進行，避免畫面被捲動。
            event.preventDefault();

            // 依照滾輪方向改變值。
            if (event.deltaY > 0) {
                currentValue--;
            } else {
                currentValue++;
            }

            // 確保值有效，然後修改元件的文字成新的值。
            currentValue = getAdoptedValue();
            span.innerText = getText(currentValue);

            // 觸發變更事件。
            onChange();
        };
        
        // 建立元件的變更觀察器，如果元件被使用者直接修改 (改數字、剪下和貼上等任何修改) 就會觸發變更事件。
        const observer = new MutationObserver(() => onChange());

        // 將元件的文字改成目前的值。
        currentValue = getAdoptedValue();
        span.innerText = getText(currentValue);
        
        // 當滑鼠進到此元件的範圍內時，就要讓變更觀察器開始監視元件是否被變更，並且開始監試滾輪。
        span.addEventListener("mouseenter", () => {
            observer.observe(span, { characterData: true, subtree: true, childList: true });
            span.classList.add("hover");
            window.addEventListener("wheel", onScroll, { passive: false }); // 針對 Safari 加上 passive，讓 preventDefault 可以正常運作。
        });

        // 當滑鼠離開此元件的範圍時，停止監視滾輪。就算滑鼠離開，仍要持續監視元件是否被變更，停止監視的行為由上一層來控制。
        span.addEventListener("mouseleave", () => {
            span.classList.remove("hover");
            window.removeEventListener("wheel", onScroll);
        });

        return { element: span, observer };
    }

    private _buildEvents(): void {
        this._updateFocusSegmentSpan = this._updateFocusSegmentSpan.bind(this);

        this._inserter.addEventListener("click", () => this.onTriggerInserter());
        this._textarea.addEventListener("click", async () => {
            if (!this._editable) {
                await this.onTriggerEdit();
            }

            this._updateFocusSegmentSpan();
        });
        this._textarea.addEventListener("keyup", this._updateFocusSegmentSpan);

        this._startTimeInput.addEventListener("blur", () => this._focusOnStartTime = false);
        this._endTimeInput.addEventListener("blur", () => this._focusOnEndTime = false);

        this._startTimeInput.addEventListener("click", async () => {
            if (!this._editable) {
                this._focusOnStartTime = true;
                await this.onTriggerEdit();
                this._startTimeInput.focus();
            }
        });

        this._endTimeInput.addEventListener("click", async () => {
            if (!this._editable) {
                this._focusOnEndTime = true;
                await this.onTriggerEdit();
                this._endTimeInput.focus();
            }
        });

        const observer = new MutationObserver(() => {
            if (!this._textAreaUpdating) {
                this.onTriggerUpdate();
            }
        });

        this._textarea.addEventListener("focus", () => {
            this._focusTextArea = true;
            this._updateFocusSegmentSpan();
            observer.observe(this._textarea, { characterData: true, subtree: true, childList: true });
        });
        this._textarea.addEventListener("blur", () => {
            this._focusTextArea = false;
            this._clearFocusSegmentSpan();
            observer.disconnect();
        });

        this._textarea.addEventListener("compositionstart", () => {
            if (this._focusTextArea) {
                observer.disconnect();
            }
        });
        this._textarea.addEventListener("compositionend", () => {
            if (this._focusTextArea) {
                this.onTriggerUpdate();
                observer.observe(this._textarea, { characterData: true, subtree: true, childList: true });
            }
        });

        this._selectbox.addEventListener("change", () => {
            this.onSelectChange();
        });
    }

    /** 更新目前輸入指標所在的位置。 */
    private _updateFocusSegmentSpan(): void {
        const selection = window.getSelection();
        if (this._segmentMode) {
            // 如果是區塊模式，輸入指標的位置是被觸發的區塊位置，和預設的 Selection 物件所提供的資料不同。
            if (selection && selection.focusNode && selection.focusNode.parentNode instanceof HTMLSpanElement) {
                this._currentSelection = selection;
                this._caretAtFirstPosition = false;
                this._caretAtLastPostion = false;
    
                // 找到目前輸入指標所在的區塊 Span 元件，並將該元件加上觸發的效果。
                const span = selection.focusNode.parentNode;
                span.classList.add("focus");
    
                this._segmentElements.forEach((elem, index) => {
                    // 依序比對所有區塊的 Span 元件來找出目前被觸發的是哪個。
                    if (elem.span === selection.anchorNode?.parentNode) {
                        this._caretStartPosition = index;
                        this._caretStartPositionInSpan = selection.anchorOffset;
                    }

                    if (elem.span !== span) {
                        elem.span.classList.remove("focus");
                    } else {
                        this._caretPosition = index;
                        this._caretEndPosition = index;
                        this._caretEndPositionInSpan = selection.focusOffset;
    
                        // 計算目前被觸發的區塊是否是最後一個，且輸入指標放在該區塊的最後一個字後。
                        if (index === this._segmentElements.length - 1 && selection.anchorOffset === elem.segment.word.length) {
                            this._caretAtLastPostion = true;
                        }
    
                        // 計算目前被觸發的區塊是否是第一個，且輸入指標放在該區塊的第一個字前。
                        if (index === 0 && selection.anchorOffset === 0) {
                            this._caretAtFirstPosition = true;
                        }
                    }
                });

                // 如果區塊內的起始和結束數字不同，則代表使用者一定選擇了多個文字，在這樣的狀況下輸入指標不會位於第一或最後位。
                if (this._caretStartPositionInSpan !== this._caretEndPositionInSpan) {
                    this._caretAtLastPostion = false;
                    this._caretAtFirstPosition = false;
                }
                
                // 如果區塊內的起始大於結束的數字，代表使用者不只選擇了多個文字，這些文字還橫跨多個區塊。這時需要糾正起始和結束相反的情形。
                if (this._caretStartPositionInSpan > this._caretEndPositionInSpan) {
                    const start = this._caretStartPositionInSpan;
                    const end = this._caretEndPositionInSpan;
                    this._caretStartPositionInSpan = end;
                    this._caretEndPositionInSpan = start;
                }

                // 如果被觸發的起始位置大於結束位置，表示使用者是由右到左來選擇文字，這時需要糾正相反的情形。
                if (this._caretStartPosition > this._caretEndPosition) {
                    const start = this._caretStartPosition;
                    const end = this._caretEndPosition;
                    this._caretStartPosition = end;
                    this._caretEndPosition = start;
                }

                // console.log(`_caretStartPosition = ${this._caretStartPosition}`)
                // console.log(`_caretEndPosition = ${this._caretEndPosition}`)
                // console.log(`_caretPosition = ${this._caretPosition}`)
                // console.log(`_caretAtLastPostion = ${this._caretAtLastPostion}`)
                // console.log(`_caretAtFirstPosition = ${this._caretAtFirstPosition}`)
                // console.log(`_caretStartPositionInSpan = ${this._caretStartPositionInSpan}`)
                // console.log(`_caretEndPositionInSpan = ${this._caretEndPositionInSpan}`)
            }
        } else if (selection) {
            // 非區塊模式
            this._currentSelection = selection;
            this._caretPosition = selection.anchorOffset;
            this._caretAtFirstPosition = selection.anchorOffset === 0;
            this._caretAtLastPostion = selection.anchorOffset === this.content.length;
        } else {
            // 在很稀少的狀況會抓不到 Selection 物件，這時要清掉相關的資料。
            this._currentSelection = undefined;
            this._caretPosition = undefined;
            this._caretAtFirstPosition = false;
            this._caretAtLastPostion = false;
        }
    }

    /** 清除輸入指標資訊，如果元件處於區塊模式，則將被觸發的區塊效果還原。 */
    private _clearFocusSegmentSpan(): void {
        this._caretPosition = undefined;
        this._caretAtFirstPosition = false;
        this._caretAtLastPostion = false;

        if (!this._segmentMode) {
            return;
        }

        this._segmentElements.forEach(elem => {
            if (elem.span.classList.contains("focus")) {
                elem.span.classList.remove("focus");
            }
        });
    }

    /**
     * 輸入文字以更新文字輸入框，這會導致區塊模式被關閉。
     * @param value 新的文字
     */
    private _setTextAreaValue(value: string): void {
        this._textAreaUpdating = true;

        if (this._segmentMode) {
            // 如果目前是區塊模式，刪除所有區塊物件並將模式關閉。
            this._segmentElements.forEach(elem => elem.span.remove());
            this._segmentElements = [];
            this._segmentMode = false;
        }

        // 將文字輸入框的內文取代成新的版本，這會導致輸入指標跑回最前面。
        this._textarea.innerText = Util.replaceHtmlEntities((value ?? ""));

        // 如果可以抓到上一個輸入指標的位置，嘗試將指標移動到正確的位置。
        if (this._caretPosition !== undefined && this._currentSelection) {
            const range = document.createRange();
            const node = this._textarea.childNodes[0];
            const textContent = node.textContent ?? "";
            const offset = this._caretPosition - 1 < textContent.length ? this._caretPosition - 1 : textContent.length;
            const adoptedOffset = offset <= textContent.length ? (offset < 0 ? 0 : offset) : textContent.length;
            range.setStart(node, adoptedOffset);
            range.setEnd(node, adoptedOffset);
            this._currentSelection.removeAllRanges();
            this._currentSelection.addRange(range)
        }

        this._textAreaUpdating = false;
    }

    /**
     * 輸入區塊以更新文字輸入框，這會導致區塊模式被啟用。
     * @param segments 新的區塊陣列
     */
    private _setTextAreaSegments(segments: WordSegment[]): void {
        this._textAreaUpdating = true;
        this._segmentMode = true;

        if (this._segmentElements.length === 0) {
            // 如果此元件尚未包含任何區塊，代表此元件剛建立並第一次接收到資料。
            this._textarea.innerText = "";
            this._segmentElements = segments.map(segment => {
                const span = document.createElement("span");
                span.className = "se-segment-span";
                span.setAttribute("start", segment.start);
                span.innerText = segment.word;
                this._textarea.appendChild(span);
    
                return { span, segment: { ...segment } };
            });
        } else {
            // 首先要找出可能受到影響導致文字被刪除的區塊，這個陣列儲存所有受到影響的區塊。
            const effectedSegmentElements: WordSegment[] = [];
            if (segments.length <= this._segmentElements.length && this._caretPosition !== undefined) {
                // 這裡透過使用者輸入指標圈選的範圍來找出被影響的區塊。
                const selectionRange = Math.abs(this._caretEndPosition - this._caretStartPosition) + 1;
                for (let i = 0; i < selectionRange; i++) {
                    effectedSegmentElements.push({ ...this._segmentElements[this._caretStartPosition + i].segment });
                }
            }
            
            // 清除文字輸入框。
            this._textarea.innerText = "";

            // 使用新輸入的區塊陣列建立新的區塊元件。
            this._segmentElements = segments.map(segment => {
                const span = document.createElement("span");
                span.className = "se-segment-span";
                span.setAttribute("start", segment.start);
                span.innerText = segment.word;
                this._textarea.appendChild(span);
                return { span, segment: { ...segment } };
            });

            // 如果可以抓到之前輸入指標的位置，嘗試將指標放回正確的位置。
            if (this._caretPosition !== undefined && this._currentSelection) {
                const range = document.createRange();

                if (effectedSegmentElements.length === 1) {
                    // 如果受到影響的區塊只有一個，代表使用者沒有使用範圍修改，或修改範圍僅限同一個區塊。
                    if (this._editor.keyState.backspace) {
                        // Backspace 按鍵被觸發中，代表某段文字被向前刪除了。
                        // 修正指標的位置。如果受影響的區塊只有一個字，代表該區塊被刪除了，此時指標應來到該區塊的前一節點。
                        const adoptedCaretPosition = this._caretPosition > 0 ? 
                            this._caretPosition - (effectedSegmentElements[0].word.length > 1 ? 0 : 1) : 0;

                        // 使用指標位置來找出 Span 節點。
                        const node = this._textarea.childNodes.length > adoptedCaretPosition ? 
                            this._textarea.childNodes.item(adoptedCaretPosition) : this._textarea.childNodes.item(this._textarea.childNodes.length - 1);

                        if (effectedSegmentElements[0].word.length > 1 || this._caretPosition > 0) {
                            // 如果受影響的節點擁有超過一個字，或是指標位置不在最前面，則需要透過計算刪除的字數來調整指標位置。
                            const effectText = effectedSegmentElements[0].word;
                            const currentText = node.textContent ?? "x";
                            const textDifferenceLength = effectText.length - currentText.length;

                            const offset = this._caretStartPositionInSpan - textDifferenceLength;
                            const adoptedOffset = offset < 0 ? 0 : offset > currentText.length ? currentText.length : offset;
                            range.setStart(node.childNodes[0], adoptedOffset);
                            range.setEnd(node.childNodes[0], adoptedOffset);
                        } else {
                            // 如果受影響的節點被砍掉了，且指標位置在整句的最前面，則直接將指標放回最前面。
                            range.setStart(node.childNodes[0], 0);
                            range.setEnd(node.childNodes[0], 0);
                        }
                    } else if (this._editor.keyState.delete) {
                        // Delete 按鍵被觸發中，代表某段文字被向後刪除了。
                        // 根據儲存的指標位置找出受影響的 Span 節點。
                        const node = this._textarea.childNodes.length > this._caretPosition ? 
                            this._textarea.childNodes.item(this._caretPosition) : this._textarea.childNodes.item(this._textarea.childNodes.length - 1);
                        // 指標就該放在該 Span 先前的位置。
                        const offset = this._caretStartPositionInSpan;
                        range.setStart(node.childNodes[0], offset);
                        range.setEnd(node.childNodes[0], offset);
                    } else {
                        // 如果不是 Backspace 也不是 Delete，代表文字長度被增加了。
                        // 找出受影響的節點。
                        const node = this._textarea.childNodes.length > this._caretPosition ? 
                            this._textarea.childNodes.item(this._caretPosition) : this._textarea.childNodes.item(this._textarea.childNodes.length - 1);
                        
                        // 透過增加的字數來調整指標位置。
                        const effectText = effectedSegmentElements[0].word;
                        const currentText = node.textContent ?? "";
                        const textDifferenceLength = currentText.length - effectText.length;
                        const offset = this._caretEndPositionInSpan + textDifferenceLength;
                        const adoptedOffset = offset <= currentText.length ? offset : currentText.length;

                        range.setStart(node.childNodes[0], adoptedOffset);
                        range.setEnd(node.childNodes[0], adoptedOffset);
                    }
                } else {
                    // 如果受影響的區塊超過一個，則此次變更一定是"刪除"，後續的文字增加會再重複觸發此方法。
                    // _caretStartPosition 位置會在受影響的第一個區塊，將他 -1 來調整區塊為圈選範圍的前一個。
                    const nodePosition = this._caretStartPosition - 1;

                    // 找出前一個節點。
                    const node = this._textarea.childNodes.length > nodePosition ? 
                        nodePosition > 0 ? this._textarea.childNodes.item(nodePosition) : this._textarea.childNodes.item(0) :
                        this._textarea.childNodes.item(this._textarea.childNodes.length - 1);
                    
                    const offset = nodePosition <= 0 ? 0 : 1;
                    range.setStart(node.childNodes[0], offset);
                    range.setEnd(node.childNodes[0], offset);
                }

                // 套用輸入指標的設定
                this._currentSelection.removeAllRanges();
                this._currentSelection.addRange(range)
            }

            // 更新輸入指標所觸發的區塊
            this._updateFocusSegmentSpan();
        }

        this._textAreaUpdating = false;
    }

    private _getTextAreaValue(): string {
        return this._textarea.innerText;
    }

    private _getSegments(): WordSegment[] {
        if (!this._segmentMode) {
            return [];
        } else {
            return this._segmentElements
                .filter(elem => !!elem.span.parentNode)
                .map(elem => {
                    return { start: elem.segment.start, word: elem.span.textContent ?? "" };
                });
        }
    }

    /**
     * 取得指定時間輸入框的內文。
     * @param element 要取得資料的文字輸入框元件。
     * @returns 通常是時間格式的文字。
     */
    private _getTimeInputValue(element: HTMLSpanElement): string {
        if (this._frameRate) {
            const secondArray = element.innerText.split(",");
            if (secondArray.length === 2) {
                const miniSecond = Math.ceil(Number(secondArray[1]) * 1000 / this._frameRate!);
                secondArray[1] = miniSecond < 10 ? `00${miniSecond}` : miniSecond < 100 ? `0${miniSecond}` : String(miniSecond);
                return secondArray.join(".");
            }
        }

        return element.innerText;
    }
}
