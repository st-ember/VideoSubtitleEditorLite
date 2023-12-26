import LoginPage from "elements/pages/LoginPage";
import RenewPasswordPage from "elements/pages/RenewPasswordPage";
import { Page } from "uform-page";

export const pageMap: { [key: string]: () => Page } = {
    "Login": () => new LoginPage(),
    "RenewPassword": () => new RenewPasswordPage()
}