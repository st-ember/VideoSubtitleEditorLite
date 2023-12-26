import Editor from "../Editor";
import { FocusLineCommand, ReplaceCommand, SearchCommand } from "../commands";
import Line from "./Line";

export default class SearchPanel {

    editor: Editor;
    sideGroup: HTMLDivElement;
    element: HTMLDivElement = document.createElement("div");

    get searchText(): string { return this._findInput.innerText; }
    set searchText(value: string | undefined) { 
        this._findInput.innerText = value ?? "";
        if (!this._findInput.innerText) {
            this._updateBySearchResult([]);
        }
    }
    get replaceText(): string { return this._replaceInput.innerText; }
    set replaceText(value: string | undefined) { this._replaceInput.innerText = value ?? ""; }

    private _inputGroup: HTMLDivElement = document.createElement("div");
    private _resultGroup: HTMLDivElement = document.createElement("div");

    private _findInput: HTMLDivElement = document.createElement("div");
    private _findInputPlaceholder: HTMLSpanElement = document.createElement("span");
    private _findPrevButton: HTMLButtonElement = document.createElement("button");
    private _findCounter: HTMLSpanElement = document.createElement("span");
    private _findNextButton: HTMLButtonElement = document.createElement("button");

    private _replaceInput: HTMLDivElement = document.createElement("div");
    private _replaceInputPlaceholder: HTMLSpanElement = document.createElement("span");
    private _replaceButton: HTMLButtonElement = document.createElement("button");
    private _replaceAllButton: HTMLButtonElement = document.createElement("button");

    private _focusFindInput: boolean = false;

    private _matchedLines: Line[] = [];
    private _currentLineIndex: number = 0;

    constructor(editor: Editor, sideGroup: HTMLDivElement) {
        this.editor = editor;
        this.sideGroup = sideGroup;
        this._buildAsync();
    }

    focus(): void {
        //this._findInput.focus();
    }

    setSearchResult(lines: Line[]): void {
        this._updateBySearchResult(lines);
    }

    async updateLinesAsync(): Promise<void> {
        if (this._matchedLines.length > 0) {
            await SearchCommand.doSearchAsync(this.editor, this._findInput.innerText);
        }
    }

    private async _buildAsync(): Promise<void> {
        this.element.className = "se-search-panel";
        this.sideGroup.appendChild(this.element);

        this._inputGroup.className = "se-search-input-group";
        this.element.appendChild(this._inputGroup);

        this._resultGroup.className = "se-search-result-group";
        this.element.appendChild(this._resultGroup);

        this._buildInputGroup();
        this._bindEvents();
    }

    private _buildInputGroup(): void {
        const findGroup = document.createElement("div");
        findGroup.className = "se-search-input-group";
        this._inputGroup.appendChild(findGroup);

        this._findInput.className = "se-search-find-input";
        this._findInput.contentEditable = "true";
        findGroup.appendChild(this._findInput);

        this._findInputPlaceholder.className = "se-search-placeholder";
        this._findInputPlaceholder.innerText = "搜尋";
        findGroup.appendChild(this._findInputPlaceholder);

        const findActionGroup = document.createElement("div");
        findActionGroup.className = "se-search-action-group";
        findGroup.appendChild(findActionGroup);

        const replaceGroup = document.createElement("div");
        replaceGroup.className = "se-search-input-group";
        this._inputGroup.appendChild(replaceGroup);

        this._replaceInput.className = "se-search-replace-input";
        this._replaceInput.contentEditable = "true";
        replaceGroup.appendChild(this._replaceInput);

        this._replaceInputPlaceholder.className = "se-search-placeholder";
        this._replaceInputPlaceholder.innerText = "取代";
        replaceGroup.appendChild(this._replaceInputPlaceholder);

        const replaceActionGroup = document.createElement("div");
        replaceActionGroup.className = "se-search-action-group";
        replaceGroup.appendChild(replaceActionGroup);
        
        this._buildFindActions(findActionGroup);
        this._buildReplaceActions(replaceActionGroup);
    }

    private _buildFindActions(actionGroup: HTMLDivElement): void {
        this._findPrevButton.className = "se-search-find-prev dark mini-button";
        this._findPrevButton.type = "button";
        this._findPrevButton.innerHTML = "上一個";
        actionGroup.appendChild(this._findPrevButton);

        this._findCounter.className = "se-search-find-counter";
        this._findCounter.innerText = "0/0";
        actionGroup.appendChild(this._findCounter);

        this._findNextButton.className = "se-search-find-next dark mini-button";
        this._findNextButton.type = "button";
        this._findNextButton.innerHTML = "下一個";
        actionGroup.appendChild(this._findNextButton);
    }

    private _buildReplaceActions(actionGroup: HTMLDivElement): void {
        this._replaceButton.className = "se-search-replace-next dark warning-hover mini-button";
        this._replaceButton.type = "button";
        this._replaceButton.innerHTML = "取代";
        actionGroup.appendChild(this._replaceButton);

        this._replaceAllButton.className = "se-search-replace-all dark warning-hover mini-button";
        this._replaceAllButton.type = "button";
        this._replaceAllButton.innerHTML = "取代全部";
        actionGroup.appendChild(this._replaceAllButton);
    }

    private _bindEvents(): void {

        this._findInput.addEventListener("keyup", keyboardEvent => {
            if (keyboardEvent.key === "Enter") {
                keyboardEvent.preventDefault();
                if (this._findInput.innerText) {
                    SearchCommand.doSearchAsync(this.editor, this._findInput.innerText);
                } else {
                    SearchCommand.doClearSearchAsync(this.editor);
                }
            }
        });

        this._findInput.addEventListener("focus", () => {
            this._focusFindInput = true;
        });

        this._findInput.addEventListener("blur", () => {
            this._focusFindInput = false;
        });

        this._replaceInput.addEventListener("keyup", keyboardEvent => {
            if (keyboardEvent.key === "Enter") {
                keyboardEvent.preventDefault();
            }
        });

        this._findPrevButton.addEventListener("click", () => this._findPrev());
        this._findNextButton.addEventListener("click", () => this._findNext());
        this._replaceButton.addEventListener("click", () => this._replaceCurrentAsync());
        this._replaceAllButton.addEventListener("click", () => this._replaceAllAsync());

        this._initTimer(this._findInput, this._findInputPlaceholder, async () => {
            if (this._focusFindInput) {
                if (this._findInput.innerText) {
                    const matchedLines = await SearchCommand.doSearchAsync(this.editor, this._findInput.innerText);
                    this._updateBySearchResult(matchedLines);
                } else {
                    SearchCommand.doClearSearchAsync(this.editor);
                    this._updateBySearchResult([]);
                }
            }
        });

        this._initTimer(this._replaceInput, this._replaceInputPlaceholder, () => { });
    }

    private _initTimer(input: HTMLDivElement, placeholder: HTMLSpanElement, onChangeFunc: () => void): void {
        let originalValue = input.innerText;
        let tempValue = input.innerText;
        let countdown = 10;
        setInterval(() => {
            const currentValue = input.innerText;

            if (currentValue !== tempValue) {
                countdown = 10;
                tempValue = currentValue;
            }

            if (tempValue !== originalValue) {
                countdown--;
                if (countdown <= 0) {
                    countdown = 10;
                    originalValue = tempValue;
                    onChangeFunc();
                }
            } else {
                countdown = 10;
            }

            placeholder.style.display = currentValue ? "none" : "";
        }, 100);
    }

    private _updateBySearchResult(lines: Line[]): void {
        this._matchedLines = lines;
        if (lines.length > 0) {
            this._currentLineIndex = lines.length <= this._currentLineIndex ? lines.length - 1 : this._currentLineIndex;
            this._findCounter.style.display = "";
            this._replaceButton.style.display = "";
            this._replaceAllButton.style.display = "";
            this._focusCurrentLineAsync();
        } else {
            this._findCounter.style.display = "none";
            this._replaceButton.style.display = "none";
            this._replaceAllButton.style.display = "none";
        }
    }

    private _findNext(): void {
        this._clearSearchHighlight();
        this._currentLineIndex++;

        if (this._currentLineIndex >= this._matchedLines.length) {
            this._currentLineIndex = 0;
        }

        this._focusCurrentLineAsync();
    }

    private _findPrev(): void {
        this._clearSearchHighlight();
        this._currentLineIndex--;

        if (this._currentLineIndex < 0) {
            this._currentLineIndex = this._matchedLines.length - 1;
        }

        this._focusCurrentLineAsync();
    }

    private async _replaceCurrentAsync(): Promise<void> {
        const line = this._currentLineIndex < this._matchedLines.length ? this._matchedLines[this._currentLineIndex] : undefined;
        if (line && !line.deleted) {
            await ReplaceCommand.invokeAsync([line], this.searchText, this.replaceText);
        }

        await this.updateLinesAsync();
    }

    private async _replaceAllAsync(): Promise<void> {
        await ReplaceCommand.invokeAsync(this._matchedLines.filter(line => !line.deleted), this.searchText, this.replaceText);
        await this.updateLinesAsync();
    }

    private _clearSearchHighlight(): void {
        this._matchedLines.forEach(o => o.setSearchHighlight(false));
    }

    private async _focusCurrentLineAsync(): Promise<void> {
        const line = this._currentLineIndex < this._matchedLines.length ? this._matchedLines[this._currentLineIndex] : undefined;
        if (line && !line.deleted) {
            line.setSearchHighlight(true);
            this._findCounter.innerText = `${this._currentLineIndex + 1}/${this._matchedLines.length}`;
            await FocusLineCommand.invokeAsync(line);
        }
    }
}