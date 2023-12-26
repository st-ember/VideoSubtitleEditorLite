import { Subtitle } from "../components";
import InsertLineHistory from "../histories/InsertLineHistory";

export default class InsertLineCommand {

    static async invokeAsync(subtitle: Subtitle, index: number): Promise<void> {
        await subtitle.insertAsync(index);
        subtitle.addHistoryAsync(new InsertLineHistory(subtitle, { index: index + 1 }));
    }
}
