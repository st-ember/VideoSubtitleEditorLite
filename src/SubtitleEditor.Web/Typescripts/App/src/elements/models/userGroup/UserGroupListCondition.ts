
export default interface UserGroupListCondition {
    keyword?: string;

    page: number;
    pageSize: number;
    orderColumn: string;
    descending: boolean;
}