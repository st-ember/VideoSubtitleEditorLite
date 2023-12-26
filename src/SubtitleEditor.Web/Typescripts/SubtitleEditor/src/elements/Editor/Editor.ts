import VideoPlayer from "../VideoPlayer";
import { ActionBar, Line, SelectorGroup, Subtitle, Transcript } from "./components";
import { EditorOptions } from "./options";
import { SubtitleData } from "./models";
import { dialogController } from "uform-dialog";
import { CancelSelectionCommand, CompensateLinesCommand, DeleteLinesCommand, MargeLineCommand, RedoCommand, SearchCommand, SelectAllLinesCommand, SelectLineCommand, ShiftLinesCommand, UndoCommand } from "./commands";
import SearchPanel from "./components/SearchPanel";

export default class Editor {

    container?: HTMLElement;

    currentTranscript?: Transcript;
    currentSubtitle?: Subtitle;
    transcripts: Transcript[] = [];
    subtitles: Subtitle[] = [];
    searchpanel?: SearchPanel;
    editingLine?: Line;

    keyState = { shift: false, alt: false, control: false, backspace: false, delete: false };
    inSearchMode = false;
    sideGroupShowing = false;
    latestSelectedLineIndex: number = 0;

    get datas(): SubtitleData[] { return this.subtitles.map(subtitle => subtitle.data); }
    get time(): number { return this._player.time; }
    get player(): VideoPlayer { return this._player; }
    get alwaysFinishLine(): boolean { return this._alwaysFinishLine; }
    get options(): EditorOptions { return this._options; }
    get wordLimit(): number | undefined { return this._wordLimit; }

    private _options: EditorOptions = { subtitles: [] };

    get onHotkeyStateChange(): (hotkeyListensing: boolean) => Promise<void> { return this._getAdoptedFuncFromOptions("onHotkeyStateChange"); }
    private get _beforeBuildAsync(): () => Promise<void> { return this._getAdoptedFuncFromOptions("beforeBuild"); }
    private get _afterBuildAsync(): () => Promise<void> { return this._getAdoptedFuncFromOptions("afterBuild"); }
    private get _beforeLoadAsync(): () => Promise<void> { return this._getAdoptedFuncFromOptions("beforeLoad"); }
    private get _afterLoadAsync(): () => Promise<void> { return this._getAdoptedFuncFromOptions("afterLoad"); }
    private get _onErrorAsync(): (error: string) => Promise<void> { return this._getAdoptedFuncFromOptions("onError"); }
    
    private _selectorGroup?: SelectorGroup;
    private _playerTitle: HTMLDivElement = document.createElement("div");
    private _playerTitleGroup: HTMLDivElement = document.createElement("div");
    private _actionBar?: ActionBar;
    private _player: VideoPlayer = new VideoPlayer();

    private _element: HTMLDivElement = document.createElement("div");
    private _sourceGroup: HTMLDivElement = document.createElement("div");
    private _editorGroup: HTMLDivElement = document.createElement("div");
    private _sideGroup: HTMLDivElement = document.createElement("div");
    private _sideGroupTitle: HTMLDivElement = document.createElement("div");
    private _transcriptTitle: HTMLDivElement = document.createElement("div");
    private _transcriptModeGroup: HTMLDivElement = document.createElement("div");
    private _transcriptGroup: HTMLDivElement = document.createElement("div");
    private _subtitleTitle: HTMLDivElement = document.createElement("div");
    private _subtitleTitleGroup: HTMLDivElement = document.createElement("div");
    private _subtitleGroup: HTMLDivElement = document.createElement("div");

    private __subtitleGroupRect?: DOMRect;
    private get _subtitleGroupRect(): DOMRect {
        if (!this.__subtitleGroupRect) {
            this.__subtitleGroupRect = this._subtitleGroup.getBoundingClientRect();
        }
        return this.__subtitleGroupRect;
    }
    
    private _mouseInSubtitleGroup: boolean = false;
    private _playingLine?: Line;
    private _nextLine?: Line;
    private _isWatchingPlayingLine: boolean = true;
    private _isTracking: boolean = true;
    private _periodPlayState?: { start: number, end: number } = undefined;
    private _alwaysFinishLine: boolean = true;
    private _wordLimit?: number = undefined;

    constructor() {
        console.warn(`© ${new Date().getFullYear()} 過日子股份有限公司版權所有。`);
    }

    async buildAsync(options: EditorOptions): Promise<{ success: boolean, message?: string }> {
        
        this._options = { ...options };
        if (!this._options.subtitles || this._options.subtitles.length === 0) {
            const message = "必須至少提供一份字幕設定。"
            await this._onErrorAsync(message);
            return { success: false, message };
        }
        
        this._wordLimit = this._options.topic?.wordLimit;

        await this._beforeBuildAsync();

        if (options.container) {
            this.container = options.container;
        } else {
            this.container = document.createElement("div");
        }
        
        this._selectorGroup = new SelectorGroup(this._options.subtitles);
        this._actionBar = new ActionBar();

        this._build();

        this.searchpanel = new SearchPanel(this, this._sideGroup);

        this._bindSelectorHooks();
        this._bindVideoPlayerHooks();
        this._bindActionButtonHooks();
        this._bindWindowEvents();
        
        const loadResult = await this.loadAsync(0);
        if (!loadResult.success) {
            return loadResult;
        }
        
        await this._afterBuildAsync();

        return { success: true };
    }

    async loadAsync(index: number): Promise<{ success: boolean, message?: string }> {
        await this._beforeLoadAsync();
        this.unload();

        this.transcripts.forEach((transcript, i) => transcript.setActive(i === index));
        this.currentTranscript = this.transcripts[index];
        this.updateTranscriptState();
        this.updateTranscriptModeGroup();

        this.subtitles.filter((_subtitle, i) => i !== index).forEach(subtitle => subtitle.setActiveAsync(false));
        this.currentSubtitle = this.subtitles[index];
        this.currentSubtitle.onHistoryChanged = async () => this.updateUndoRedoButton();
        await this.currentSubtitle.setActiveAsync(true);
        await this.updateWordLimitAsync();

        this.updateUndoRedoButton();
        this._subtitleGroup.scrollTo({ top: 0, behavior: "smooth" });
        
        const videoOptions = this._options.subtitles[index].video;
        if (!this._player.isLoaded(<string>videoOptions?.name)) {
            const loadResult = await this._player.loadAsync(videoOptions);
            if (!loadResult.success) {
                await this._onErrorAsync(loadResult.message ?? "");
                await dialogController.showErrorAsync("錯誤", loadResult.message ?? "載入時發生錯誤。");
                return loadResult;
            }
        }
        
        await this._player.switchToAsync(<string>videoOptions?.name);

        await this._afterLoadAsync();
        return { success: true };
    }

    unload(): void {
        this.cancelLineAsync();
        this.disableUndoRedoButton();
        this._player.unload();

        if (this.currentTranscript) {
            this.currentTranscript.setLoaded(false);
            this.currentTranscript = undefined;
        }

        if (this.currentSubtitle) {
            this.currentSubtitle.setLoaded(false);
            this.currentSubtitle = undefined;
        }
    }

    dispose(): void {
        this._player.dispose();
        this._unbindWindowEvents();
    }

    play(time?: number): void {
        if (this._periodPlayState) {
            this._periodPlayState = undefined;
        }

        this._player.play(time);
    }

    playPeriod(start: number, end: number): void {
        if (start === undefined || start === null || end === undefined || end === null || start >= end) {
            return;
        }

        this._periodPlayState = { start, end };
        this._player.play(start);
    }

    pause(): void {
        this._player.pause();
    }

    goBack(): void {
        this._isTracking = true;
        this._updateWatchingPlayingLine();
        this._scrollToPlayingLine();
    }

    async editLineAsync(line: Line): Promise<void> {
        if (this.editingLine && this.editingLine.element === line.element) {
            return;
        }

        await this.cancelLineAsync();
        this.pause();

        this.editingLine = line;
        line.setEditable(true);
        this._isTracking = false;

        await SelectLineCommand.invokeAsync(line);
    }

    async finishLineAsync(options?: { blurTextarea: boolean }): Promise<void> {
        if (!this.editingLine) {
            return;
        }

        const { success } = await this.editingLine.finishAsync(options);
        if (success && options?.blurTextarea !== false) {
            this.editingLine = undefined;
            
            if (this.sideGroupShowing) {
                await this.searchpanel?.updateLinesAsync();
            }
        }
    }

    async cancelLineAsync(): Promise<void> {
        if (!this.editingLine) {
            return;
        }

        if (!this._alwaysFinishLine) {
            await this.editingLine.cancelAsync();
            this.editingLine = undefined;
        } else {
            await this.finishLineAsync();
        }
    }

    updateLineState(): void {
        if (!this.currentSubtitle) {
            return;
        }

        let newCurrentLine: Line | undefined;
        for (let i = 0; i < this.currentSubtitle.lines.length; i++) {
            if (this.currentSubtitle.lines[i].startTime <= this._player.time &&
                this._player.time < this.currentSubtitle.lines[i].endTime
            ) {
                newCurrentLine = this.currentSubtitle.lines[i];
                if (i + 1 < this.currentSubtitle.lines.length - 1) {
                    this._nextLine = this.currentSubtitle.lines[i + 1];
                }
            }
        }

        if (!newCurrentLine) {
            if (this._playingLine) {
                this._playingLine.setPlaying(false);
                this._playingLine = undefined;
            }
            return;
        }

        if (!this._playingLine || newCurrentLine.startTime !== this._playingLine.startTime) {
            this._playingLine = newCurrentLine;
            this._playingLine.setPlaying(true);

            const startTime = newCurrentLine.startTime;
            this.currentSubtitle.lines.forEach(line => {
                if (line.startTime !== startTime) {
                    line.setPlaying(false);
                }
            });

            this._updateWatchingPlayingLine();
            this._scrollToPlayingLine();
        }
    }

    showSideGroup(show: boolean): void {
        if (show) {
            this.sideGroupShowing = true;
            this._sideGroup.classList.remove("hide");
        } else {
            this.sideGroupShowing = false;
            this._sideGroup.classList.add("hide");
        }
    }

    private _build(): void {
        this._element.className = "se se-editor";

        if (this.container) {
            this.container.appendChild(this._element);
        }

        if (this._selectorGroup) {
            this._element.appendChild(this._selectorGroup.element);
        }

        this._sourceGroup.className = "se-source-group";
        this._element.appendChild(this._sourceGroup);

        this._editorGroup.className = "se-editor-group";
        this._element.appendChild(this._editorGroup);

        this._sideGroup.className = "se-side-group hide";
        this._element.appendChild(this._sideGroup);

        this._sideGroupTitle.className = "se-group-title";
        this._sideGroupTitle.innerText = "尋找與取代";
        this._sideGroup.appendChild(this._sideGroupTitle);

        const sideGroupCloseButton = document.createElement("button");
        sideGroupCloseButton.className = "se-title-close-button";
        sideGroupCloseButton.type = "button";
        this._sideGroupTitle.appendChild(sideGroupCloseButton);
        sideGroupCloseButton.addEventListener("click", () => this.showSideGroup(false));

        this._buildPlayerGroup();
        this._buildEditorGroup();
        this._buildTranscriptGroup();
        this._buildTranscriptTitleActionGroup();
        this._buildSubtitleGroup();
        this._buildSubtitles();
        this._buildTranscripts();
    }

    private _bindWindowEvents(): void {
        this._onWindowKeyDown = this._onWindowKeyDown.bind(this);
        window.addEventListener("keydown", this._onWindowKeyDown);

        this._onWindowKeyUp = this._onWindowKeyUp.bind(this);
        window.addEventListener("keyup", this._onWindowKeyUp);

        this._onWindowBlur = this._onWindowBlur.bind(this);
        window.addEventListener("blur", this._onWindowBlur);
    }

    private _unbindWindowEvents(): void {
        window.removeEventListener("keydown", this._onWindowKeyDown);
        window.removeEventListener("keyup", this._onWindowKeyUp);
        window.removeEventListener("blur", this._onWindowBlur);
    }

    private _bindSelectorHooks(): void {
        if (!this._selectorGroup) {
            return;
        }

        this._selectorGroup.onSelectorChange = async () => {
            this.loadAsync(Number(this._selectorGroup?.value ?? "0"));
        };
    }

    private _bindVideoPlayerHooks(): void {
        this._player.onTimeUpdate = async () => {
            this.updateLineState();
            this._processPeriodPlay();
        };

        this._player.onTriggerSwitch = async () => {

        };

        this._player.onSwitchCompleted = async () => {
            if (this.currentTranscript) {
                this.currentTranscript.setLoaded(true);
            }

            if (this.currentSubtitle) {
                this.currentSubtitle.setLoaded(true);
            }
        };
    }

    private _bindActionButtonHooks(): void {
        if (!this._actionBar) {
            return;
        }

        this._actionBar.onTriggerGoBack = async () => this.goBack();
        this._actionBar.onTriggerUndo = () => UndoCommand.invokeAsync(this);
        this._actionBar.onTriggerRedo = () => RedoCommand.invokeAsync(this);
        this._actionBar.onTriggerShiftTime = () => this._showShiftTimeDialogAsync();
        this._actionBar.onTriggerCompensate = () => this._showCompensateDialogAsync();
        this._actionBar.onTriggerSearch = () => <any>SearchCommand.invokeAsync(this);

        this._actionBar.onTriggerMarge = async () => {
            if (this.currentSubtitle) {
                MargeLineCommand.invokeAsync(this.currentSubtitle.selectedLines);
            }
        };

        this._actionBar.onTriggerDelete = () => this._showDeleteDialogAsync();
    }

    private async _showShiftTimeDialogAsync(): Promise<void> {
        await this.cancelLineAsync();
        await ShiftLinesCommand.invokeAsync(this);
    }

    private async _showCompensateDialogAsync(): Promise<void> {
        await this.cancelLineAsync();
        await CompensateLinesCommand.invokeAsync(this);
    }

    private async _showDeleteDialogAsync(): Promise<void> {
        this.cancelLineAsync();

        if (this.currentSubtitle) {
            const lines = this.currentSubtitle.selectedLines;
            if (lines.length > 0 && await dialogController.confirmWarningAsync("確認", `您確定要刪除選擇的 ${lines.length} 筆字幕嗎？`)) {
                await DeleteLinesCommand.invokeAsync(lines);
            }
        }
    }

    private _buildPlayerGroup(): void {
        this._playerTitle.className = "se-group-title";
        this._playerTitle.innerText = "播放器";
        this._sourceGroup.appendChild(this._playerTitle);

        this._sourceGroup.appendChild(this._player.element);

        if (!!this.options.topic?.frameRate) {
            this._playerTitleGroup.className = "se-group-title-group";
            this._playerTitle.appendChild(this._playerTitleGroup);
            this._playerTitleGroup.innerText = `每秒畫格數 ${this.options.topic?.frameRate}`;
        }
    }

    private _buildEditorGroup(): void {
        this._subtitleTitle.className = "se-group-title";
        this._subtitleTitle.innerText = "字幕編輯器";
        this._editorGroup.appendChild(this._subtitleTitle);

        if (this._actionBar) {
            this._editorGroup.appendChild(this._actionBar.element);
        }
    }

    private _buildTranscriptGroup(): void {
        this._transcriptTitle.className = "se-group-title";
        this._transcriptTitle.innerText = "逐字稿";
        this._sourceGroup.appendChild(this._transcriptTitle);

        this._transcriptGroup.className = "se-transcript-group";
        this._sourceGroup.appendChild(this._transcriptGroup);
    }

    private _buildTranscriptTitleActionGroup(): void {
        this._transcriptModeGroup.className = "se-group-title-group";
        this._transcriptTitle.appendChild(this._transcriptModeGroup);

        // 文字編輯模式
        const modifyModeCheckbox = document.createElement("input");
        modifyModeCheckbox.type = "checkbox";
        modifyModeCheckbox.id = "transcript-modify-mode-checkbox";
        this._transcriptModeGroup.appendChild(modifyModeCheckbox);

        const modifyModeModeText = document.createElement("label");
        modifyModeModeText.innerHTML = "逐字稿編輯模式";
        modifyModeModeText.setAttribute("for", "transcript-modify-mode-checkbox");
        modifyModeModeText.style.color = "#ccc";
        modifyModeModeText.style.marginRight = "10px";
        this._transcriptModeGroup.appendChild(modifyModeModeText);

        // 快速模式 checkbox
        const quickModeCheckbox = document.createElement("input");
        quickModeCheckbox.type = "checkbox";
        quickModeCheckbox.id = "transcript-mode-checkbox";
        this._transcriptModeGroup.appendChild(quickModeCheckbox);

        const quickModeText = document.createElement("label");
        quickModeText.innerHTML = "快速模式 (按下<span class=\"keycode-container quick-create-line-key\"></span>直接將整行建立為新字幕)";
        quickModeText.setAttribute("for", "transcript-mode-checkbox");
        quickModeText.style.color = "#ccc";
        this._transcriptModeGroup.appendChild(quickModeText);

        modifyModeCheckbox.addEventListener("change", () => {
            const modifyMode = modifyModeCheckbox.checked;
            if (modifyMode) {
                quickModeCheckbox.disabled = true;
                quickModeCheckbox.checked = false;

                if (this.currentTranscript) {
                    this.currentTranscript.modeState.quickBreakLine = false;
                    this.currentTranscript.setModifing(true);
                }
            } else {
                quickModeCheckbox.disabled = false;

                if (this.currentTranscript) {
                    this.currentTranscript.setModifing(false);
                }
            }
            
            modifyModeCheckbox.blur();
        });

        quickModeCheckbox.addEventListener("change", () => {
            if (this.currentTranscript) {
                this.currentTranscript.modeState.quickBreakLine = quickModeCheckbox.checked;
            }
            quickModeCheckbox.blur();
        });

        // 字數限制
        const wordLimitText = document.createElement("label");
        wordLimitText.innerHTML = "字數限制";
        wordLimitText.setAttribute("for", "transcript-word-limit-input");
        wordLimitText.style.color = "#ccc";
        wordLimitText.style.marginRight = "5px";
        wordLimitText.style.marginLeft = "10px";
        this._transcriptModeGroup.appendChild(wordLimitText);

        const wordLimitInput = document.createElement("input");
        wordLimitInput.type = "number";
        wordLimitInput.min = "1";
        wordLimitInput.max = "1024";
        wordLimitInput.step = "1";
        wordLimitInput.id = "transcript-word-limit-input";
        wordLimitInput.className = "word-limit-input";
        wordLimitInput.value = this._wordLimit ? String(this._wordLimit) : "";
        this._transcriptModeGroup.appendChild(wordLimitInput);

        let timer: number | undefined;
        let text = wordLimitInput.value;
        let count = 0;
        wordLimitInput.addEventListener("focus", () => {
            if (timer === undefined) {
                timer = setInterval(() => {
                    if (wordLimitInput.value !== text) {
                        count++;
                    }

                    if (count > 10) {
                        count = 0;
                        text = wordLimitInput.value;
                        this.updateWordLimitAsync(wordLimitInput);
                    }
                }, 100);
            }
        });

        wordLimitInput.addEventListener("blur", () => {
            if (timer !== undefined) {
                clearInterval(timer);
                timer = undefined;
            }

            count = 0;

            if (wordLimitInput.value !== text) {
                text = wordLimitInput.value;
                this.updateWordLimitAsync(wordLimitInput);
            }
        });
    }

    updateTranscriptModeGroup(): void {
        const quickBreakLine = !!this.currentTranscript && this.currentTranscript.modeState.quickBreakLine;
        const checkbox = <HTMLInputElement>document.getElementById("transcript-mode-checkbox");
        checkbox.checked = quickBreakLine;
    }

    async updateWordLimitAsync(input?: HTMLInputElement): Promise<void> {
        if (input) {
            const newValue = !!input.value ? Number(input.value) : 0;
            const adoptedNewValue = !isNaN(newValue) && newValue > 0 ? newValue : undefined;
            const inputElems = document.querySelectorAll(".word-limit-input");
            for (let i = 0; i < inputElems.length; i++) {
                (<HTMLInputElement>inputElems.item(i)).value = adoptedNewValue ? String(adoptedNewValue) : "";
            }

            this._wordLimit = adoptedNewValue;

            if (this._options.hooks?.onLimitationChange) {
                this._options.hooks?.onLimitationChange();
            }
        }
        
        if (this.currentTranscript) {
            this.currentTranscript.setWordLimit(this._wordLimit);
        }

        if (this.currentSubtitle) {
            this.currentSubtitle.setWordLimit(this._wordLimit);
        }
    }

    private _buildSubtitleGroup(): void {
        this._subtitleGroup.className = "se-subtitle-group";
        this._editorGroup.appendChild(this._subtitleGroup);

        this._subtitleGroup.addEventListener("scroll", () => {
            if (this._mouseInSubtitleGroup) {
                this._updateWatchingPlayingLine();
                if (!this._isWatchingPlayingLine) {
                    this._isTracking = false;
                } else if (!this.editingLine) {
                    this._isTracking = true;
                }
            }
        });

        this._subtitleGroup.addEventListener("mouseenter", () => this._mouseInSubtitleGroup = true);
        this._subtitleGroup.addEventListener("mouseleave", () => this._mouseInSubtitleGroup = false);

        this._subtitleTitleGroup.className = "se-group-title-group";
        this._subtitleTitle.appendChild(this._subtitleTitleGroup);

        const wordLimitText = document.createElement("label");
        wordLimitText.innerHTML = "字數限制";
        wordLimitText.setAttribute("for", "subtitle-word-limit-input");
        wordLimitText.style.color = "#ccc";
        wordLimitText.style.marginRight = "5px";
        this._subtitleTitleGroup.appendChild(wordLimitText);

        const wordLimitInput = document.createElement("input");
        wordLimitInput.type = "number";
        wordLimitInput.min = "1";
        wordLimitInput.max = "1024";
        wordLimitInput.step = "1";
        wordLimitInput.id = "subtitle-word-limit-input";
        wordLimitInput.className = "word-limit-input";
        wordLimitInput.value = this._wordLimit !== undefined ? String(this._wordLimit) : "";
        this._subtitleTitleGroup.appendChild(wordLimitInput);

        let timer: number | undefined;
        let text = wordLimitInput.value;
        let count = 0;
        wordLimitInput.addEventListener("focus", () => {
            if (timer === undefined) {
                timer = setInterval(() => {
                    if (wordLimitInput.value !== text) {
                        count++;
                    }

                    if (count > 10) {
                        count = 0;
                        text = wordLimitInput.value;
                        this.updateWordLimitAsync(wordLimitInput);
                    }
                }, 100);
            }
        });

        wordLimitInput.addEventListener("blur", () => {
            if (timer !== undefined) {
                clearInterval(timer);
                timer = undefined;
            }

            count = 0;

            if (wordLimitInput.value !== text) {
                text = wordLimitInput.value;
                this.updateWordLimitAsync(wordLimitInput);
            }
        });
    }

    private _buildTranscripts(): void {
        this.transcripts = this._options.subtitles.map(options => {
            const transcript = new Transcript(this, options);
            transcript.onModeStateInitialFuncs.push(() => this.updateTranscriptModeGroup());
            this._transcriptGroup.appendChild(transcript.transcriptContainer);
            return transcript;
        });
    }

    private _buildSubtitles(): void {
        this.subtitles = this._options.subtitles.map(options => {
            const subtitle = new Subtitle(this, options);
            this._subtitleGroup.appendChild(subtitle.lineContainer);
            return subtitle;
        });
    }

    updateTranscriptState(): void {
        if (this.currentTranscript) {
            if (this.currentTranscript.containsSpan) {
                this._transcriptTitle.style.display = "";
                this._transcriptGroup.classList.add("active");
            } else {
                this._transcriptTitle.style.display = "none";
                this._transcriptGroup.classList.remove("active");
            }
        }
    }

    private _updateWatchingPlayingLine(): void {
        if (!this._playingLine) {
            this._actionBar?.setGoBackEnable(false);
            return;
        }

        const subtitleGroupRect = this._subtitleGroupRect;
        const lineRect = this._playingLine.element.getBoundingClientRect();
        const y = lineRect.top - subtitleGroupRect.height + lineRect.height - subtitleGroupRect.top;
        this._isWatchingPlayingLine = y <= lineRect.height * -2 && y >= subtitleGroupRect.height * -1;
        this._actionBar?.setGoBackEnable(!this._isWatchingPlayingLine);
    }

    private _scrollToPlayingLine(): void {
        if (!this._isTracking || !this._playingLine || this._isWatchingPlayingLine) {
            return;
        }

        this.scrollToLine(this._playingLine);
    }

    scrollToLine(line: Line): void {
        const subtitleGroupRect = this._subtitleGroupRect;
        const lineRect = line.element.getBoundingClientRect();
        const y = lineRect.top - subtitleGroupRect.height + lineRect.height - subtitleGroupRect.top;
        if (this._nextLine) {
            const nextLineRect = this._nextLine.element.getBoundingClientRect();
            const nextLineY = y + nextLineRect.height * 4;
            const top = this._subtitleGroup.scrollTop + nextLineY;
            this._subtitleGroup.scrollTo({ top, behavior: "smooth" });
            this._actionBar?.setGoBackEnable(false);
        } else {
            const padding = lineRect.height * 4;
            const min = padding;
            const max = subtitleGroupRect.height - padding;
            const top = lineRect.top - subtitleGroupRect.top;
            const target = this._subtitleGroup.scrollTop + y;

            if (top > max) {
                this._subtitleGroup.scrollTo({ top: target + padding, behavior: "smooth" });
                this._actionBar?.setGoBackEnable(false);
            }

            if (top < min) {
                this._subtitleGroup.scrollTo({ top: target + (subtitleGroupRect.height - min), behavior: "smooth" });
                this._actionBar?.setGoBackEnable(false);
            }
        }
    }

    disableUndoRedoButton(): void {
        if (!this._actionBar) {
            return;
        }

        this._actionBar.setUndoEnable(false);
        this._actionBar.setRedoEnable(false);
    }

    updateUndoRedoButton(): void {
        if (!this.currentSubtitle || !this._actionBar) {
            return;
        }

        this._actionBar.setUndoEnable(this.currentSubtitle.histories.length > 0 && this.currentSubtitle.historyIndex >= 0);
        this._actionBar.setRedoEnable(this.currentSubtitle.historyIndex < this.currentSubtitle.histories.length - 1);
    }

    updateMultipleSelectionButton(): void {
        if (!this._actionBar) {
            return;
        }

        this._actionBar.setShiftTimeEnable(!!this.currentSubtitle && this.currentSubtitle.selectedLines.length > 0);
        this._actionBar.setCompensateEnable(!!this.currentSubtitle && this.currentSubtitle.selectedLines.length > 0);
        this._actionBar.setDeleteEnable(!!this.currentSubtitle && this.currentSubtitle.selectedLines.length > 0);
        this._actionBar.setMargeEnable(!!this.currentSubtitle && this.currentSubtitle.selectedLines.length > 1);
    }

    private _getAdoptedFuncFromOptions(name: string): () => Promise<void> {
        const func: (() => Promise<void>) | undefined | null = this._options.hooks ? (<any>this._options.hooks)[name] : undefined;
        return func !== undefined && func !== null && typeof func === "function" ? func : async () => {};
    }

    private _processPeriodPlay(): void {
        if (!this._periodPlayState) { return; }

        if (this._player.time >= this._periodPlayState.end) {
            this.pause();
            this._periodPlayState = undefined;
        }
    }

    private _onWindowKeyDown(event: KeyboardEvent): void {
        if (event.key === "Shift") {
            this.keyState.shift = true;
        } else if (event.key === "Alt") {
            this.keyState.alt = true;
        } else if (event.key === "Control") {
            this.keyState.control = true;
        } else if (event.key === "Backspace") {
            this.keyState.backspace = true;
        } else if (event.key === "Delete") {
            this.keyState.delete = true;
        }
    }

    private _onWindowKeyUp(event: KeyboardEvent): void {
        if (event.key === "Escape") {
            if (this.editingLine) {
                this.cancelLineAsync();
                if (this.inSearchMode) {
                    SearchCommand.invokeAsync(this);
                }
            } else if (this.searchpanel && this.inSearchMode) {
                SearchCommand.doClearSearchAsync(this);
            } else if (this.sideGroupShowing) {
                this.showSideGroup(false);
            } else {
                CancelSelectionCommand.invokeAsync(this);
            }
        } else if (event.key === "Shift") {
            this.keyState.shift = false;
        } else if (event.key === "Alt") {
            this.keyState.alt = false;
        } else if (event.key === "Control") {
            this.keyState.control = false;
        } else if (event.key === "Backspace") {
            this.keyState.backspace = false;
        } else if (event.key === "Delete") {
            this.keyState.delete = false;
        }
    }

    private _onWindowBlur(): void {
        this.keyState.shift = false;
        this.keyState.alt = false;
        this.keyState.control = false;
    }
}