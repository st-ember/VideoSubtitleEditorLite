
export default interface UserListCondition {
    keyword?: string;
    status?: string;

    page: number;
    pageSize: number;
    orderColumn: string;
    descending: boolean;
}