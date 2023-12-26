import UserListData from "./UserListData";

export default interface UserListResponse {
    list: UserListData[];
    totalCount: number;
    totalPage: number;
    page: number;
}