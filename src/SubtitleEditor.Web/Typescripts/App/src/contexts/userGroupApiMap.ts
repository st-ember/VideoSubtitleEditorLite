import { Api } from "uform-api";

export const userGroupApiMap = {
    listAsOption: <Api>{ url: "/UserGroup/ListAsOption", method: "GET" },
    listGroupTypeAsOption: <Api>{ url: "/UserGroup/ListGroupTypeAsOption", method: "GET" },
    listPermissionAsOption: <Api>{ url: "/UserGroup/ListPermissionAsOption", method: "GET" },
    listByUser: <Api>{ url: "/UserGroup/ListByUser", method: "GET" },
    get: <Api>{ url: "/UserGroup/Get", method: "GET" },
    list: <Api>{ url: "/UserGroup/ListUserGroup", method: "GET" },
    create: <Api>{ url: "/UserGroup/Create", method: "POST" },
    update: <Api>{ url: "/UserGroup/Update", method: "POST" },
    duplicate: <Api>{ url: "/UserGroup/Duplicate", method: "POST" },
    delete: <Api>{ url: "/UserGroup/Delete", method: "POST" }
}