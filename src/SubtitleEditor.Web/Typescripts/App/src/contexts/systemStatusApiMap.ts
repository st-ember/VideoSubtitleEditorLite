import { Api } from "uform-api";

export const systemStatusApiMap = {
    getStatus: <Api>{ url: "/Status/GetStatus", method: "GET" },
    setActivationKey: <Api>{ url: "/Status/SetActivationKey", method: "POST" },
    clearActivationKey: <Api>{ url: "/Status/ClearActivationKey", method: "POST" },
    getAsrAccess: <Api>{ url: "/Topic/GetAsrAccess", method: "GET" }
};