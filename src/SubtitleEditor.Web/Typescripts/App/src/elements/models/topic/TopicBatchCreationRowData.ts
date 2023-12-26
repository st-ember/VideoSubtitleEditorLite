
export default interface TopicBatchCreationRowData {
    filename?: string;
    ticket?: string;
    file?: File;
    name: string;
    description?: string;
    createType: number;
    subtitleTicket?: string;
    transcriptTicket?: string;
    frameRate?: number;
    wordLimit?: number;
    modelName?: string;
}