
export default interface TopicUpdateRequest {
    id: string;
    name: string;
    description?: string;
    frameRate?: number;
    wordLimit?: number;
    modelName?: string;
}