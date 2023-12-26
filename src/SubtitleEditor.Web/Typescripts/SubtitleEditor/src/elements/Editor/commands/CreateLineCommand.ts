import { Transcript } from "../components";
import TranscriptSpan from "../components/TranscriptSpan";
import CreateLineHistory from "../histories/CreateLineHistory";

export default class CreateLineCommand {

    static async invokeAsync(transcript: Transcript, spans: TranscriptSpan[]): Promise<void> {

        const line = transcript.bindSpan(spans);
        transcript.subtitle.addHistoryAsync(new CreateLineHistory(transcript.subtitle, 
        { 
            startSpanIndex: spans[0].index, 
            endSpanIndex: spans[spans.length - 1].index, 
            start: line.startText, 
            end: line.endText, 
            content: line.content
        }));
    }
}

