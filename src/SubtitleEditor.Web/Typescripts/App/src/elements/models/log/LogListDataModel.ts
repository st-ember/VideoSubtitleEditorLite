
export default interface LogListDataModel {
    id?: string;
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
    actionMessage?: string;
}