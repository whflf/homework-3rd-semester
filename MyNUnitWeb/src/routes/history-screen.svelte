<script>
    import { mount, onMount, unmount } from "svelte";
    import TestInfo from "./test-info.svelte";

    const DESCRIPTORS = [null, "pass", "fail", "ignore", "custom"];

    onMount(async () => {
        const response = await fetch("https://localhost:7113/api/history");
        const testEntries = await response.json();

        const historyBody = document.getElementById("history-body");

        historyBody.childNodes.forEach((value) => value.remove());

        for (
            let historyEntryIdx = 0;
            historyEntryIdx < testEntries.length;
            historyEntryIdx++
        ) {
            const historyEntry = testEntries[historyEntryIdx];
            const historyEntryRow = document.createElement("tr");
            for (let i = 0; i <= 5; i++) {
                const cell = document.createElement("td");
                switch (i) {
                    case 0:
                        cell.innerHTML = historyEntry.buildName;
                        break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        const testsInCategory = historyEntry[DESCRIPTORS[i]];
                        const testsInCategoryCount = testsInCategory.length;
                        cell.innerHTML = testsInCategoryCount.toString();
                        if (testsInCategoryCount === 0) break;
                        const moreInfo = document.createElement("sup");
                        moreInfo.innerHTML = "(?)";
                        moreInfo.classList.add("more-info");
                        moreInfo.id = `more-info-${historyEntryIdx}-${i}`;
                        moreInfo.addEventListener("pointerenter", () => {
                            ShowInfo(moreInfo.id, testsInCategory);
                        });
                        moreInfo.addEventListener("pointerleave", () => {
                            HideInfo();
                        });
                        cell.appendChild(moreInfo);
                        break;
                    case 5:
                        cell.innerHTML = historyEntry.time;
                        break;
                }
                historyEntryRow.appendChild(cell);
            }
            historyBody.appendChild(historyEntryRow);
        }
    });

    let InfoHold;
    function ShowInfo(id, info) {
        InfoHold = mount(TestInfo, {
            target: document.getElementById(id),
            props: { info: info },
        });
    }
    function HideInfo() {
        if (!InfoHold) return;
        unmount(InfoHold);
        InfoHold = undefined;
    }
</script>

<table border="2" align="center">
    <thead>
        <tr>
            <th>Сборка</th>
            <th style="background-color: greenyellow;">Успех</th>
            <th style="background-color: lightcoral">Провал</th>
            <th style="background-color: gainsboro;">Игнор</th>
            <th>Прочее</th>
            <th>Время</th>
        </tr>
    </thead>
    <tbody id="history-body"></tbody>
</table>

<style>
    th {
        font-size: medium;
        padding: 0 5px 0 5px;
    }
    table {
        width: 60%;
    }
    :global(.more-info) {
        text-decoration: underline;
    }
    :global(.more-info):hover {
        cursor: pointer;
    }
</style>
