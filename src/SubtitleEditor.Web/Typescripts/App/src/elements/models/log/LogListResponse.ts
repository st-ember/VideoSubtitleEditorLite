import LogListPrimaryDataModel from "./LogListPrimaryDataModel";

export default interface LogListResponse {
    list: LogListPrimaryDataModel[];
    totalCount: number;
    totalPage: number;
    page: number;
}