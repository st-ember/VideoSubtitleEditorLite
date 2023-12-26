import { apiController } from "ApiController";
import { permissionApiMap } from "contexts/permissionApiMap";
import PermissionData from "elements/models/permission/PermissionData";

class PermissionController implements PermissionData {

    isSystemAdmin: boolean = false;
    actions: string[] = [];

    async initAsync(): Promise<void> {
        const response = await apiController.queryAsync(permissionApiMap.get);
        if (response.success) {
            this.isSystemAdmin = response.data.isSystemAdmin;
            this.actions = response.data.actions;
            console.log(response.data)
        } else {
            window.location.href = "/Account/Login";
        }
    }

    contains(action: string): boolean {
        return this.isSystemAdmin || this.actions.indexOf(action) >= 0;
    }

    containsAny(actions: string[]): boolean {
        return this.isSystemAdmin || actions.filter(o => this.actions.indexOf(o) >= 0).length > 0;
    }

    containsAll(actions: string[]): boolean {
        return this.isSystemAdmin || actions.filter(o => this.actions.indexOf(o) >= 0).length === actions.length;
    }
}

export const permissionController = new PermissionController();