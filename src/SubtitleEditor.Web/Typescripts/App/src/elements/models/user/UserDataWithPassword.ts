import UserData from "./UserData";

export default interface UserDataWithPassword extends UserData {
    editPassword: boolean;
    password?: string;
    confirm?: string;
    userGroups: string[];
}