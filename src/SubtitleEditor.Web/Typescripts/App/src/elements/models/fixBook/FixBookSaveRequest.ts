import FixBookItem from "./FixBookItem";

export default interface FixBookSaveRequest {
    modelName: string;
    fixBookItems: FixBookItem[];
}