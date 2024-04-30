<script lang="ts">
    import Thread from "../component/Thread.svelte";
    import type { ThreadModel } from "../data/data";
    import { Utility } from "../data/utility";
    import PostUploader from "../component/PostUploader.svelte";
	import { router } from 'tinro';


    export let board: string;
    export let threadId: number;

    let thread: ThreadModel = null;
    let errorOccurred: Boolean = false;

    let isRefreshing: Boolean = false;
    let hasLoadedSuccessfullyOnce: Boolean = false;

    async function FetchThread() {
        try {
            thread = <ThreadModel>(await Utility.FetchData(`/${board}/thread/${threadId}`));
            errorOccurred = false;

            hasLoadedSuccessfullyOnce = true;
        }
        catch {
            errorOccurred = true;
        }
    }

    async function Refresh(e) {
        e.preventDefault();
        isRefreshing = true;
        await FetchThread();
        isRefreshing = false;
        return false;
    }

    FetchThread();
</script>

<div>
    {#if errorOccurred}
        <div class="alert alert-warning" role="alert">
            Couldn't load the thread.
        </div>
    {:else if thread === null}
        <div class="ml-2 spinner-border spinner-border-sm" role="status">
            <span class="sr-only">Loading...</span>
        </div>
        <span>Loading...</span>
    {:else}
        <Thread {thread} jumpToHash={true} />

        <div class="my-3">
            [<a href={`/board/${board}`}>Return</a>]
            [<a href="#top">Top</a>]
            [<a href="#" on:click={Refresh}>Update</a>]

            {#if isRefreshing}
                <div class="ml-2 spinner-border spinner-border-sm" role="status">
                    <span class="sr-only">Loading...</span>
                </div>
                <span>Updating...</span>
            {/if}
            
            <div class="float-right" title="Post Count / File Count">
                [{thread.posts.length.toLocaleString()}
                /
                {thread.posts.map(t => t.files.length).reduce((a, b) => a + b, 0).toLocaleString()}]
            </div>
        </div>

        {#if thread.board.isReadOnly === false && thread.archived === false}
            <PostUploader
                isThreadUploader={false}
                board={board}
                threadId={threadId}
                on:success={() => Refresh()} />
        {/if}
    {/if}
</div>
