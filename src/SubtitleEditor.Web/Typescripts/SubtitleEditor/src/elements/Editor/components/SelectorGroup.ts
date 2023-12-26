import { SubtitleOptions } from "../options";

export default class SelectorGroup {

    subtitleOptions: SubtitleOptions[] = [];

    element: HTMLDivElement = document.createElement("div");
    label: HTMLLabelElement = document.createElement("label");
    selector: HTMLSelectElement = document.createElement("select");

    get value(): string { return this.selector.value; }

    onSelectorChange: () => Promise<void> = async () => { };

    constructor(subtitleOptions: SubtitleOptions[]) {
        this.subtitleOptions = subtitleOptions.map(o => { return { ...o }; });
        this._build();
    }

    private _build(): void {
        this.element.className = "se-selector-group";

        this.label.innerHTML = "目前檔案";
        this.element.appendChild(this.label);

        this.element.appendChild(this.selector);
        this.selector.addEventListener("change", () => this.onSelectorChange());

        this.subtitleOptions.forEach((o, index) => {
            const option = document.createElement("option");
            option.value = String(index);
            option.innerHTML = o.video && o.video.name ? o.video.name : `音檔 ${index + 1}`;
            this.selector.appendChild(option);
        });
    }
}
