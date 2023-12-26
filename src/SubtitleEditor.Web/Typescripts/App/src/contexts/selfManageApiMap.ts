import { Api } from "uform-api";

export const selfManageApiMap = {
    getSelfModifyData: <Api>{ url: "/SelfManage/GetSelfModifyData", method: "GET" },
    getSelfModifyGroupData: <Api>{ url: "/SelfManage/GetSelfModifyGroupData", method: "GET" },
    selfUpdateUser: <Api>{ url: "/SelfManage/SelfUpdateUser", method: "POST" },
    selfUpdateUserPassword: <Api>{ url: "/SelfManage/SelfUpdateUserPassword", method: "POST" },
}