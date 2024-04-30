import { writable } from "svelte/store"
import { Api } from "./api";
import type { BoardModel, ModeratorRole } from "./data";

export class Exception {
    exceptionObject: any

    constructor(exception: any) {
        this.exceptionObject = exception;
    }
}

export const moderatorUserStore = writable<ModeratorRole | null>(null);
export const boardInfoStore = writable<Promise<BoardModel[]> | null>(null);
export const searchParamStore = writable<Record<string, string> | null>(null);

localStorage.removeItem("hayden_theme");

export const theme = writable(localStorage.getItem("sude_theme") || 'yotsubab4')
theme.subscribe((value) => localStorage.setItem("sude_theme", value))

export async function initStores() {
    Api.GetUserInfoAsync()
        .then(value => moderatorUserStore.set(value.role))
        .catch(reason => moderatorUserStore.set(null));

    boardInfoStore.set(Api.GetBoardInfoAsync());
    // Api.GetBoardInfoAsync()
    //     .then(availableBoardsStore.set)
    //     .catch(reason => moderatorUserStore.set(null));
}
