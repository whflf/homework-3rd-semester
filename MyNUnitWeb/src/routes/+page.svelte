<script module>
    import TestsScreen from "./tests-screen.svelte";
    import HistoryScreen from "./history-screen.svelte";
    import { mount, onMount, unmount } from "svelte";

    export const BUTTON_TYPES = {
        tests: 0,
        history: 1,
    };

    const Screens = [TestsScreen, HistoryScreen];

    let MountedScreen;

    export function SelectorButtonClicked(buttonType) {
        unmount(MountedScreen);
        MountedScreen = mount(Screens[buttonType], {
            target: document.getElementById("screen-container"),
        });
    }
</script>

<script>
    onMount(() => {
        MountedScreen = mount(TestsScreen, {
            target: document.getElementById("screen-container"),
        });
    });
</script>

<svelte:head>
    <title>MyNUnitWeb</title>
</svelte:head>

<div id="screen-selectors">
    <button
        class="screen-button"
        id="tests-btn"
        onclick={() => {
            SelectorButtonClicked(BUTTON_TYPES.tests);
        }}>Тестирование</button
    >
    <button
        class="screen-button"
        id="history-btn"
        onclick={() => {
            SelectorButtonClicked(BUTTON_TYPES.history);
        }}>История</button
    >
</div>

<div id="screen-container"></div>

<style>
    .screen-button {
        width: 35%;
        height: 40px;
        color: purple;
        font-size: 120%;
        margin-top: 1%;
    }
    #tests-btn {
        margin-right: 2%;
    }
    #screen-selectors,
    #screen-container {
        text-align: center;
    }
    #screen-container {
        margin-top: 50px;
    }
</style>
