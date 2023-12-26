import { Api } from "uform-api";

export const userApiMap = {
    list: <Api>{ url: "/User/ListUser", method: "GET" },
    get: <Api>{ url: "/User/GetUser", method: "GET" },
    isAccountExist: <Api>{ url: "/User/IsAccountExist", method: "GET" },
    listLoginRecord: <Api>{ url: "/User/ListLoginRecord", method: "GET" },
    create: <Api>{ url: "/User/CreateUser", method: "POST" },
    update: <Api>{ url: "/User/UpdateUser", method: "POST" },
    updateStatus: <Api>{ url: "/User/UpdateStatus", method: "POST" },
    updatePassword: <Api>{ url: "/User/UpdatePassword", method: "POST" },
    updateUsersUserGroup: <Api>{ url: "/User/UpdateUsersUserGroup", method: "POST" },
    remove: <Api>{ url: "/User/Remove", method: "POST" },
}