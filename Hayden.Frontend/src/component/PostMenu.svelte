<script lang="ts">
	import { createEventDispatcher } from "svelte";
	import type { PostModel } from "../data/data";

	const dispatch = createEventDispatcher();

	export let boardId: number;
	export let postId: number;
	export let moderator: boolean;
	export let originalUrl: string;

	function showDeletePostModal() {
		dispatch("postaction", {
			action: "delete-post",
			boardId: boardId,
			postId: postId,
		});
	}

	function showBanIpModal() {
		dispatch("postaction", {
			action: "ban-ip",
			boardId: boardId,
			postId: postId,
		});
	}

	function showReportModal() {
		dispatch("postaction", {
			action: "report",
			boardId: boardId,
			postId: postId,
		});
	}

	function viewOriginal() {
		window.open(originalUrl, '_blank').focus();
	}
</script>

<div class="menu">
	<div class="menu-caption">Post</div>
	<div class="menu-item" on:click={viewOriginal}>Original</div>
	<div class="menu-item" on:click={showReportModal}>Report</div>
	{#if moderator}
		<div class="menu-item" on:click={showDeletePostModal}>Delete post</div>
		<!-- <div class="menu-item">
        Delete image
    </div> -->
		<div class="menu-item" on:click={showBanIpModal}>Ban poster IP</div>
	{/if}
</div>

<style>
</style>
