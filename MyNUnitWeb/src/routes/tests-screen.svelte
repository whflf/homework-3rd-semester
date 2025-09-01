<script>
    import { SelectorButtonClicked, BUTTON_TYPES } from "./+page.svelte";
    function FormChanged() {
        const testsForm = document.getElementById("tests-form");
        const files = document.getElementById("files");
        const startBtn = document.getElementById("start-btn");
        for (const filenameNode of document.getElementsByClassName(
            "filename",
        )) {
            filenameNode.remove();
        }
        for (const file of files.files) {
            const fileListElement = document.createElement("p");
            fileListElement.innerHTML = file.name;
            fileListElement.classList.add("filename");
            testsForm.insertBefore(fileListElement, startBtn);
        }
    }

    async function FormSubmit(event) {
        event.preventDefault();

        const testsForm = document.getElementById("tests-form");

        const response = await fetch("https://localhost:7113/api/startTests", {
            method: "POST",
            body: new FormData(testsForm),
        });

        testsForm.reset();
        FormChanged();
        alert("Тестирование запущено...");

        const result = await response.json();

        if (result.ok) {
            SelectorButtonClicked(BUTTON_TYPES.history);
        } else {
            alert("Что-то пошло не так.");
        }
    }
</script>

<form id="tests-form" enctype="multipart/form-data" onsubmit={FormSubmit}>
    <input
        id="files"
        name="files"
        type="file"
        multiple
        onchange={FormChanged}
        accept="application/x-msdownload"
    /><br />
    <button id="start-btn">Начать тестирование</button>
</form>

<style>
    :global(.filename) {
        white-space: pre-line;
    }
    #files {
        margin-bottom: 8px;
        font-size: medium;
    }
    #start-btn {
        width: 30%;
        height: 50px;
        margin-top: 30px;
        color: white;
        font-size: large;
        background-color: #ee82ee;
    }
    #start-btn:hover {
        background-color: #ce62ee;
    }
</style>
