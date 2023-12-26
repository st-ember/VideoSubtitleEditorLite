
class MenuController {

    activeMenu(href: string): void;
    activeMenu(hrefs: string[]): void;
    activeMenu(arg: string | string[]): void {
        const hrefs = arg instanceof Array ? arg : [arg];
        hrefs.forEach(href => {
            const adoptedHref = href.toLowerCase();
            const origin = window.location.origin.toLowerCase();
            const anchorElems = document.querySelectorAll("[role=\"menu\"] a");
            for (let i = 0; i < anchorElems.length; i++) {
                const anchor = <HTMLAnchorElement>anchorElems.item(i);
                if (anchor.href.toLowerCase().replace(origin, "") === adoptedHref) {
                    anchor.classList.add("active");
                    continue;
                }
            }
        });
    }
}

export const menuController = new MenuController();