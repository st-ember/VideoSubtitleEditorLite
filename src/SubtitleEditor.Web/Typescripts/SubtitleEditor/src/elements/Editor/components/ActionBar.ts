import icons from "../../../icons";

export default class ActionBar {

    element: HTMLDivElement = document.createElement("div");
    undoButton: HTMLButtonElement = document.createElement("button");
    redoButton: HTMLButtonElement = document.createElement("button");
    goBackButton: HTMLButtonElement = document.createElement("button");
    shiftTimeButton: HTMLButtonElement = document.createElement("button");
    margeButton: HTMLButtonElement = document.createElement("button");
    deleteButton: HTMLButtonElement = document.createElement("button");
    compensateButton: HTMLButtonElement = document.createElement("button");
    searchButton: HTMLButtonElement = document.createElement("button");

    onTriggerUndo: () => Promise<void> = async () => { };
    onTriggerRedo: () => Promise<void> = async () => { };
    onTriggerGoBack: () => Promise<void> = async () => { };
    onTriggerShiftTime: () => Promise<void> = async () => { };
    onTriggerMarge: () => Promise<void> = async () => { };
    onTriggerDelete: () => Promise<void> = async () => { };
    onTriggerCompensate: () => Promise<void> = async () => { };
    onTriggerSearch: () => Promise<void> = async () => { };

    constructor() {
        this._build();
        this._bindEvents();
    }

    setUndoEnable(enable: boolean): void {
        this.undoButton.disabled = !enable;
    }

    setRedoEnable(enable: boolean): void {
        this.redoButton.disabled = !enable;
    }

    setGoBackEnable(enable: boolean): void {
        this.goBackButton.disabled = !enable;
    }

    setShiftTimeEnable(enable: boolean): void {
        this.shiftTimeButton.disabled = !enable;
    }

    setMargeEnable(enable: boolean): void {
        this.margeButton.disabled = !enable;
    }

    setDeleteEnable(enable: boolean): void {
        this.deleteButton.disabled = !enable;
    }

    setCompensateEnable(enable: boolean): void {
        this.compensateButton.disabled = !enable;
    }

    setSearchEnable(enable: boolean): void {
        this.searchButton.disabled = !enable;
    }

    private _build(): void {
        this.element.className = "se-action-bar";

        [this.undoButton, this.redoButton, this.goBackButton, this.shiftTimeButton, this.margeButton, this.compensateButton, this.searchButton, this.deleteButton]
            .forEach(b => {
                b.type = "button";
                b.disabled = true;
                this.element.appendChild(b);
            });

        this.undoButton.className = "small-button primary se-undo-button";
        this.undoButton.title = "上一步";
        this.undoButton.innerHTML = `${icons.Undo}上一步`;

        this.redoButton.className = "small-button primary se-redo-button";
        this.redoButton.title = "下一步";
        this.redoButton.innerHTML = `${icons.Redo}下一步`;

        this.goBackButton.className = "small-button primary se-go-back-button";
        this.goBackButton.title = "回到目前播放位置";
        this.goBackButton.innerHTML = `回到目前播放位置`;

        this.shiftTimeButton.className = "small-button primary se-shift-all-button";
        this.shiftTimeButton.title = "修改時間";
        this.shiftTimeButton.innerHTML = `${icons.EditTimeAll}修改時間`;

        this.margeButton.className = "small-button primary se-marge-button";
        this.margeButton.title = "合併";
        this.margeButton.innerHTML = `合併`;

        this.compensateButton.className = "small-button primary se-compensate-button";
        this.compensateButton.title = "補償句子間的空白";
        this.compensateButton.innerHTML = `補償空白`;

        this.searchButton.className = "small-button primary se-search-button";
        this.searchButton.title = "搜尋";
        this.searchButton.innerHTML = `<i class="fa-solid fa-magnifying-glass"></i>搜尋`;
        this.searchButton.disabled = false;

        this.deleteButton.className = "small-button danger se-delete-button";
        this.deleteButton.title = "刪除";
        this.deleteButton.innerHTML = `${icons.Delete}刪除`;
    }

    private _bindEvents(): void {
        this.undoButton.addEventListener("click", () => {
            if (!this.undoButton.disabled) {
                this.onTriggerUndo();
            }
        });

        this.redoButton.addEventListener("click", () => {
            if (!this.redoButton.disabled) {
                this.onTriggerRedo();
            }
        });

        this.goBackButton.addEventListener("click", () => {
            if (!this.goBackButton.disabled) {
                this.onTriggerGoBack();
            }
        });

        this.shiftTimeButton.addEventListener("click", () => {
            if (!this.shiftTimeButton.disabled) {
                this.onTriggerShiftTime();
            }
        });

        this.margeButton.addEventListener("click", () => {
            if (!this.margeButton.disabled) {
                this.onTriggerMarge();
            }
        });

        this.compensateButton.addEventListener("click", () => {
            if (!this.compensateButton.disabled) {
                this.onTriggerCompensate();
            }
        });

        this.searchButton.addEventListener("click", () => {
            if (!this.searchButton.disabled) {
                this.onTriggerSearch();
            }
        });

        this.deleteButton.addEventListener("click", () => {
            if (!this.deleteButton.disabled) {
                this.onTriggerDelete();
            }
        });
    }
}
