
export default interface BenchmarkResult {
    success?: boolean;
    output?: string;
    convertArgument?: string;
    length: number;
    transferCost?: number;
    convertCost?: number;
    start: string;
    pullRawFileFromAsr?: string;
    pullRawFileFromLocal?: string;
    savedRawFile?: string;
    startedConvert?: string;
    completedConvert?: string;
}