import BenchmarkResult from "elements/models/benchmark/BenchmarkResult";
import { FormGeneric, NoteElement, ReadonlyTextAreaElement, TextAreaElement, TextElement, TitleElement } from "uform-form";

export default class TopicBenchmarkForm extends FormGeneric<{ argumentTemplate?: string }> {

    private _arguementTemplate?: TextAreaElement;

    private _resultTitle?: TitleElement;
    private _success?: TextElement;
    private _start?: TextElement;
    private _fileSource?: TextElement;
    private _pullRawFile?: TextElement;
    private _savedRawFile?: TextElement;
    private _startedConvert?: TextElement;
    private _completedConvert?: TextElement;
    private _length?: TextElement;
    private _totalCost?: TextElement;
    private _transferCost?: TextElement;
    private _convertCost?: TextElement;
    private _output?: ReadonlyTextAreaElement;
    private _error?: NoteElement;

    async buildChildrenAsync(): Promise<void> {

        const warning = NoteElement.fromAsync({
            type: "warning",
            text: "請先確保無其他轉檔工作進行中。"
        });
        await this.appendAsync(warning);

        this._arguementTemplate = await TextAreaElement.fromAsync({
            label: "轉檔參數",
            rows: 6
        });
        await this.appendAsync(this._arguementTemplate);

        const info = NoteElement.fromAsync({
            label: "參數",
            type: "info",
            text: "使用 {sourceFilePath} 來表示輸入路徑; 使用 {outputPath} 來表示輸出路徑。"
        });
        await this.appendAsync(info);

        const example = NoteElement.fromAsync({
            label: "範例",
            type: "info",
            text: "-i \"{sourceFilePath}\" -b:v 250000 -vf scale=842:-2 -codec:a aac -ac 2 -hls_time 10 -hls_list_size 0 \"{outputPath}\""
        });
        await this.appendAsync(example);

        this._resultTitle = await TitleElement.fromAsync({
            text: "結果",
            hide: true
        });
        await this.appendAsync(this._resultTitle);

        this._success = await TextElement.fromAsync({
            label: "轉檔結果",
            hide: true
        });
        await this.appendAsync(this._success);

        this._start = await TextElement.fromAsync({
            label: "開始時間",
            hide: true
        });
        await this.appendAsync(this._start);

        this._length = await TextElement.fromAsync({
            label: "長度",
            hide: true
        });
        await this.appendAsync(this._length);

        this._fileSource = await TextElement.fromAsync({
            label: "檔案來源",
            hide: true
        });
        await this.appendAsync(this._fileSource);

        this._pullRawFile = await TextElement.fromAsync({
            label: "開始下載檔案",
            hide: true
        });
        await this.appendAsync(this._pullRawFile);

        this._savedRawFile = await TextElement.fromAsync({
            label: "完成檔案傳輸",
            hide: true
        });
        await this.appendAsync(this._savedRawFile);

        this._startedConvert = await TextElement.fromAsync({
            label: "開始轉檔",
            hide: true
        });
        await this.appendAsync(this._startedConvert);

        this._completedConvert = await TextElement.fromAsync({
            label: "結束轉檔",
            hide: true
        });
        await this.appendAsync(this._completedConvert);

        this._totalCost = await TextElement.fromAsync({
            label: "總花費",
            hide: true
        });
        await this.appendAsync(this._totalCost);

        this._transferCost = await TextElement.fromAsync({
            label: "傳輸花費",
            hide: true
        });
        await this.appendAsync(this._transferCost);

        this._convertCost = await TextElement.fromAsync({
            label: "轉檔花費",
            hide: true
        });
        await this.appendAsync(this._convertCost);

        this._output = await ReadonlyTextAreaElement.fromAsync({
            label: "輸出",
            hide: true
        });
        await this.appendAsync(this._output);

        this._error = await NoteElement.fromAsync({
            hide: true,
            type: "danger",
            multipleLines: true
        });
        await this.appendAsync(this._error);
    }

    async getValueAsync(): Promise<{ argumentTemplate?: string | undefined; }> {
        const argumentTemplate = await this._arguementTemplate!.getValueAsync();
        return {
            argumentTemplate: argumentTemplate ? argumentTemplate : undefined
        };
    }

    async setResultAsync(result: BenchmarkResult): Promise<void> {
        await this._resultTitle?.showAsync();

        await this._success?.setValueAsync(result.success ? "成功" : "失敗 (無檔案產出)");
        await this._success?.showAsync();

        await this._start?.setValueAsync(result.start);
        await this._start?.showAsync();

        if (result.pullRawFileFromAsr) {
            await this._fileSource?.setValueAsync("ASR");
            await this._pullRawFile?.setValueAsync(result.pullRawFileFromAsr);
        } else {
            await this._fileSource?.setValueAsync("本機");
            await this._pullRawFile?.setValueAsync(result.pullRawFileFromLocal);
        }

        await this._fileSource?.showAsync();
        await this._pullRawFile?.showAsync();

        await this._savedRawFile?.setValueAsync(result.savedRawFile);
        await this._savedRawFile?.showAsync();

        await this._startedConvert?.setValueAsync(result.startedConvert);
        await this._startedConvert?.showAsync();

        await this._completedConvert?.setValueAsync(result.completedConvert);
        await this._completedConvert?.showAsync();

        await this._length?.setValueAsync(`${Math.round(100 * result.length) / 100} 秒`);
        await this._length?.showAsync();

        const total = (result.transferCost ?? 0) + (result.convertCost ?? 0);
        await this._totalCost?.setValueAsync(total ? `${Math.round(100 * total) / 100} 秒 (${Math.round(100 * result.length / total) / 100}x)` : "無法計算");
        await this._totalCost?.showAsync();

        await this._transferCost?.setValueAsync(result.transferCost ? `${Math.round(100 * result.transferCost) / 100} 秒` : "無法計算");
        await this._transferCost?.showAsync();

        await this._convertCost?.setValueAsync(result.convertCost ? `${Math.round(100 * result.convertCost) / 100} 秒 (${Math.round(100 * result.length / result.convertCost) / 100}x)` : "無法計算");
        await this._convertCost?.showAsync();

        await this._output?.setValueAsync(result.output ?? "");
        await this._output?.showAsync();
    }

    async clearResultAsync(): Promise<void> {
        await this._resultTitle?.hideAsync();
        await this._success?.hideAsync();
        await this._start?.hideAsync();
        await this._fileSource?.hideAsync();
        await this._pullRawFile?.hideAsync();
        await this._savedRawFile?.hideAsync();
        await this._startedConvert?.hideAsync();
        await this._completedConvert?.hideAsync();
        await this._length?.hideAsync();
        await this._totalCost?.hideAsync();
        await this._transferCost?.hideAsync();
        await this._convertCost?.hideAsync();
        await this._output?.hideAsync();
        await this._error?.hideAsync();
    }

    async setErrorAsync(message: string): Promise<void> {
        await this._error?.showAsync();
        await this._error?.setValueAsync(message);
    }
}