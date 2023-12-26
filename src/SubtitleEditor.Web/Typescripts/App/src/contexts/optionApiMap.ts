import { Api } from "uform-api";

export const optionApiMap = {
    list: <Api>{ url: "/Option/ListOption", method: "GET" },
    update: <Api>{ url: "/Option/Update", method: "POST" }
}