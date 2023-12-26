import { Api } from "uform-api";

export const fixBookApiMap = {
    get: <Api>{ url: "/FixBook/Get", method: "GET" },
    save: <Api>{ url: "/FixBook/Save", method: "POST" }
}