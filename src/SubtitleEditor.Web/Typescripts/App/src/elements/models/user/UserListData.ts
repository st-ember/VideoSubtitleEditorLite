
export default interface UserListData {
    id: string;
    account: string;
    name?: string;
    title?: string;
    telephone?: string;
    email?: string;
    description?: string;
    userGroups: string[];
    status: number;
    create: string;
    update: string;
}