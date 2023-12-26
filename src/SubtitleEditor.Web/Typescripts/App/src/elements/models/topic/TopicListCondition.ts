
export default interface TopicListCondition {
    keyword?: string;
    start?: string;
    end?: string;
    topicStatus?: string;
    asrMediaStatus?: string;
    convertMediaStatus?: string;

    page: number;
    pageSize: number;
    orderColumn: string;
    descending: boolean;
}