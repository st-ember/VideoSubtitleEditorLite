import LogListDataModel from "./LogListDataModel";

export default interface LogListPrimaryDataModel {
    id?: string;
    actionId?: string;
    actionText?: string;
    userId?: string;
    userAccount?: string;
    ipAddress?: string;
    request?: string;
    response?: string;
    time?: string;
    success: boolean;
    target?: string;
    field?: string;
    before?: string;
    after?: string;
    message?: string;
    code?: string;
    exception?: string;
    innerException?: string;
    actionName?: string;
    actionMessage?: string;
    children: LogListDataModel[];
}