import { VideoState } from "../models";
import { VideoOptions } from "../options";
import { UrlVideoLoader } from "./UrlVideoLoader";

export class RelativeUrlVideoLoader extends UrlVideoLoader {

    isMatch(options: VideoOptions): boolean {
        return typeof options.source === "string" && (options.source.startsWith("./") || options.source.startsWith("/"));
    }

    async loadAsync(options: VideoOptions): Promise<VideoState> {
        const source = <string>options.source;
        const url = `${window.origin}${source.startsWith("./") ? source.substring(1) : source}`;
        const state = this.loadFromUrl(url, options.name);
        state.mode = "relative";
        return state;
    }
}