import { pageMap } from "contexts/pageMap";
import { permissionController } from "controllers/PermissionController";
import SelfManagerController from "controllers/SelfManagerController";
import sessionController from "controllers/SessionController";
import { PageController } from "uform-page";

(async () => {
    await permissionController.initAsync();

    sessionController.startAsync();

    const pageController = new PageController();
    pageController.pageMap = pageMap;
    await pageController.loadAsync();

    const selfManageController = new SelfManagerController();
    selfManageController.init(<HTMLDivElement>document.getElementById("self-manage")!);
})();