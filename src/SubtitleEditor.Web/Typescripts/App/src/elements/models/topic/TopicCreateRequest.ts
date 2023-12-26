
export default interface TopicCreateRequest {
    filename: string;
    ticket: string;
    name: string;
    description?: string;
    createType: number;
    subtitleTicket?: string;
    transcriptTicket?: string;
    frameRate?: number;
    wordLimit?: number;
    modelName?: string;
}