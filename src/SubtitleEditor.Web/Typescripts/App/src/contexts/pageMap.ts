import EditorPage from "elements/pages/EditorPage";
import FixBookPage from "elements/pages/FixBookPage";
import LogListPage from "elements/pages/LogListPage";
import OptionListPage from "elements/pages/OptionListPage";
import StatusPage from "elements/pages/StatusPage";
import TopicBatchCreationPage from "elements/pages/TopicBatchCreationPage";
import TopicListPage from "elements/pages/TopicListPage";
import UserGroupListPage from "elements/pages/UserGroupListPage";
import UserListPage from "elements/pages/UserListPage";
import { Page } from "uform-page";

export const pageMap: { [key: string]: () => Page } = {
    "Editor": () => new EditorPage(),
    "TopicList": () => new TopicListPage(),
    "TopicBatchCreation": () => new TopicBatchCreationPage(),
    "LogList": () => new LogListPage(),
    "UserList": () => new UserListPage(),
    "UserGroupList": () => new UserGroupListPage(),
    "OptionList": () => new OptionListPage(),
    "FixBook": () => new FixBookPage(),
    "Status": () => new StatusPage()
}