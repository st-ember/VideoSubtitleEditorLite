
export default interface LogListCondition {
    start?: string;
    end?: string;
    actions?: string;
    target?: string;
    ipAddress?: string;
    user?: string;
    isActionSuccess?: boolean;

    page: number;
    pageSize: number;
    orderColumn: string;
    descending: boolean;
}