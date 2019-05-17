﻿import { UIElement, DOM } from "brandup-ui";
import ContentPage from "../pages/content";
import { editPage } from "../dialogs/page-edit";
import { publishPage } from "../dialogs/page-publish";
import iconDiscard from "../svg/toolbar-button-discard.svg";
import iconEdit from "../svg/toolbar-button-edit.svg";
import iconPublish from "../svg/toolbar-button-publish.svg";
import iconSave from "../svg/toolbar-button-save.svg";
import iconSettings from "../svg/toolbar-button-settings.svg";

export class PageToolbar extends UIElement {
    get typeName(): string { return "BrandUpPages.PageToolbar"; }

    constructor(page: ContentPage) {
        super();

        var toolbarElem = DOM.tag("div", { class: "brandup-pages-elem brandup-pages-toolbar brandup-pages-toolbar-right" });
        let isLoading = false;

        if (page.model.editId) {
            toolbarElem.appendChild(DOM.tag("button", { class: "brandup-pages-toolbar-button", "data-command": "brandup-pages-content" }, iconSettings));
            toolbarElem.appendChild(DOM.tag("button", { class: "brandup-pages-toolbar-button", "data-command": "brandup-pages-commit" }, iconSave));
            toolbarElem.appendChild(DOM.tag("button", { class: "brandup-pages-toolbar-button", "data-command": "brandup-pages-discard" }, iconDiscard));
            
            this.registerCommand("brandup-pages-content", () => {
                editPage(page.model.editId).then(() => {
                    page.app.reload();
                });
            });
            this.registerCommand("brandup-pages-commit", (elem: HTMLElement) => {
                if (isLoading)
                    return;
                isLoading = true;

                page.app.request({
                    urlParams: { handler: "CommitEdit" },
                    method: "POST",
                    success: (data: string, status: number) => {
                        page.app.nav({ url: data, pushState: false });
                        isLoading = false;
                    }
                });
            });
            this.registerCommand("brandup-pages-discard", (elem: HTMLElement) => {
                if (isLoading)
                    return;
                isLoading = true;

                page.app.request({
                    urlParams: { handler: "DiscardEdit" },
                    method: "POST",
                    success: (data: string, status: number) => {
                        page.app.nav({ url: data, pushState: false });
                        isLoading = false;
                    }
                });
            });
        }
        else {
            if (page.model.status !== "Published") {
                toolbarElem.appendChild(DOM.tag("button", { class: "brandup-pages-toolbar-button", "data-command": "brandup-pages-publish" }, iconPublish));

                this.registerCommand("brandup-pages-publish", () => {
                    publishPage(page.model.id).then(result => {
                        page.app.nav({ url: result.url, pushState: false });
                    });
                });
            }

            toolbarElem.appendChild(DOM.tag("button", { class: "brandup-pages-toolbar-button", "data-command": "brandup-pages-edit" }, iconEdit));
            this.registerCommand("brandup-pages-edit", () => {
                if (isLoading)
                    return;
                isLoading = true;

                page.app.request({
                    urlParams: { handler: "BeginEdit" },
                    method: "POST",
                    success: (data: string, status: number) => {
                        isLoading = false;
                        page.app.nav({ url: data, pushState: false });
                    }
                });
            });
        }
        
        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);
    }

    destroy() {
        this.element.remove();

        super.destroy();
    }
}