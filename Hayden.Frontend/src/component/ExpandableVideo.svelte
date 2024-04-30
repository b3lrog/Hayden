<script lang="ts">
	import { Utility } from "../data/utility";

    export let thumbUrl: string;
    export let videoUrl: string;
    export let altText: string;
    export let expanded: boolean = false;
    export let width: number;
    export let height: number;
    export let extension: string;

    let displayThumbUrl = extension === "mov" ? '/file.png' : thumbUrl;

    let thumbnailSize: { width: number, height: number } | null = (extension === "mov" || extension === "swf" || extension == "pdf") ? null :
        Utility.guessThumbnailSize(width, height);

    let img : HTMLImageElement;

    export let onClick: () => void = () => {
        const newValue = !expanded;
        
        if (!newValue && !isElementInViewport(img)) {
            img.scrollIntoView();
        }

        expanded = newValue;
    };

    function isElementInViewport (el: Element) {
        const rect = el.getBoundingClientRect();

        return rect.y >= 0;
    }

    function onClickInternal(e: Event) {
        if (!Utility.infoObject.fileExpanding) {
            return;
        }
        e.preventDefault();
        onClick();
    }

    function onClickClose(e: Event) {
        e.preventDefault();
        expanded = false;
    }
</script>

{#if Utility.infoObject.thumbLinks}
    {#if expanded}
        <a on:click={onClickClose}>[Close]</a>
        <br/>
        <!-- svelte-ignore a11y-media-has-caption -->
        <video controls autoplay>
            <source src={videoUrl} />
        </video>
    {:else}
        <a href={videoUrl} on:click={onClickInternal} tinro-ignore>
            <img bind:this={img} src={displayThumbUrl} alt={altText} decoding="async" class="thumb-clickable" width={thumbnailSize.width} height={thumbnailSize.height}/>
        </a>
    {/if}
{:else}
    <img
        bind:this={img}
        src={displayThumbUrl}
        alt={altText}
        decoding="async"
        width={thumbnailSize ? thumbnailSize.width : null}
        height={!thumbnailSize ? 128 : null}
        style:aspect-ratio={thumbnailSize ? (thumbnailSize.width / thumbnailSize.height) : null}/>
{/if}

<style>
    img {
        max-width: 100%;
    }

    .thumb-clickable {
        cursor: pointer;
    }

    video {
        max-width: 100%;
    }
</style>
