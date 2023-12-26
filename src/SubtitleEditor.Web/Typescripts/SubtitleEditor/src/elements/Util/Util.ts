import { LineData } from "../Editor/models";

export default class Util {

    static DEFAULT_TIME = "0:00:00.000";

    static parseTime(text: string): number {
        const startArray = (text ?? Util.DEFAULT_TIME).split(":").map(o => o.indexOf("NaN") >= 0 ? "0" : o);
        return Number(startArray[0]) * 3600 + Number(startArray[1]) * 60 + Number(startArray[2]);
    }

    static formatTime(time: number): string {
        if (time <= 0) { return Util.DEFAULT_TIME; }
        const hours = Math.floor(time / 3600);
        const minutes = Math.floor((time - 3600 * hours) / 60);
        const seconds = Math.round((time - 3600 * hours - 60 * minutes) * 1000) / 1000;
        const adoptedSeconds = Math.round(seconds * 1000) / 1000;
        const fSecs = Math.round(adoptedSeconds % 1 * 1000);

        const hourText = hours >= 10 ? String(hours) : `0${hours}`;
        const minuteText = minutes >= 10 ? String(minutes) : `0${minutes}`;
        const secondText = adoptedSeconds >= 10 ? String(Math.floor(adoptedSeconds)) : `0${Math.floor(adoptedSeconds)}`;
        const fSecText = fSecs >= 100 ? String(fSecs) : (fSecs >= 10 ? `0${fSecs}` : `00${fSecs}`);
        return `${hourText}:${minuteText}:${secondText}.${fSecText}`;
    }

    static isValidTime(text: string | number): boolean {
        if (typeof text === "number") { return true; }

        const array = text ? text.split(":").map(item => Number(item)) : [];
        if (array.length !== 3 || array.filter(item => isNaN(item)).length > 0) {
            return false;
        }
        return array[0] >= 0 && array[1] < 60 && array[1] >= 0 && array[2] < 60 && array[2] >= 0;
    }

    static parseSrt(text: string): LineData[] {
        return text.split('\n\n').filter(o => !!o).map(o => {
            const array = o.split('\n');
            const timeLine = array[1];
            const timeArray = timeLine.split(' --> ');
            const content = array[2].startsWith("- ") ? array[2].substring(2) : array[2];
            return { start: timeArray[0].replace(',', '.'), end: timeArray[1].replace(',', '.'), content }
        });
    }

    static convertToSrt(lineDatas: LineData[]): string {
        return lineDatas.map((lineData, index) =>
            `${index + 1}\n${lineData.start.replace(".", ",")} --> ${lineData.end.replace(".", ",")}\n- ${lineData.content}`
        ).join("\n\n");
    }

    private static _htmlEntityTranslateMap: { [key: string]: string; } = {
        "nbsp": " ",
        "amp": "&",
        "quot": "\"",
        "lt": "<",
        "gt": ">"
    };

    static replaceHtmlEntities(text: string): string {
        return text.replace(/\n*/g, "").replace(/&(nbsp|amp|quot|lt|gt);/g, (_, entity) => Util._htmlEntityTranslateMap[entity]);
    }
}
