<script lang="ts">
	import { onMount } from "svelte";
	import type { ThreadModel, PostModel } from "../data/data";
	import BanUserModal from "./modal/BanUserModal.svelte";
	import DeletePostModal from "./modal/DeletePostModal.svelte";
	import ReportModal from "./modal/ReportModal.svelte";
	import Post from "./Post.svelte";
	import { Utility } from "../data/utility";

	export let thread: ThreadModel;
	export let fullThread: ThreadModel;
	export let jumpToHash: boolean = false;

	export let expanded: boolean = false;
	export let expanding: boolean = false;

	let banUserModal: BanUserModal;
	let deletePostModal: DeletePostModal;
	let reportModal: ReportModal;

	function postAction(
		e: CustomEvent<{ action: string; boardId: number; postId: number }>,
	) {
		if (e.detail.action === "ban-ip") {
			banUserModal.showModal(e.detail.boardId, e.detail.postId);
		} else if (e.detail.action === "delete-post") {
			deletePostModal.showModal(e.detail.boardId, e.detail.postId);
		} else if (e.detail.action === "report") {
			reportModal.showModal(e.detail.boardId, e.detail.postId);
		}
	}

	function calculateBackquotes(post: PostModel): number[] {
		return (fullThread ?? thread).posts
			.filter((x) => {
				return (
					(x.contentHtml &&
						x.contentHtml.indexOf(`>&gt;&gt;${post.postId}</a>`) >= 0) ||
					(x.contentRaw &&
						x.contentRaw.indexOf(`>>${post.postId}`) >= 0)
				);
			})
			.map((x) => x.postId);
	}

	async function toggleExpand() {
		if (expanding) {
			return;
		}
		if (expanded) {
			expanded = false;
			return;
		}
		if (fullThread != null) {
			expanded = true;
			return;
		}
		expanding = true;
        try {
            fullThread = <ThreadModel>(await Utility.FetchData(`/${thread.board.shortName}/thread/${thread.threadId}`));
            
			expanding = false;
			expanded = true;
        }
        catch {
            expanding = false;
        }
	}

	onMount(() => {
		if (jumpToHash) {
			window.location.hash = window.location.hash;
		}
	});
</script>

<div class="thread">
	{#each (expanded ? fullThread : thread).posts as post, index (post.postId)}
		<div class:reply-margin={index !== 0} class:op={index === 0}>
			<Post
				{post}
				threadId={thread.threadId}
				board={thread.board}
				subject={index === 0 ? thread.subject : null}
				backquotes={calculateBackquotes(post)}
				on:postaction={postAction}
			/>
			{#if thread.omitted && index === 0}
				<div class="omitted" class:expanding={expanding}>
					{#if expanding}
						<div class="spinner-border spinner-border-sm expand-spinner" role="status">
							<span class="sr-only">Loading...</span>
						</div>{/if}<i class="fa-solid expand"
						class:fa-up-right-and-down-left-from-center={!expanded}
						class:fa-down-left-and-up-right-to-center={expanded}
						role="button" tabindex="-1" on:click={toggleExpand}></i>
					{#if expanded}
						{fullThread.posts.length.toLocaleString()} posts displayed
					{:else}
						{thread.omitted.toLocaleString()} {thread.omitted === 1 ? 'post' : 'posts'} omitted
					{/if}
				</div>
			{/if}
		</div>
	{/each}
</div>

<BanUserModal bind:this={banUserModal} />
<DeletePostModal bind:this={deletePostModal} />
<ReportModal bind:this={reportModal} />

<style>
	/* .reply-margin {
		margin-left: 25px;
	} */

	.omitted {
		font-weight: 700;
		font-size: 14px;
		white-space: pre;
		position: relative;
	}

	.expand {
		font-size: 11px;
		margin: 5px;
		color: var(--link-text-color);
		cursor: pointer;
	}

	.expanding .expand {
		opacity: 0.5;
		cursor: initial;
	}

	.expand-spinner {
		color: var(--link-text-color);
		position: absolute;
		left: -0.75rem;
		width: 0.75rem;
		height: 0.75rem;
		top: 0.25rem;
	}
</style>
