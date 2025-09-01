<script>
    import { onMount } from "svelte";

    let { info } = $props();

    onMount(() => {
        const testInfoBody = document.getElementById("test-info-body");

        testInfoBody.childNodes.forEach((value) => value.remove);

        for (const test of info) {
            const testEntryRow = document.createElement("tr");

            for (let i = 0; i < 3; i++) {
                const cell = document.createElement("td");
                switch (i) {
                    case 0:
                        cell.innerHTML = test.name;
                        break;
                    case 1:
                        cell.innerHTML = test.time;
                        break;
                    case 2:
                        cell.innerHTML = test.note;
                        break;
                }
                testEntryRow.appendChild(cell);
            }

            testInfoBody.appendChild(testEntryRow);
        }
    });
</script>

<table border="1" id="infotable" align="center">
    <thead>
        <tr>
            <th style="background-color: lightgray;">Тест</th>
            <th>Время</th>
            <th>Комментарий</th>
        </tr>
    </thead>
    <tbody id="test-info-body"></tbody>
</table>

<style>
    #infotable {
        position: absolute;
        z-index: 3;
        border: solid;
        background-color: white;
    }
</style>
