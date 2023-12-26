import { Api } from "uform-api";

export const fileApiMap = {
    upload: <Api>{ url: "/File/UploadToCache", method: "POST" },
    uploadToStorage: <Api>{ url: "/File/Upload", method: "POST" }
};