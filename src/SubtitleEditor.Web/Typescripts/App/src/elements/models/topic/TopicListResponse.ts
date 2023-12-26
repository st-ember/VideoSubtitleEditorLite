import TopicListData from "./TopicListData";

export default interface TopicListResponse {
    list: TopicListData[];
    totalCount: number;
    totalPage: number;
    page: number;
}