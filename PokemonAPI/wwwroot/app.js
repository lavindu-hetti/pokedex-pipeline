const pokemonGrid = document.getElementById("pokemon-grid");
const statusMessage = document.getElementById("status-message");
const resultCount = document.getElementById("result-count");
const idForm = document.getElementById("id-form");
const nameForm = document.getElementById("name-form");
const typeForm = document.getElementById("type-form");
const loadAllButton = document.getElementById("load-all");
const dbModeButton = document.getElementById("db-mode");
const liveModeButton = document.getElementById("live-mode");
const modeDescription = document.getElementById("mode-description");
const cardTemplate = document.getElementById("pokemon-card-template");

let currentMode = "db";

idForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    const id = document.getElementById("pokemon-id").value.trim();
    if (!id) {
        setStatus("Enter a Pokemon ID first.");
        return;
    }

    const url = currentMode === "live"
        ? `/api/pokemon/live/${id}`
        : `/api/pokemon/${id}`;
    const source = currentMode === "live" ? "live API" : "database";

    await fetchAndRender(url, `Pokemon #${id} from ${source}`);
});

nameForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    const name = document.getElementById("pokemon-name").value.trim().toLowerCase();
    if (!name) {
        setStatus("Enter a Pokemon name first.");
        return;
    }

    const url = currentMode === "live"
        ? `/api/pokemon/live/name/${encodeURIComponent(name)}`
        : `/api/pokemon/name/${encodeURIComponent(name)}`;
    const label = currentMode === "live"
        ? `live Pokemon named ${name}`
        : `name match for ${name}`;

    await fetchAndRender(url, label);
});

typeForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    if (currentMode === "live") {
        setStatus("Live mode currently supports ID search only.");
        return;
    }

    const type = document.getElementById("pokemon-type").value.trim().toLowerCase();
    if (!type) {
        setStatus("Enter a type like grass, fire, or water.");
        return;
    }

    await fetchAndRender(`/api/pokemon/type/${encodeURIComponent(type)}`, `${type} type Pokemon`);
});

loadAllButton.addEventListener("click", async () => {
    if (currentMode === "live") {
        setStatus("Live mode currently supports single Pokemon lookup by ID.");
        return;
    }

    await fetchAndRender("/api/pokemon", "all Pokemon");
});

dbModeButton.addEventListener("click", () => {
    setMode("db");
});

liveModeButton.addEventListener("click", () => {
    setMode("live");
});

async function fetchAndRender(url, label) {
    setStatus(`Loading ${label}...`);

    try {
        const response = await fetch(url);

        if (response.status === 404) {
            renderPokemon([]);
            setStatus(`No results found for ${label}.`);
            return;
        }

        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}.`);
        }

        const data = await response.json();
        const pokemonList = Array.isArray(data) ? data : [data];

        renderPokemon(pokemonList);
        setStatus(`Loaded ${pokemonList.length} result${pokemonList.length === 1 ? "" : "s"} for ${label}.`);
    } catch (error) {
        renderPokemon([]);
        setStatus(error.message || "Something went wrong while calling the API.");
    }
}

function renderPokemon(pokemonList) {
    pokemonGrid.innerHTML = "";
    resultCount.textContent = `${pokemonList.length} Pokemon`;

    if (pokemonList.length === 0) {
        const emptyState = document.createElement("div");
        emptyState.className = "empty-state";
        emptyState.textContent = "No Pokemon to show yet. Try loading the dex or searching by ID, name, or type.";
        pokemonGrid.appendChild(emptyState);
        return;
    }

    for (const pokemon of pokemonList) {
        const fragment = cardTemplate.content.cloneNode(true);

        fragment.querySelector(".pokemon-number").textContent = `#${pokemon.id}`;
        fragment.querySelector(".pokemon-name").textContent = pokemon.name;
        fragment.querySelector(".pokemon-height").textContent = pokemon.height;
        fragment.querySelector(".pokemon-weight").textContent = pokemon.weight;
        fragment.querySelector(".pokemon-abilities").textContent = formatList(pokemon.abilities);
        fragment.querySelector(".pokemon-badge").textContent = currentMode === "live" ? "Live" : "Saved";

        const typeContainer = fragment.querySelector(".pokemon-types");
        for (const type of splitCsv(pokemon.types)) {
            const chip = document.createElement("span");
            chip.className = "type-chip";
            chip.textContent = type;
            typeContainer.appendChild(chip);
        }

        pokemonGrid.appendChild(fragment);
    }
}

function splitCsv(value) {
    const rawItems = Array.isArray(value) ? value : (value || "").split(",");

    return rawItems
        .map((item) => item.trim())
        .filter(Boolean);
}

function formatList(value) {
    const items = splitCsv(value);
    return items.length > 0 ? items.join(", ") : "None";
}

function setStatus(message) {
    statusMessage.textContent = message;
}

function setMode(mode) {
    currentMode = mode;

    const isLiveMode = mode === "live";

    dbModeButton.classList.toggle("mode-button-active", !isLiveMode);
    liveModeButton.classList.toggle("mode-button-active", isLiveMode);

    loadAllButton.disabled = isLiveMode;
    document.getElementById("pokemon-type").disabled = isLiveMode;
    document.getElementById("pokemon-name").disabled = false;

    modeDescription.textContent = isLiveMode
        ? "Live API mode supports exact ID and exact name lookup directly from PokeAPI."
        : "Database mode supports full list, ID, name, and type search.";

    setStatus(isLiveMode
        ? "Live mode is active. Search by exact ID or exact name to fetch real-time data."
        : "Database mode is active. Browse saved Pokemon from SQL Server.");
}

setMode("db");
fetchAndRender("/api/pokemon", "all Pokemon");
