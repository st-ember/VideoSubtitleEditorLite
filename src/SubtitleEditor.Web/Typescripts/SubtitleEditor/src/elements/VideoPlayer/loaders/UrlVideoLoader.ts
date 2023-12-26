import { VideoState } from "../models";
import VideoOptions from "../options/VideoOptions";
import VideoLoader from "./VideoLoader";


export class UrlVideoLoader implements VideoLoader {

    isMatch(options: VideoOptions): boolean {
        return typeof options.source === "string" && options.source.startsWith("http");
    }

    async loadAsync(options: VideoOptions): Promise<VideoState> {
        return this.loadFromUrl(<string>options.source, options.name);
    }

    protected loadFromUrl(url: string, name: string): VideoState {
        const state: VideoState = { mode: "url", state: "checking", name, url, type: "application/x-mpegURL" };

        try {
            this.checkVideoFileAsync(url)
                .then(result => {
                    if (!result) {
                        state.state = "checked";
                        state.error = undefined;
                    } else {
                        state.state = "error";
                        state.error = result;
                    }

                    if (state.onCompleted) {
                        state.onCompleted();
                    }
                });
        } catch (error) {
            state.state = "error";
            state.error = <string>error;
            
            if (state.onCompleted) {
                state.onCompleted();
            }
        }

        return state;
    }

    protected checkVideoFileAsync(url: string): Promise<string | undefined> {
        return new Promise<string | undefined>(resolve => {
            const request = new XMLHttpRequest();
            request.open("GET", url, true);
            request.onreadystatechange = function () {
                if (request.readyState === 4 && request.status === 200) {
                    resolve(undefined);
                } else if (request.readyState === 4 && request.status !== 200) {
                    resolve(request.status === 404 ? "找不到檔案" : request.status === 400 ? "ID 錯誤" : "內部錯誤");
                }
            };
            request.send();
        });
    }
}