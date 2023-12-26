import { Api } from "uform-api";

export const userMetaApiMap = {
    getKeybinding: <Api>{ url: "/UserMeta/GetKeybinding", method: "GET" },
    saveKeybinding: <Api>{ url: "/UserMeta/SaveKeybinding", method: "POST" },
    recoverKeybinding: <Api>{ url: "/UserMeta/RecoverKeybinding", method: "POST" },
    getSelfOptions: <Api>{ url: "/UserMeta/GetSelfOptions", method: "GET" },
    saveSelfOptions: <Api>{ url: "/UserMeta/SaveSelfOptions", method: "POST" },
};