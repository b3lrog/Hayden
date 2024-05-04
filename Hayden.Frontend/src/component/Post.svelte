<script lang="ts">
	import type { BoardModel, FileModel, PostModel } from "../data/data";
	import moment from "moment";
	import ExpandableImage from "./ExpandableImage.svelte";
	import { onMount } from "svelte";
	import { Utility } from "../data/utility";
	import { RenderRawPost } from "../data/postrender";
	import PostMenu from "./PostMenu.svelte";
	import { moderatorUserStore } from "../data/stores";
	import ExpandableVideo from "./ExpandableVideo.svelte";
	import { router } from "tinro";

	export let threadId: number;
	export let post: PostModel;
	export let board: BoardModel;
	export let subject: string = null;
	export let backquotes: number[] = null;

	function getDateTime() {
		if (post.dateTime.endsWith("Z")) {
			return post.dateTime;
		}

		return post.dateTime + "Z";
	}

	const time = moment(getDateTime());

	const originalUrl = `https://soyjak.party/${board.shortName}/thread/${threadId}.html#${post.postId}`;

	let showDropdown: boolean = false;
	let menu: HTMLElement;
	let menuArrow: HTMLElement;

	onMount(() => {
		jQuery(".post-contents a").attr("tinro-ignore", "true");
	});

	function documentClick(e: MouseEvent) {
		const path = e.composedPath();
		if (path.includes(menu) || path.includes(menuArrow)) {
			return;
		}

		toggleMenu(false);
	}

	function toggleMenu(value: boolean | null) {
		showDropdown = value ?? !showDropdown;

		if (showDropdown) {
			document.body.addEventListener('click', documentClick);
			//setImmediate(() => {menu.focus();});
			setTimeout(() => {
				menu.focus();
			}, 0);
		} else {
			document.body.removeEventListener('click', documentClick);
		}
	}

	function menuKeyDown(e: KeyboardEvent) {
		if (e.keyCode === 27) {
			showDropdown(false);
			e.preventDefault();
		}
	}

	function getFilename(file: FileModel): string {
		if (file.extension) return `${file.filename}.${file.extension}`;

		return file.filename;
	}

	function view() {
		router.goto(`/${board.shortName}/thread/${threadId}`)
	}
</script>

<div id="p{post.postId}" class="post reply">
	<div id="pi{post.postId}" class="postInfo">
		{#if subject}
			<span class="subject">{subject}</span>
		{/if}
		<span class="nameBlock">
			{#if post.email}
				<a class="email" href={`mailto:${post.email}`}>
					<span class="name">{post.author ?? "Anonymous"}</span>
					{#if post.email === "sage"}
						SAGE!
					{/if}
				</a>
			{:else}
				<span class="name">{post.author ?? "Anonymous"}</span>
			{/if}
			{#if post.posterId}<span class="posterId">ID: {post.posterId}</span>{/if}
			{#if post.capcode}<span class="capcode">## {post.capcode}</span>{/if}
			{#if post.tripcode}<span class="tripcode">{post.tripcode}</span>{/if}
		</span>
		{#if post.country}
			<img class="flag" src={`/flags/${post.country.code.toLowerCase()}.png`} alt={post.country.name} title={post.country.name}>
		{/if}
		<span title={time.fromNow()}>
			{time.local().format("DD/MM/YY(ddd)HH:mm")}
		</span>
		<span>
			<a href="/{board.shortName}/thread/{threadId}#p{post.postId}" tinro-ignore
				>&numero;{post.postId}</a
			>
		</span>

		<!-- svelte-ignore a11y-click-events-have-key-events -->
		<span class="menu-button" on:click={() => toggleMenu(!showDropdown)}>
			<i class="fa-solid fa-caret-right menu-arrow" class:turn={showDropdown} bind:this={menuArrow}></i>
			<div
				tabindex="-1"
				class:hidden={!showDropdown}
				class="menu-container"
				on:keydown={menuKeyDown}
				on:click={(e) => {
					if (e.target.classList.contains('menu-item')) {
						toggleMenu(false);
					}
					e.stopPropagation();
				}}
				bind:this={menu}
			>
				<PostMenu
					on:postaction
					boardId={board.id}
					postId={post.postId}
					moderator={!!$moderatorUserStore}
					originalUrl={originalUrl}
				/>
			</div>
		</span>

		{#if backquotes.length}
			<div class="backquote-list">
				Quoted By:
				{#each backquotes as backquoteId (backquoteId)}
					<div class="backquote">
						<a href="/{board.shortName}/thread/{threadId}#p{backquoteId}" class="quotelink" tinro-ignore
							>&gt;&gt;{backquoteId}</a
						>
					</div>
				{/each}
			</div>
		{/if}
	</div>
	{#if post.embed}
		{@html post.embed}
	{/if}
	{#if post.files.length === 1}
		{@const file = post.files[0]}
		<div class="file">
			<div class="fileText">
				<a href={Utility.infoObject.thumbLinks ? file.imageUrl : null} tinro-ignore
					>{getFilename(file)}</a
				>
				{#if file.fileSize}
				{Utility.ToHumanReadableSize(file.fileSize)}{post.files[0]
					.imageWidth !== null
					? `, ${file.imageWidth} × ${file.imageHeight}`
					: ""}
				{/if}
			</div>
			{#if !file.thumbnailUrl && !file.imageUrl}
				<img
					class="fileThumb"
					style="width: 125px;"
					src="/image-error.png"
					alt="Missing file"
				/>
			{:else}
				{#if file.extension === "webm" || file.extension === "mp4" || file.extension === "mov"}
					<ExpandableVideo
						videoUrl={file.imageUrl}
						thumbUrl={file.thumbnailUrl}
						altText={file.filename}
						width={file.imageWidth}
						height={file.imageHeight}
						extension={file.extension}
					/>
				{:else}
					<ExpandableImage
						fullImageUrl={file.imageUrl}
						thumbUrl={file.thumbnailUrl}
						altText={file.filename}
						width={file.imageWidth}
						height={file.imageHeight}
						extension={file.extension}
					/>
				{/if}
			{/if}
		</div>
	{:else if post.files.length > 1}
		<div class="panelUploads multipleUploads">
			{#each post.files as file}
				<figure class="uploadCell">
					<div class="uploadDetails">
						{#if file.fileSize}
						<span class="sizeLabel"
							>{Utility.ToHumanReadableSize(file.fileSize)}</span
						>
						{/if}
						{#if file.imageWidth !== null}
							<span class="dimensionLabel"
								>{file.imageWidth} × {file.imageHeight}</span
							>
						{/if}
						<a
							class="originalNameLink"
							href={Utility.infoObject.thumbLinks ? file.imageUrl : null}
							download="{getFilename(file)}"
							>{getFilename(file)}</a
						>
					</div>

					<div />
					{#if !file.thumbnailUrl && !file.imageUrl}
						<img
							class="fileThumb"
							style="width: 125px;"
							src="/image-error.png"
							alt="Missing file"
						/>
					{:else}
						{#if file.extension === "webm" || file.extension === "mp4" || file.extension === "mov"}
							<ExpandableVideo
								videoUrl={file.imageUrl}
								thumbUrl={file.thumbnailUrl}
								altText={file.filename}
								width={file.imageWidth}
								height={file.imageHeight}
								extension={file.extension}
							/>
						{:else}
							<ExpandableImage
								fullImageUrl={file.imageUrl}
								thumbUrl={file.thumbnailUrl}
								altText={file.filename}
								width={file.imageWidth}
								height={file.imageHeight}
								extension={file.extension}
							/>
						{/if}
					{/if}
				</figure>
			{/each}
		</div>
	{/if}
	<blockquote class="post-contents">
		{#if post.contentHtml}
			{@html post.contentHtml
				.replaceAll('\n', '<br/>')
				.replace(/<a\s+[^>]*?href="\/(.*?)\/(thread|res)\/(\d+)\.html#(\d+)"[^>]*?>/g, '<a class="quoteLink" href="/$1/thread/$3#p$4" tinro-ignore="true">')
				.replace(/<a\s+[^>]*?href="\/(.*?)\/index.html">&gt;&gt;&gt;\/(.*?)\/<\/a>/g, '<a class="quoteLink" href="/board/$2" tinro-ignore="true">&gt;&gt;&gt;/$2/</a>')
				.replaceAll('<a href="https://jump.kolyma.net?', '<a href="')
				.replaceAll('<img src="/static/inline_soyjaks/', '<img src="/inline_soyjaks/')}
		{/if}
	</blockquote>
</div>

<style>
	.backquote {
		display: inline-block;
		padding-left: 3px;
		padding-right: 3px;
	}

	.menu-button {
		cursor: pointer;
		position: relative;
	}

	.post {
		overflow: initial;
	}

	.post-contents {
		overflow-wrap: anywhere;
	}

	.backquote-list {
		font-size: 11px;
		display: block;
		margin-bottom: 5px;
		line-height: 150%;
	}

	.backquote-list a:not(:hover) {
		text-decoration: none;
	}

	.flag {
		width: 16px;
		height: 11px;
		vertical-align: baseline;
	}

	.capcode {
		font-weight: bold;
		color: #ff0000;
	}

	.email .name {
		color: inherit;
	}

	.menu-arrow {
		transition: transform 0.15s;
	}

	.turn {
		transform: rotate(90deg);
	}

	.menu-container {
		position: absolute;
		left: 0;
		top: 100%;
	}
</style>
