import FixBookItem from "./FixBookItem";

export default interface FixBookModel {
    modelName: string;
    maxFixbookSize: number;
    items: FixBookItem[];
}