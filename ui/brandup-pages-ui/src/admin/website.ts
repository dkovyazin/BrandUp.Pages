﻿import { UIElement, DOM } from "brandup-ui";
import Page from "../pages/page";
import { listPageCollection } from "../dialogs/page-collection-list";
import ContentPage from "../pages/content";

export class WebSiteToolbar extends UIElement {
    get typeName(): string { return "BrandUpPages.WebSiteToolbar"; }

    constructor(page: Page<any>) {
        super();

        var toolbarElem = DOM.tag("div", { class: "brandup-pages-elem brandup-pages-toolbar" }, [
            DOM.tag("button", { class: "brandup-pages-toolbar-button list", "data-command": "brandup-pages-collections" })
        ]);
        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);
        
        this.registerCommand("brandup-pages-collections", () => {
            let parentPageId: string = null;
            if (page instanceof ContentPage)
                parentPageId = (<ContentPage>page).model.parentPageId;
            listPageCollection(parentPageId);
        });
    }

    destroy() {
        this.element.remove();

        super.destroy();
    }
}