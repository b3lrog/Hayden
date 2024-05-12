<script lang="ts">
	import { Utility } from "../data/utility";

    export let thumbUrl: string;
    export let fullImageUrl: string;
    export let altText: string;
    export let expanded: boolean = false;
    export let width: number;
    export let height: number;
    export let extension: string;

    let displayThumbUrl = (extension === "mp3" || extension === "wav" || extension === "flac" || extension === "opus" || extension === "ogg") ? '/speaker.png' : ((extension === "pdf") ? '/file.png' : (extension === "swf" ? '/swf.png' : thumbUrl));

    let thumbnailSize: { width: number, height: number } | null = (extension === "mov" || extension === "swf" || extension == "pdf") ? null :
        Utility.guessThumbnailSize(width, height);

    let img : HTMLImageElement;

    let loading: boolean = false;

    export let onClick: () => void = () => {
        const newValue = !expanded;
        
        if (!newValue && !isElementInViewport(img)) {
            img.scrollIntoView();
        }
        else if (newValue) {
            loading = true;
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
</script>

{#if Utility.infoObject.thumbLinks}
<a href={fullImageUrl} on:click={onClickInternal} tinro-ignore>
    <img
        bind:this={img}
        on:load={() => loading = false}
        loading="lazy"
        src={expanded ? fullImageUrl : displayThumbUrl}
        alt={altText}
        class="thumb-clickable"
        class:loading={loading}
        decoding="async"
        width={expanded ? null : (thumbnailSize ? thumbnailSize.width : null)}
        height={expanded ? null : (!thumbnailSize ? 128 : null)}
        style:aspect-ratio={expanded ? null : (thumbnailSize ? (thumbnailSize.width / thumbnailSize.height) : null)}/>
</a>
{:else}
    <img
        bind:this={img}
        loading="lazy"
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

    .loading {
        opacity: 50%;
    }
</style>