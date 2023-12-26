import UserGroupListData from "./UserGroupListData";

export default interface UserGroupListResponse {
    list: UserGroupListData[];
    totalCount: number;
    totalPage: number;
    page: number;
}