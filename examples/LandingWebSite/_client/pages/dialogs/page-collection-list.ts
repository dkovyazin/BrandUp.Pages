﻿import { DialogOptions } from "./dialog";
import { createPageCollection } from "./page-collection-create";
import { deletePageCollection } from "./page-collection-delete";
import { updatePageCollection } from "./page-collection-update";
import { ListDialog } from "./dialog-list";
import { listPage } from "./page-list";
import { DOM } from "brandup-ui";

export class PageCollectionListDialog extends ListDialog<PageCollectionListModel, PageCollectionModel> {
    readonly pageId: string;
    private __isModified: boolean = false;
    private navElem: HTMLElement;

    constructor(pageId: string, options?: DialogOptions) {
        super(options);

        this.pageId = pageId;
    }

    get typeName(): string { return "BrandUpPages.PageCollectionListDialog"; }

    protected _onRenderContent() {
        super._onRenderContent();
        
        this.registerCommand("item-create", () => {
            createPageCollection(this.pageId).then((createdItem: PageCollectionModel) => {
                this.loadItems();
                this.__isModified = true;
            });
        });
        this.registerItemCommand("item-update", (pageCollectionId: string, el: HTMLElement) => {
            updatePageCollection(pageCollectionId).then((updatedItem: PageCollectionModel) => {
                this.loadItems();
                this.__isModified = true;
            });
        });
        this.registerItemCommand("item-delete", (pageCollectionId: string, el: HTMLElement) => {
            deletePageCollection(pageCollectionId).then((deletedItem: PageCollectionModel) => {
                this.loadItems();
                this.__isModified = true;
            });
        });
        this.registerItemCommand("page-list", (pageCollectionId: string) => {
            listPage(pageCollectionId);
        });
    }

    protected _onClose() {
        if (this.__isModified)
            this.resolve(null);
        else
            super._onClose();
    }
    
    protected _buildUrl(): string {
        return `/brandup.pages/collection/list`;
    }
    protected _buildUrlParams(urlParams: { [key: string]: string; }) {
        if (this.pageId)
            urlParams["pageId"] = this.pageId;
    }
    protected _buildList(model: PageCollectionListModel) {
        this.setHeader("Коллекции страниц");
        this.setNotes("Просмотр и управление коллекциями страниц.");

        if (!this.navElem) {
            this.navElem = DOM.tag("ol", { class: "nav" })
            this.content.insertAdjacentElement("afterbegin", this.navElem);
        }
        else {
            DOM.empty(this.navElem);
        }

        this.navElem.appendChild(DOM.tag("li", null, DOM.tag("span", null, "root")));
        if (model.parents && model.parents.length) {
            for (let i = 0; i < model.parents.length; i++) {
                this.navElem.appendChild(DOM.tag("li", null, DOM.tag("span", {}, model.parents[i])));
            }
        }
    }
    protected _getItemId(item: PageCollectionModel): string {
        return item.id;
    }
    protected _renderItemContent(item: PageCollectionModel, contentElem: HTMLElement) {
        contentElem.appendChild(DOM.tag("div", { class: "title" }, DOM.tag("span", { }, item.title)));
    }
    protected _renderItemMenu(item: PageCollectionModel, menuElem: HTMLElement) {
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "page-list" }, "View pages")]));
        menuElem.appendChild(DOM.tag("li", { class: "split" }));
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-update" }, "Edit")]));
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-delete" }, "Delete")]));
    }
    protected _renderEmpty(container: HTMLElement) {
        container.innerText = "Коллекций не создано.";
    }
    protected _renderNewItem(containerElem: HTMLElement) {
        containerElem.appendChild(DOM.tag("a", { href: "", "data-command": "item-create" }, "Создать коллекцию страниц"));
    }
}

interface PageCollectionListModel {
    parents: Array<string>;
}

export var listPageCollection = (pageId: string) => {
    let dialog = new PageCollectionListDialog(pageId);
    return dialog.open();
};