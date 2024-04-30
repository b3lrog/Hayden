<script lang="ts">
    import { onMount } from "svelte";
    import PageSelector from "../component/PageSelector.svelte";
    import PostUploader from "../component/PostUploader.svelte";
    import Thread from "../component/Thread.svelte";
    import { Api } from "../data/api";
    import type { BoardPageModel } from "../data/data";
	import { router } from 'tinro';

    let dataPromise: Promise<BoardPageModel>;

    export let initialCurrentPage = 1;
    export let board: string;

    let maxPage = initialCurrentPage;

    async function navigatePage(page: number) {
        let newUrl;

        if (page === 1) {
            newUrl = `/board/${board}/`;
        }
        else {
            newUrl = `/board/${board}/page/${page}/`;
        }

        router.goto(newUrl);
    }

    async function loadData() {
        dataPromise = Api.GetBoardPage(board, initialCurrentPage);

        var model = await dataPromise;

        maxPage = Math.ceil(model.totalThreadCount / 10);
    }

    loadData();

</script>

<style>
    .board-title {
        font-size: 24px;
        text-align: center;
        margin-bottom: 10px;
        font-weight: 700;
    }
</style>

{#await dataPromise}
    <div class="ml-2 spinner-border spinner-border-sm" role="status">
        <span class="sr-only">Loading...</span>
    </div>
    <span>Loading...</span>
{:then data}

    <div class="board-title">
        /{data.boardInfo.shortName}/ - {data.boardInfo.longName}
    </div>

    {#if data.boardInfo.isReadOnly === false}
        <PostUploader isThreadUploader={true} board={board} />
    {/if}

    {#each data.threads as thread, index ([thread.board.id, thread.threadId]) }
    
        <!-- {#if index > 0}<br/>{/if} -->
        <hr/>

        <Thread {thread} />
    {/each}

    <PageSelector currentPage={initialCurrentPage} {maxPage} on:page={event => navigatePage(event.detail.page)} />
{:catch}
    <div class="alert alert-warning" role="alert">
        Couldn't load the board.
    </div>
{/await}
