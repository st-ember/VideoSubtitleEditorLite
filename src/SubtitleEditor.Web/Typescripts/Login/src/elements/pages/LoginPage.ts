import { loginApiMap } from "contexts/loginApiMap";
import { pageController } from "controllers/PageController";
import apiController from "uform-api";
import { Page } from "uform-page";
import Utility from "uform-utility";

export default class LoginPage extends Page {

    paths: string[] = ["/", "/Login", "/Account/Login"];

    loginForm: HTMLFormElement = undefined!;
    accountInput: HTMLInputElement = undefined!;
    passwordInput: HTMLInputElement = undefined!;
    captchaImageContainer: HTMLDivElement = undefined!;
    captchaImage: HTMLImageElement = undefined!;
    nextImageButton: HTMLButtonElement = undefined!;
    captchaInput: HTMLInputElement = undefined!;
    loginButton: HTMLButtonElement = undefined!;

    accountInputUpdating: boolean = false;
    spans: HTMLSpanElement[] = [];
    currentAccount: string = "";

    protected async loadAsync(): Promise<void> {
        this.loginButton = <HTMLButtonElement>document.getElementById("login-button");
        
        this.loginForm = <HTMLFormElement>document.getElementById("login-form");
        this.accountInput = <HTMLInputElement>document.getElementById("account-input");
        this.passwordInput = <HTMLInputElement>document.getElementById("password-input");
        this.nextImageButton = <HTMLButtonElement>document.getElementById("next-image-button");
        this.captchaImageContainer = <HTMLDivElement>this.nextImageButton.parentElement;
        this.captchaInput = <HTMLInputElement>document.getElementById("captcha-input");

        if (this.captchaInput) {
            this.captchaInput.value = "";
        }

        if (this.nextImageButton) {
            this.nextImageButton.disabled = true;
            this.nextImageButton.addEventListener("click", async () => {
                this.nextImageButton.disabled = true;
                const image = await this._getCaptchaImageAsync();
                if (image) {
                    await this._buildCaptchaImageAsync(image);
                }
                
                await Utility.wait(1000);
                this.nextImageButton.disabled = false;
            });

            Utility.wait(1000).then(() => this.nextImageButton.disabled = false);
        }

        const image = await this._getCaptchaImageAsync();
        if (image) {
            await this._buildCaptchaImageAsync(image);
        }

        pageController.onIdleTickingFuncs.push(idleTick => {
            if (idleTick > 10) {
                this.reload();
            }
        });
    }

    private async _getCaptchaImageAsync(): Promise<string | undefined> {
        const response = await apiController.queryAsync(loginApiMap.getCaptchaImage, { i: String(Number(new Date())) });
        if (response.success) {
            return response.data;
        } else {
            alert("取得驗證碼圖片失敗！");
            return undefined;
        }
    }

    private async _buildCaptchaImageAsync(image: string): Promise<void> {
        if (this.captchaImage) {
            this.captchaImage.remove();
        }

        const imageElement = document.createElement("img");
        imageElement.src = `data:image/png;base64, ${image}`;
        imageElement.id = "captcha-image";
        this.captchaImageContainer.insertBefore(imageElement, this.captchaImageContainer.firstChild);
        this.captchaImage = imageElement;
    }
}