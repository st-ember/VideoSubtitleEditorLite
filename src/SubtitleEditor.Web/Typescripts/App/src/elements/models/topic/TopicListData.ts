
export default interface TopicListData {
    id: string;
    name: string;
    extension: string;
    asrTaskId?: number;
    originalSize: number;
    size: number;
    length: number;
    lengthText: string;
    processTime: number;
    processTimeText: string;
    topicStatus: number;
    topicStatusText: string;
    // mediaStatus: number;
    // mediaStatusText: string;
    asrMediaStatus: number;
    convertMediaStatus: number;
    asrMediaStatusText: string;
    convertMediaStatusText: string;
    mediaError?: string;
    progress: number;
    creatorId: string;
    create: string;
    update: string;
}