<script lang="ts">
    import { BoardModel, InfoObject } from "./data/data";
    import { Utility } from "./data/utility";
    import { moderatorUserStore, boardInfoStore, theme as themeStore } from "./data/stores"
    import { Api } from "./data/api";
    import SearchBar from "./component/SearchBar.svelte";
	import { router } from "tinro";

    const themes = [
        { key: "yotsuba4", text: "Yotsuba" },
        { key: "yotsubab4", text: "Yotsuba B" },
        { key: "yotsuba", text: "Heyuri" },
        { key: "yotsubab", text: "Heyuri B" },
        { key: "futaba", text: "Futaba" },
        { key: "burichan", text: "Burichan" },
/*        { key: "tomorrow", text: "Tomorrow" },*/
        { key: "niniba", text: "Niniba" },
        { key: "osaka", text: "Osaka" },
        { key: "midnight", text: "Midnight" },
    ]

    let selectedTheme: string = $themeStore;

    let loadedBoardInfo: BoardModel[] | null = null;

    (async function() {
        loadedBoardInfo = await $boardInfoStore;
    })();
</script>

<style>
    .nav-link {
        padding: 0;
        padding-right: 0 !important;
    }

    .nav-text {
        padding-left: 0.5rem;
        color: var(--text-color);
    }

    .board-nav-link {
        color: var(--text-color) !important;
    }

    .board-nav-link:hover {
        color: var(--link-hover-color) !important;
        text-decoration: underline;
    }

    .brand-link {
        font-size: 16px;
        font-weight: bold;
    }

    .theme-select {
        max-width: 150px;
    }

    .logo {
        box-sizing: border-box;
        height: 85px;
        margin: auto;
        display: block;
        float: none;
    }

    .legal-link {
        margin-left: 15px;
    }

    .nav-button {
        border: 0px transparent;
        font-size: unset;
        line-height: unset;
        color: var(--link-text-color) !important;
        transition: none;
    }

    .nav-button:hover {
        color: var(--link-hover-color) !important;
    }

    .separator {
        border-left: 1px solid var(--text-color);
        margin-left: .5rem;
    }

    .max-container {
        max-width: 100%;
    }

    .banner {
        padding: 0.5rem 2rem;
        border-bottom: 2px solid var(--nav-background-color);
    }

    .donate-link {
        text-decoration: underline;
    }
</style>

<header class="mb-3">
    <nav class="navbar navbar-expand-sm navbar-toggleable-sm box-shadow" class:navbar-light={selectedTheme !== 'tomorrow' && selectedTheme !== 'midnight' && selectedTheme !== 'osaka'} class:navbar-dark={selectedTheme === 'tomorrow' || selectedTheme === 'midnight' || selectedTheme === 'osaka'}>
        <div class="container max-container">
            <a class="board-nav-link brand-link" href="/">{Utility.infoObject.siteName}</a>
            <button class="navbar-toggler" type="button" data-toggle="collapse" data-target=".navbar-collapse" aria-controls="navbarSupportedContent"
                    aria-expanded="false" aria-label="Toggle navigation">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="navbar-collapse collapse d-sm-inline-flex flex-sm-row-reverse">
                <ul class="navbar-nav flex-grow-1">
                    <li class="separator"></li>
                    {#await $boardInfoStore}
                        <li class="nav-item nav-text">Loading...</li>
                    {:then boardInfo}
						{#if Utility.infoObject.compactBoardMode}
							{@const groupedBoards = Utility.groupByArray(boardInfo, b => b.category)}

							{#each groupedBoards as groupedBoard}
								<li class="nav-item dropdown">
									<a class="nav-link dropdown-toggle" href="#" role="button" data-toggle="dropdown" aria-expanded="false">
										{groupedBoard.key}
									</a>
									<div class="dropdown-menu">

									{#each groupedBoard.values as board, index}
										<li class="nav-item"><a class="nav-link board-nav-link" href="/board/{board.shortName}" title={board.longName}>/{board.shortName}/</a></li>
										<!-- {#if index < boardInfo.length - 2}
											<span class="nav-text">-</span>
										{/if} -->
									{/each}

								</div></li>
							{/each}

						{:else}

							{#each boardInfo as board, index}
								<li class="nav-item"><a class="nav-link board-nav-link" href="/board/{board.shortName}" title={board.longName}>/{board.shortName}/</a></li>
								<!-- {#if index < boardInfo.length - 2}
									<span class="nav-text">-</span>
								{/if} -->
							{/each}

						{/if}
                    {:catch}
                        <li class="nav-item nav-text">Unable to load boards</li>
                    {/await}
                    <!-- <li class="nav-item dropdown">
                        <a class="nav-link dropdown-toggle" href="#" role="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                            Boards
                        </a>
                        <div class="dropdown-menu" aria-labelledby="navbarDropdown">

                        </div>
                    </li> -->
                    {#if $moderatorUserStore}
                        <li class="separator"></li>
                        <li class="nav-item">
                            <a class="nav-link" href="/Admin">Admin</a>
                        </li>
                        <li class="nav-item">
                            <button type="button" class="btn btn-link nav-link nav-button" on:click={() => { $moderatorUserStore = null; Api.UserLogoutAsync(); } }>Logout</button>
                            <!-- <a class="nav-link" href="#" on:click={() => { $moderatorUserStore = null; Api.UserLogoutAsync(); } }>Logout</a> -->
                        </li>
                    {/if}
                    <!-- <li class="nav-item">
                        <a class="nav-link" href="/Search">Search</a>
                    </li> -->
                    {#if Utility.infoObject.searchEnabled}
                        <li class="ml-auto">
                            <SearchBar boardInfo={loadedBoardInfo} />
                        </li>
                    {/if}
                </ul>
            </div>
        </div>
    </nav>
</header>
<div class="mx-4">
    <main class="pb-3">
        {#if Utility.infoObject.bannerFilename}
            <img src={`/${Utility.infoObject.bannerFilename}`} class="logo mb-4" alt="banner" />
        {/if}
        <slot></slot>
    </main>
</div>

<footer class="footer text-muted">
    <div class="container d-flex align-items-center">
        <span class="mr-3"><a href="https://github.com/bbepis/Hayden" tinro-ignore>Hayden</a> 1.0 + <a href="https://chud.site" tinro-ignore>SudeArchive</a> patch + Marge modifications</span>
        <div class="flex-grow-1"></div>
        <select class="form-control theme-select" style="padding: 0.25rem; height: calc(1.5rem + 0.25rem)"
            bind:value={selectedTheme}
            on:change={() => $themeStore = selectedTheme}
        >
            {#each themes as theme}
                <option selected={selectedTheme === theme.key} value={theme.key}>{theme.text}</option>
            {/each}
        </select>
    </div>
</footer>
