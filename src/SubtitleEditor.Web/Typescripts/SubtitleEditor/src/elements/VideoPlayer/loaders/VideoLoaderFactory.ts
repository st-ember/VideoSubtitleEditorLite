import { VideoOptions } from "../options";
import { RelativeUrlVideoLoader } from "./RelativeUrlVideoLoader";
import { UrlVideoLoader } from "./UrlVideoLoader";
import VideoLoader from "./VideoLoader";

export default class VideoLoaderFactory {

    private _loaders: VideoLoader[] = [new UrlVideoLoader(), new RelativeUrlVideoLoader()];

    getVideoLoader(options: VideoOptions): VideoLoader | undefined {
        const matchs = this._loaders.filter(loader => loader.isMatch(options));
        return matchs.length > 0 ? matchs[0] : undefined;
    }
}