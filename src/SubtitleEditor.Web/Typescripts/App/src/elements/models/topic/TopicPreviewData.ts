
export default interface TopicPreviewData {
    name: string;
    description?: string;
    filename: string;
    extension: string;
    originalSize: number;
    size: number;
    length: number;
    processTime: number;
    frameRate?: number;
    wordLimit?: number;
    status: number;
    asrTaskId?: number;
    modelName?: string;
    mediaStatus: string;
    error?: string;

    lengthText: string;
    processTimeText: string;
}