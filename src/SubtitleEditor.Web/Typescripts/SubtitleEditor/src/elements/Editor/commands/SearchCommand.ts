import Editor from "../Editor";
import { Line } from "../components";

export default class SearchCommand {

    static async invokeAsync(editor: Editor): Promise<Line[]> {
        let searchText = "";

        if (editor.editingLine) {
            const selection = window.getSelection();
            if (selection) {
                searchText = selection.toString() ?? "";
            }

            if (editor.searchpanel && searchText) {
                editor.searchpanel.searchText = searchText;
                editor.showSideGroup(true);
            }
        } else if (editor.searchpanel) {
            editor.showSideGroup(true);
        }

        if (editor.searchpanel && editor.searchpanel.searchText) {
            searchText = editor.searchpanel.searchText;
        }

        if (searchText) {
            return await SearchCommand.doSearchAsync(editor, searchText);
        }

        return [];
    }

    static async doSearchAsync(editor: Editor, searchText: string): Promise<Line[]> {
        if (!editor.currentSubtitle) { return []; }

        editor.inSearchMode = true;

        const matchedLines = editor.currentSubtitle.lines.filter(line => {
            return line.searchText(searchText);
        });

        if (editor.searchpanel) {
            editor.searchpanel.setSearchResult(matchedLines);
        }

        return matchedLines;
    }

    static async doClearSearchAsync(editor: Editor): Promise<void> {
        if (!editor.currentSubtitle) { return; }

        editor.inSearchMode = false;
        editor.currentSubtitle.lines.forEach(line => {
            line.clearSearch();
        });

        if (editor.searchpanel) {
            editor.searchpanel.searchText = "";
        }
    }
}