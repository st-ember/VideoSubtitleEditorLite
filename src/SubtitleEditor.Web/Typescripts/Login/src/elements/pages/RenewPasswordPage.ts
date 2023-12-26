import { pageController } from "controllers/PageController";
import { Page } from "uform-page";

export default class RenewPasswordPage extends Page {
    
    paths: string[] = ["/RenewPassword", "/Account/RenewPassword"];

    protected async loadAsync(): Promise<void> {
        pageController.onIdleTickingFuncs.push(idleTick => {
            if (idleTick > 10) {
                window.location.href = "/Login";
            }
        });
    }
}