document.addEventListener('DOMContentLoaded', function () {
    "use strict";

    // --- Configuration ---

    // URLS
    const URLS = {
        D2E: 'https://raw.githubusercontent.com/NPBruce/valkyrie-store/master/D2E/manifestDownload.ini',
        D2E_PACKS: 'https://raw.githubusercontent.com/NPBruce/valkyrie-store/master/D2E/contentPacksManifestDownload.ini',
        MOM: 'https://raw.githubusercontent.com/NPBruce/valkyrie-store/master/MoM/manifestDownload.ini',
        MOM_PACKS: 'https://raw.githubusercontent.com/NPBruce/valkyrie-store/master/MoM/contentPacksManifestDownload.ini'
    };

    const LANG_FLAGS = {
        'English': '🇬🇧',
        'Spanish': '🇪🇸',
        'German': '🇩🇪',
        'French': '🇫🇷',
        'Italian': '🇮🇹',
        'Polish': '🇵🇱',
        'Portuguese': '🇵🇹',
        'Russian': '🇷🇺',
        'Czech': '🇨🇿',
        'Hungarian': '🇭🇺',
        'Dutch': '🇳🇱',
        'Chinese': '🇨🇳',
        'Korean': '🇰🇷',
        'Japanese': '🇯🇵'
    };

    const TRANSLATIONS = {
        'Difficulty': {
            'English': 'Difficulty',
            'German': 'Schwierigkeit',
            'Spanish': 'Dificultad',
            'French': 'Difficulté',
            'Italian': 'Difficoltà',
            'Polish': 'Trudność',
            'Portuguese': 'Dificuldade'
        },
        'Expansions': {
            'English': 'Required Content Packs',
            'German': 'Benötigte Inhaltspakete',
            'Spanish': 'Paquetes de Contenido Requeridos',
            'French': 'Packs de Contenu Requis',
            'Italian': 'Pacchetti di Contenuto Richiesti',
            'Polish': 'Wymagane Pakiety Zawartości',
            'Portuguese': 'Pacotes de Conteúdo Necessários'
        },
        'NoImage': {
            'English': 'No Image',
            'German': 'Kein Bild',
            'Spanish': 'Sin Imagen',
            'French': 'Pas d\'Image',
            'Italian': 'Nessuna Immagine',
            'Polish': 'Brak Obrazu',
            'Portuguese': 'Sem Imagem'
        },
        'NoContent': {
            'English': 'No content found.',
            'German': 'Kein Inhalt gefunden.',
            'Spanish': 'No se encontró contenido.',
            'French': 'Aucun contenu trouvé.',
            'Italian': 'Nessun contenuto trovato.',
            'Polish': 'Nie znaleziono zawartości.',
            'Portuguese': 'Nenhum conteúdo encontrado.'
        },
        'NoPacks': {
            'English': 'No content packs found.',
            'German': 'Keine Inhaltspakete gefunden.',
            'Spanish': 'No se encontraron paquetes de contenido.',
            'French': 'Aucun pack de contenu trouvé.',
            'Italian': 'Nessun pacchetto di contenuto trovato.',
            'Polish': 'Nie znaleziono pakietów zawartości.',
            'Portuguese': 'Nenhum pacote de conteúdo encontrado.'
        },
        'Duration': {
            'English': 'Duration',
            'German': 'Dauer',
            'Spanish': 'Duración',
            'French': 'Durée',
            'Italian': 'Durata',
            'Polish': 'Czas trwania',
            'Portuguese': 'Duração'
        },
        'Author': {
            'English': 'Author',
            'German': 'Autor',
            'Spanish': 'Autor',
            'French': 'Auteur',
            'Italian': 'Autore',
            'Polish': 'Autor',
            'Portuguese': 'Autor'
        },
        'LastUpdated': {
            'English': 'Last Updated',
            'German': 'Letzte Aktualisierung',
            'Spanish': 'Última Actualización',
            'French': 'Dernière mise à jour',
            'Italian': 'Ultimo aggiornamento',
            'Polish': 'Ostatnia aktualizacja',
            'Portuguese': 'Última atualização'
        },
        'Language': {
            'English': 'Language',
            'German': 'Sprache',
            'Spanish': 'Idioma',
            'French': 'Langue',
            'Italian': 'Lingua',
            'Polish': 'Język',
            'Portuguese': 'Idioma'
        },
        'Any': {
            'English': 'Any',
            'German': 'Beliebig',
            'Spanish': 'Cualquiera',
            'French': 'Tout',
            'Italian': 'Qualsiasi',
            'Polish': 'Dowolny',
            'Portuguese': 'Qualquer'
        },
        'SortBy': {
            'English': 'Sort By',
            'German': 'Sortieren nach',
            'Spanish': 'Ordenar por',
            'French': 'Trier par',
            'Italian': 'Ordina per',
            'Polish': 'Sortuj według',
            'Portuguese': 'Ordenar por'
        },
        'Name': {
            'English': 'Name',
            'German': 'Name',
            'Spanish': 'Nombre',
            'French': 'Nom',
            'Italian': 'Nome',
            'Polish': 'Nazwa',
            'Portuguese': 'Nome'
        },
        'Ascending': {
            'English': 'Ascending',
            'German': 'Aufsteigend',
            'Spanish': 'Ascendente',
            'French': 'Croissant',
            'Italian': 'Ascendente',
            'Polish': 'Rosnąco',
            'Portuguese': 'Ascendente'
        },
        'Descending': {
            'English': 'Descending',
            'German': 'Absteigend',
            'Spanish': 'Descendente',
            'French': 'Décroissant',
            'Italian': 'Discendente',
            'Polish': 'Malejąco',
            'Portuguese': 'Descendente'
        },
        'Search': {
            'English': 'Search',
            'German': 'Suche',
            'Spanish': 'Buscar',
            'French': 'Recherche',
            'Italian': 'Cerca',
            'Polish': 'Szukaj',
            'Portuguese': 'Procurar'
        },
        'SearchHint': {
            'English': 'Search by Name',
            'German': 'Suche nach Name',
            'Spanish': 'Buscar por nombre',
            'French': 'Recherche par nom',
            'Italian': 'Cerca per nome',
            'Polish': 'Szukaj według nazwy',
            'Portuguese': 'Procurar por nome'
        },
        'AverageDuration': {
            'English': 'Average Duration',
            'German': 'Durchschnittliche Dauer',
            'Spanish': 'Duración Media',
            'French': 'Durée Moyenne',
            'Italian': 'Durata Media',
            'Polish': 'Średni Czas',
            'Portuguese': 'Duração Média'
        },
        'UserReviews': {
            'English': 'user reviews',
            'German': 'Nutzerbewertungen',
            'Spanish': 'reseñas de usuarios',
            'French': 'avis utilisateurs',
            'Italian': 'recensioni utenti',
            'Polish': 'recenzji użytkowników',
            'Portuguese': 'análises de usuários'
        },
        'WinRatio': {
            'English': 'Win Ratio',
            'German': 'Gewinnrate',
            'Spanish': 'Ratio de Victoria',
            'French': 'Taux de Victoire',
            'Italian': 'Percentuale di Vittoria',
            'Polish': 'Współczynnik Wygranych',
            'Portuguese': 'Taxa de Vitória'
        },
        'Rating': {
            'English': 'Rating',
            'German': 'Bewertung',
            'Spanish': 'Valoración',
            'French': 'Évaluation',
            'Italian': 'Valutazione',
            'Polish': 'Ocena',
            'Portuguese': 'Avaliação'
        },
        'PlayCount': {
            'English': 'Play Count',
            'German': 'Anzahl Spiele',
            'Spanish': 'Partidas Jugadas',
            'French': 'Nombre de Parties',
            'Italian': 'Partite Giocate',
            'Polish': 'Liczba Gier',
            'Portuguese': 'Contagem de Jogos'
        },
        'CommunityRating': {
            'English': 'Community Rating',
            'German': 'Community-Bewertung',
            'Spanish': 'Valoración de la Comunidad',
            'French': 'Note de la Communauté',
            'Italian': 'Valutazione della Comunità',
            'Polish': 'Ocena Społeczności',
            'Portuguese': 'Avaliação da Comunidade'
        },
        'Reset': {
            'English': 'Reset',
            'German': 'Zurücksetzen',
            'Spanish': 'Restablecer',
            'French': 'Réinitialiser',
            'Italian': 'Reimposta',
            'Polish': 'Resetuj',
            'Portuguese': 'Redefinir'
        },
        'LabelScenarios': {
            'English': 'Scenarios',
            'German': 'Szenarien',
            'Spanish': 'Escenarios',
            'French': 'Scénarios',
            'Italian': 'Scenari',
            'Polish': 'Scenariusze',
            'Portuguese': 'Cenários'
        },
        'LabelPacks': {
            'English': 'Content Packs',
            'German': 'Inhaltspakete',
            'Spanish': 'Paquetes de Contenido',
            'French': 'Packs de Contenu',
            'Italian': 'Pacchetti di Contenuto',
            'Polish': 'Pakiety Zawartości',
            'Portuguese': 'Pacotes de Conteúdo'
        },
        'LabelFilteredOut': {
            'English': 'Filtered out',
            'German': 'Gefiltert',
            'Spanish': 'Filtrado',
            'French': 'Filtré',
            'Italian': 'Filtrato',
            'Polish': 'Odfiltrowane',
            'Portuguese': 'Filtrado'
        },
        'LabelNone': {
            'English': 'None',
            'German': 'Keine',
            'Spanish': 'Ninguno',
            'French': 'Aucun',
            'Italian': 'Nessuno',
            'Polish': 'Żaden',
            'Portuguese': 'Nenhum'
        },
        'PlayCountTooltip': {
            'English': 'Number of users who successfully finished the scenario',
            'German': 'Anzahl der Benutzer, die das Szenario erfolgreich abgeschlossen haben',
            'Spanish': 'Número de usuarios que terminaron con éxito el escenario',
            'French': 'Nombre d\'utilisateurs ayant terminé le scénario avec succès',
            'Italian': 'Numero di utenti che hanno completato con successo lo scenario',
            'Polish': 'Liczba użytkowników, którzy pomyślnie ukończyli scenariusz',
            'Portuguese': 'Número de usuários que terminaram o cenário com sucesso'
        },
        'LabelPlayerCount': {
            'English': 'Player Count',
            'German': 'Anzahl Spieler',
            'Spanish': 'Cantidad de Jugadores',
            'French': 'Nombre de Joueurs',
            'Italian': 'Numero di Giocatori',
            'Polish': 'Liczba Graczy',
            'Portuguese': 'Número de Jogadores'
        },
        'LabelMinPlayerCount': {
            'English': 'Minimum Player count',
            'German': 'Min. Spieleranzahl',
            'Spanish': 'Mín. Jugadores',
            'French': 'Min Joueurs',
            'Italian': 'Min Giocatori',
            'Polish': 'Min. Liczba Graczy',
            'Portuguese': 'Min. Jogadores'
        },
        'LabelMaxPlayerCount': {
            'English': 'Maximum Player count',
            'German': 'Max. Spieleranzahl',
            'Spanish': 'Max. Jugadores',
            'French': 'Max Joueurs',
            'Italian': 'Max Giocatori',
            'Polish': 'Max. Liczba Graczy',
            'Portuguese': 'Max. Jogadores'
        }
    };

    // --- Helpers ---

    function parseINI(data) {
        const lines = data.split(/\r?\n/);
        const items = [];
        let currentItem = null;

        lines.forEach(line => {
            line = line.trim();
            if (!line || line.startsWith(';')) return;

            if (line.startsWith('[') && line.endsWith(']')) {
                if (currentItem) items.push(currentItem);
                const id = line.substring(1, line.length - 1);
                currentItem = { id: id };
            } else if (currentItem) {
                const equalIndex = line.indexOf('=');
                if (equalIndex !== -1) {
                    const key = line.substring(0, equalIndex).trim();
                    let value = line.substring(equalIndex + 1).trim();

                    // Basic unescape for newlines
                    value = value.replace(/\\n/g, '\n');

                    // Remove surrounding quotes if present (e.g. image="foo.png")
                    if (value.length >= 2 && value.startsWith('"') && value.endsWith('"')) {
                        value = value.substring(1, value.length - 1);
                    }

                    currentItem[key] = value;
                }
            }
        });

        if (currentItem) items.push(currentItem);
        return items;
    }

    function loadData(url) {
        return fetch(url)
            .then(response => {
                if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                return response.text();
            })
            .then(text => {
                const parsed = parseINI(text);
                return parsed;
            });
    }

    function getHardwareStars(difficulty) {
        const val = parseFloat(difficulty) || 0;
        const stars = Math.round(val / 0.2);
        const count = Math.max(0, Math.min(5, stars));
        return {
            filled: '★'.repeat(count),
            empty: '☆'.repeat(5 - count)
        };
    }

    function getRatingStars(rating) {
        const val = parseFloat(rating) || 0;
        // Rating is 0-10, we want 0-5 stars
        const stars = Math.round(val / 2);
        const count = Math.max(0, Math.min(5, stars));
        return {
            filled: '★'.repeat(count),
            empty: '☆'.repeat(5 - count)
        };
    }

    function getLocalizedValue(item, fieldPrefix, lang) {
        let val = '';
        if (item[`${fieldPrefix}.${lang}`]) val = item[`${fieldPrefix}.${lang}`];
        else if (item[`${fieldPrefix}.English`]) val = item[`${fieldPrefix}.English`];
        else {
            const keys = Object.keys(item);
            const fallbackKey = keys.find(k => k.startsWith(fieldPrefix + '.'));
            if (fallbackKey) val = item[fallbackKey];
            else val = item[fieldPrefix] || '';
        }
        // Remove <size=46>, <b>, <i>, <color=red>, etc.
        return val.replace(/<[^>]+>/g, '');
    }

    const showInfoDialog = (title, message) => {
        // Remove existing modal if any
        const existing = document.getElementById('info-modal-overlay');
        if (existing) existing.remove();

        const overlay = document.createElement('div');
        overlay.id = 'info-modal-overlay';

        const dialog = document.createElement('div');
        dialog.className = 'bg-dark border border-secondary p-3 rounded shadow text-light info-modal-dialog';

        dialog.innerHTML = `
            <h6 class="mb-2 font-weight-bold">${title}</h6>
            <p class="mb-3 small">${message}</p>
            <div class="text-right text-end">
                <button class="btn btn-sm btn-light py-0 px-2" id="info-modal-close">OK</button>
            </div>
        `;

        overlay.appendChild(dialog);
        document.body.appendChild(overlay);

        const close = () => overlay.remove();
        dialog.querySelector('#info-modal-close').addEventListener('click', close);
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) close();
        });
    };

    function getFlagIcons(item) {
        const languages = new Set();
        Object.keys(item).forEach(key => {
            if (key.startsWith('name.')) {
                const code = key.split('.')[1];
                languages.add(code);
            }
        });

        return Array.from(languages).map(l => {
            const flag = LANG_FLAGS[l] || l;
            return `<span title="${l}" style="margin-right:4px; cursor:help;">${flag}</span>`;
        }).join('');
    }

    function getTransitionLabel(key, lang) {
        if (TRANSLATIONS[key] && TRANSLATIONS[key][lang]) {
            return TRANSLATIONS[key][lang];
        }
        return TRANSLATIONS[key]['English'] || key;
    }

    function formatText(text) {
        if (!text) return '';
        return text
            .replace(/\[b\]/g, '')
            .replace(/\[\/b\]/g, '')
            .replace(/\[i\]/g, '')
            .replace(/\[\/i\]/g, '');
    }

    function getPlaceholderSvg(text) {
        return "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='150' height='150' viewBox='0 0 150 150'%3E%3Crect width='150' height='150' fill='%232f2c3d'/%3E%3Ctext x='50%25' y='50%25' dominant-baseline='middle' text-anchor='middle' font-family='sans-serif' font-size='20' fill='%23555'%3E" + text + "%3C/text%3E%3C/svg%3E";
    }

    // --- Rendering ---

    function renderScenarios(data, totalCount, containerId, lang = 'English') {
        const container = document.getElementById(containerId);
        if (!container) return;

        // Show spinner during render
        container.innerHTML = '<div class="text-center w-100 py-5"><div class="spinner"><div class="bounce1"></div><div class="bounce2"></div><div class="bounce3"></div></div></div>';

        setTimeout(() => {
            container.innerHTML = '';

            // Header: "X SCENARIOS (+Y SCENARIOS FILTERED OUT)"
            const count = data ? data.length : 0;
            const filteredOut = totalCount - count;

            const lblScenarios = getTransitionLabel('LabelScenarios', lang).toUpperCase();
            const lblFiltered = getTransitionLabel('LabelFilteredOut', lang).toUpperCase();

            let headerText = `${count} ${lblScenarios}`;
            if (filteredOut > 0) {
                headerText += ` <span style="opacity:0.7">(+${filteredOut} ${lblScenarios} ${lblFiltered})</span>`;
            }

            // Create Header Element
            const headerDiv = document.createElement('div');
            headerDiv.className = 'w-100 mb-3';
            headerDiv.innerHTML = `<h5 class="scenario-list-header text-light">${headerText}</h5>`;
            container.appendChild(headerDiv);

            if (!data || data.length === 0) {
                const msg = getTransitionLabel('NoContent', lang);
                container.innerHTML += `<p class="text-center">${msg}</p>`;
                return;
            }

            const noImageText = getTransitionLabel('NoImage', lang);
            const placeholderSvg = getPlaceholderSvg(noImageText);

            data.forEach(item => {
                if (item.hidden === 'True') return;

                const card = document.createElement('div');
                card.className = 'scenario-card';

                const name = getLocalizedValue(item, 'name', lang) || item.id;
                let desc = getLocalizedValue(item, 'synopsys', lang) || getLocalizedValue(item, 'description', lang) || '';
                // Changed from 'authors' to 'authors_short' as requested
                let authors = getLocalizedValue(item, 'authors_short', lang) || getLocalizedValue(item, 'authors', lang) || 'Community';

                // Format text
                desc = formatText(desc);
                authors = formatText(authors);

                let authorsDisplay = authors;
                if (authors.length > 50) {
                    authorsDisplay = authors.substring(0, 50) + '...';
                }
                const authorsTitle = authors.replace(/<[^>]+>/g, '').replace(/"/g, '&quot;');

                // Image logic
                let imgHtml = `<img src="${placeholderSvg}" alt="${name}" class="img-placeholder">`;
                if (item.url && item.image) {
                    imgHtml = `<img src="${item.url}${item.image}" alt="${name}" loading="lazy" onerror="this.onerror=null;this.src='${placeholderSvg}';">`;
                }

                const flagsHtml = getFlagIcons(item);
                const min = item.lengthmin || '?';
                const max = item.lengthmax || '?';

                // Difficulty Stars
                const diffStars = getHardwareStars(item.difficulty);
                const diffLabel = getTransitionLabel('Difficulty', lang);

                // Rating Stars
                const ratingStars = getRatingStars(item.rating);

                // Packs logic
                let packsHtml = '';
                if (item.packs) {
                    const packsLabel = getTransitionLabel('Expansions', lang);
                    packsHtml = `<div class="meta-item"><span title="${packsLabel}">${packsLabel}: ${item.packs}</span></div>`;
                }

                // New items
                const durationLabel = getTransitionLabel('Duration', lang);
                const avgDurationLabel = getTransitionLabel('AverageDuration', lang);
                const playCountLabel = getTransitionLabel('PlayCount', lang);
                const winRatioLabel = getTransitionLabel('WinRatio', lang);
                const communityRatingLabel = getTransitionLabel('CommunityRating', lang); // New Label
                const authorLabel = getTransitionLabel('Author', lang);
                const updatedLabel = getTransitionLabel('LastUpdated', lang);
                const labelPlayerCount = getTransitionLabel('LabelPlayerCount', lang);

                const playCount = item.play_count || 0;
                const avgDuration = item.duration ? Math.round(item.duration) : 0;
                const winRatio = item.win_ratio ? Math.round(item.win_ratio * 100) : 0;
                const ratingValue = parseFloat(item.rating) || 0;

                card.innerHTML = `
                    <div class="scenario-image">${imgHtml}</div>
                    <div class="scenario-details">
                        <h4 class="scenario-title">
                            ${name} 
                        </h4>
                        <div class="scenario-meta">
                            <div class="d-flex flex-wrap align-items-center w-100">
                                <div class="meta-item"><span title="${durationLabel}">${durationLabel}: ${min}-${max} min</span></div>
                                <div class="meta-item"><span title="Difficulty">${diffLabel}: <span class="text-warning">${diffStars.filled}<span style="opacity:0.5">${diffStars.empty}</span></span></span></div>
                                ${(item.minhero && item.maxhero) ? `<div class="meta-item"><span title="${labelPlayerCount}">${labelPlayerCount}: ${item.minhero}-${item.maxhero}</span></div>` : ''}
                                <div class="meta-item"><span title="${authorsTitle}">${authorLabel}: ${authorsDisplay}</span></div>
                                ${item.latest_update ? `<div class="meta-item"><span title="${updatedLabel}">${updatedLabel}: ${item.latest_update.split('T')[0]}</span></div>` : ''}
                            </div>
                            <div class="d-flex flex-wrap align-items-center mt-2 w-100">
                                <div class="meta-item">${communityRatingLabel}:&nbsp;<span class="text-warning" title="Rating: ${ratingValue.toFixed(1)}/10">${ratingStars.filled}<span style="opacity:0.5">${ratingStars.empty}</span></span></div>
                                <div class="meta-item">${avgDurationLabel}: ${avgDuration} min</div>
                                <div class="meta-item">${playCountLabel}: ${playCount}</div>
                                <div class="meta-item">${winRatioLabel}: ${winRatio}%</div>
                                <div class="meta-item meta-langs">${flagsHtml}</div>
                                ${packsHtml}
                            </div>
                        </div>
                        <div class="scenario-description">${desc.split('\n')[0]}...</div>
                    </div>
                `;
                container.appendChild(card);
            });
        }, 50);
    }

    function renderPacks(data, containerId, lang, searchTerm = '') {
        const container = document.getElementById(containerId);
        if (!container) return;

        // Force container class to match scenarios for consistent styling
        container.className = 'scenarios-container';
        container.classList.remove('d-flex', 'flex-wrap'); // Remove badge-style classes if present

        container.innerHTML = '';
        // Filter by Search Term
        let filtered = data || [];

        if (searchTerm) {
            const term = searchTerm.toLowerCase();
            filtered = filtered.filter(item => {
                const name = (getLocalizedValue(item, 'name', lang) || item.id).toLowerCase();
                return name.includes(term);
            });
        }

        // Header
        const count = filtered.length;
        const filteredOut = (data ? data.length : 0) - count;

        const lblPacks = getTransitionLabel('LabelPacks', lang).toUpperCase();
        const lblFiltered = getTransitionLabel('LabelFilteredOut', lang).toUpperCase();

        let headerText = `${count} ${lblPacks}`;
        if (filteredOut > 0) {
            headerText += ` <span style="opacity:0.7">(+${filteredOut} ${lblPacks} ${lblFiltered})</span>`;
        }

        const headerDiv = document.createElement('div');
        headerDiv.className = 'w-100 mb-3';
        headerDiv.innerHTML = `<h5 class="scenario-list-header text-light">${headerText}</h5>`;
        container.appendChild(headerDiv);

        if (filtered.length === 0) {
            const msg = getTransitionLabel('NoPacks', lang);
            container.innerHTML += `<p class="text-center">${msg}</p>`;
            return;
        }

        // Sort Packs Alphabetically by Name
        filtered.sort((a, b) => {
            const nameA = getLocalizedValue(a, 'name', lang) || a.id;
            const nameB = getLocalizedValue(b, 'name', lang) || b.id;
            return nameA.localeCompare(nameB, undefined, { sensitivity: 'base' });
        });

        const noImageText = getTransitionLabel('NoImage', lang);
        const placeholderSvg = getPlaceholderSvg(noImageText);

        filtered.forEach(item => {
            const card = document.createElement('div');
            card.className = 'scenario-card';

            const name = getLocalizedValue(item, 'name', lang) || item.id;
            let desc = getLocalizedValue(item, 'description', lang) || '';

            // Format text
            desc = formatText(desc);

            // Image logic
            let imgHtml = `<img src="${placeholderSvg}" alt="${name}" class="img-placeholder">`;
            if (item.url && item.image) {
                imgHtml = `<img src="${item.url}${item.image}" alt="${name}" loading="lazy" onerror="this.onerror=null;this.src='${placeholderSvg}';">`;
            }

            card.innerHTML = `
                <div class="scenario-image">${imgHtml}</div>
                <div class="scenario-details">
                    <h4 class="scenario-title">${name}</h4>
                    <div class="scenario-description">${desc}</div>
                </div>
            `;
            container.appendChild(card);
        });
    }

    // --- Main ---

    // --- Main ---

    function init() {
        const loaded = { D2E: false, MOM: false };

        // Determine language
        const pageLang = document.documentElement.lang;
        let userLang = 'English';
        if (pageLang === 'es') userLang = 'Spanish';
        else if (pageLang === 'de') userLang = 'German';
        else if (pageLang === 'fr') userLang = 'French';
        else if (pageLang === 'it') userLang = 'Italian';
        else if (pageLang === 'pl') userLang = 'Polish';
        else if (pageLang === 'pt') userLang = 'Portuguese';

        // Parse Hash Helper
        const parseHash = () => {
            const hash = window.location.hash.substring(1); // Remove #
            const params = new URLSearchParams(hash);
            return {
                type: params.get('type') || (hash.includes('type=mom') ? 'mom' : 'd2e'), // Fallback for old style
                search: params.get('search') || '',
                duration: params.get('duration') || '',
                difficulty: params.get('difficulty') || '',
                language: params.get('language') || '',
                expansion: params.get('expansions') ? params.get('expansions').split(',') : [],
                author: params.get('author') || '',

                // New filters
                sortField: params.get('sort') || 'last_updated',
                sortDir: params.get('sort_direction') || 'desc',
                minRating: params.get('min_rating') || '',
                minWinRatio: params.get('min_win_ratio') || '',
                avgDuration: params.get('avg_duration') || '',
                minPlayCount: params.get('min_play_count') || '',
                minPlayers: params.get('min_players') || '',
                maxPlayers: params.get('max_players') || ''
            };
        };

        const initialParams = parseHash();

        // Filter Logic
        const state = {
            D2E: {
                data: [],
                packs: [],
                filters: {
                    search: initialParams.search,
                    duration: initialParams.duration,
                    difficulty: initialParams.difficulty,
                    language: initialParams.language,
                    expansion: initialParams.expansion,
                    author: initialParams.author,
                    minRating: initialParams.minRating,
                    minWinRatio: initialParams.minWinRatio,
                    avgDuration: initialParams.avgDuration,
                    minPlayCount: initialParams.minPlayCount,
                    minPlayers: initialParams.minPlayers,
                    maxPlayers: initialParams.maxPlayers
                },
                sort: {
                    field: initialParams.sortField,
                    dir: initialParams.sortDir
                }
            },
            MOM: {
                data: [],
                packs: [],
                filters: {
                    search: initialParams.search,
                    duration: initialParams.duration,
                    difficulty: initialParams.difficulty,
                    language: initialParams.language,
                    expansion: initialParams.expansion,
                    author: initialParams.author,
                    minRating: initialParams.minRating,
                    minWinRatio: initialParams.minWinRatio,
                    avgDuration: initialParams.avgDuration,
                    minPlayCount: initialParams.minPlayCount,
                    minPlayers: initialParams.minPlayers,
                    maxPlayers: initialParams.maxPlayers
                },
                sort: {
                    field: initialParams.sortField,
                    dir: initialParams.sortDir
                }
            }
        };

        const updateHash = (type) => {
            const s = state[type];
            const params = new URLSearchParams();
            params.set('type', type.toLowerCase());

            if (s.filters.search) params.set('search', s.filters.search);
            if (s.filters.duration) params.set('duration', s.filters.duration);
            if (s.filters.difficulty) params.set('difficulty', s.filters.difficulty);
            if (s.filters.language) params.set('language', s.filters.language);
            if (s.filters.author) params.set('author', s.filters.author);
            if (s.filters.expansion && s.filters.expansion.length > 0) params.set('expansions', s.filters.expansion.join(','));
            if (s.filters.minRating) params.set('min_rating', s.filters.minRating);
            if (s.filters.minWinRatio) params.set('min_win_ratio', s.filters.minWinRatio);
            if (s.filters.avgDuration) params.set('avg_duration', s.filters.avgDuration);
            if (s.filters.minPlayCount) params.set('min_play_count', s.filters.minPlayCount);

            if (s.sort.field !== 'last_updated') params.set('sort', s.sort.field);
            if (s.sort.dir !== 'desc') params.set('sort_direction', s.sort.dir);

            // Replace history state to avoid clogging back button
            const newHash = '#' + params.toString();
            if (window.location.hash !== newHash) {
                window.history.replaceState(null, null, newHash);
            }
        };

        const applyFilters = (type, userLang) => {
            updateHash(type);
            const s = state[type];
            let filtered = s.data.slice(); // Copy

            // Search (Name)
            if (s.filters.search) {
                const term = s.filters.search.toLowerCase();
                filtered = filtered.filter(item => {
                    const name = (getLocalizedValue(item, 'name', userLang) || item.id).toLowerCase();
                    return name.includes(term);
                });
            }

            // Duration
            if (s.filters.duration) {
                const [min, max] = s.filters.duration.split('-').map(Number);
                filtered = filtered.filter(item => {
                    const lMin = parseFloat(item.lengthmin) || 0;
                    const lMax = parseFloat(item.lengthmax) || 0;
                    if (s.filters.duration === '180+') return lMax >= 180;
                    return lMax >= min && lMin <= max;
                });
            }

            // Difficulty
            if (s.filters.difficulty !== '') {
                const targetStars = parseInt(s.filters.difficulty, 10);
                filtered = filtered.filter(item => {
                    const val = parseFloat(item.difficulty) || 0;
                    const stars = Math.round(val / 0.2);
                    const clampedStars = Math.max(0, Math.min(5, stars));
                    return clampedStars === targetStars;
                });
            }

            // Language
            if (s.filters.language) {
                filtered = filtered.filter(item => {
                    const hasExplicitLang = Object.keys(item).includes(`name.${s.filters.language}`);
                    if (hasExplicitLang) return true;

                    // Check default language
                    if (item.defaultlanguage && item.defaultlanguage === s.filters.language) return true;

                    // Fallback: If filtering for English and no default language is specified, include it
                    // This excludes cases where defaultlanguage is set to something else (e.g. Spanish)
                    if (s.filters.language === 'English' && !item.defaultlanguage) return true;

                    return false;
                });
            }

            // Author
            if (s.filters.author) {
                filtered = filtered.filter(item => {
                    let auth = getLocalizedValue(item, 'authors_short', userLang) || getLocalizedValue(item, 'authors', userLang) || 'Community';
                    auth = formatText(auth);
                    return auth === s.filters.author;
                });
            }

            // Expansion
            if (s.filters.expansion && s.filters.expansion.length > 0) {
                filtered = filtered.filter(item => {
                    // Check if 'NONE' is selected
                    const noneSelected = s.filters.expansion.includes('NONE');
                    const hasPacks = item.packs && item.packs.trim().length > 0;

                    if (noneSelected && !hasPacks) return true;

                    // Basic intersection logic
                    if (!hasPacks) return false;
                    const itemPacks = item.packs.split(' ');
                    // Check intersection: Keep if item has ANY of the selected packs
                    return s.filters.expansion.some(code => itemPacks.includes(code));
                });
            }

            // Min Rating
            if (s.filters.minRating) {
                const minR = parseFloat(s.filters.minRating);
                filtered = filtered.filter(item => {
                    const r = parseFloat(item.rating) || 0;
                    return r >= minR;
                });
            }

            // Min Win Ratio
            if (s.filters.minWinRatio) {
                const minW = parseFloat(s.filters.minWinRatio);
                filtered = filtered.filter(item => {
                    const w = item.win_ratio ? item.win_ratio * 100 : 0;
                    return w >= minW;
                });
            }

            // Average Duration
            if (s.filters.avgDuration) {
                const minD = parseFloat(s.filters.avgDuration);
                filtered = filtered.filter(item => {
                    const d = parseFloat(item.duration) || 0;
                    return d >= minD;
                });
            }

            // Min Play Count
            if (s.filters.minPlayCount) {
                const minPC = parseInt(s.filters.minPlayCount);
                filtered = filtered.filter(item => {
                    const pc = parseInt(item.play_count) || 0;
                    return pc >= minPC;
                });
            }

            // Min Players
            if (s.filters.minPlayers) {
                const minP = parseInt(s.filters.minPlayers);
                filtered = filtered.filter(item => {
                    // Strict Subset: Scenario Min must be >= Filter Min
                    // Default to 0 if missing, so 0 >= X fails (unless X is 0)
                    const mp = item.minhero ? parseInt(item.minhero) : 0;
                    return mp >= minP;
                });
            }

            // Max Players
            if (s.filters.maxPlayers) {
                const maxP = parseInt(s.filters.maxPlayers);
                filtered = filtered.filter(item => {
                    // Strict Subset: Scenario Max must be <= Filter Max
                    // Default to 99 if missing, so 99 <= X fails
                    const mp = item.maxhero ? parseInt(item.maxhero) : 99;
                    return mp <= maxP;
                });
            }

            // Sorting
            filtered.sort((a, b) => {
                let valA, valB;
                const field = s.sort.field;

                if (field === 'last_updated') {
                    // Date comparison
                    valA = a.latest_update || '';
                    valB = b.latest_update || '';
                } else if (field === 'author') {
                    // Author comparison
                    valA = getLocalizedValue(a, 'authors_short', userLang) || getLocalizedValue(a, 'authors', userLang) || '';
                    valB = getLocalizedValue(b, 'authors_short', userLang) || getLocalizedValue(b, 'authors', userLang) || '';
                } else if (field === 'rating') {
                    valA = parseFloat(a.rating) || 0;
                    valB = parseFloat(b.rating) || 0;
                } else if (field === 'play_count') {
                    valA = parseInt(a.play_count) || 0;
                    valB = parseInt(b.play_count) || 0;
                } else if (field === 'duration') {
                    valA = parseFloat(a.duration) || 0;
                    valB = parseFloat(b.duration) || 0;
                } else if (field === 'win_ratio') {
                    valA = parseFloat(a.win_ratio) || 0;
                    valB = parseFloat(b.win_ratio) || 0;
                } else {
                    // Name comparison (default)
                    valA = getLocalizedValue(a, 'name', userLang) || a.id;
                    valB = getLocalizedValue(b, 'name', userLang) || b.id;
                }

                if (typeof valA === 'string' && typeof valB === 'string') {
                    return s.sort.dir === 'asc'
                        ? valA.localeCompare(valB, undefined, { sensitivity: 'base' })
                        : valB.localeCompare(valA, undefined, { sensitivity: 'base' });
                }

                if (valA < valB) return s.sort.dir === 'asc' ? -1 : 1;
                if (valA > valB) return s.sort.dir === 'asc' ? 1 : -1;
                return 0;
            });

            // Update Hash
            const params = new URLSearchParams();
            if (s.filters.search) params.set('search', s.filters.search);
            if (s.filters.duration) params.set('duration', s.filters.duration);
            if (s.filters.difficulty) params.set('difficulty', s.filters.difficulty);
            if (s.filters.language) params.set('language', s.filters.language);
            if (s.filters.expansion && s.filters.expansion.length > 0) params.set('expansion', s.filters.expansion.join(' '));
            if (s.filters.author) params.set('author', s.filters.author);
            if (s.filters.minRating) params.set('min_rating', s.filters.minRating);
            if (s.filters.minWinRatio) params.set('min_win_ratio', s.filters.minWinRatio);
            if (s.filters.avgDuration) params.set('avg_duration', s.filters.avgDuration);
            if (s.filters.minPlayCount) params.set('min_play_count', s.filters.minPlayCount);
            if (s.filters.minPlayers) params.set('min_players', s.filters.minPlayers);
            if (s.filters.maxPlayers) params.set('max_players', s.filters.maxPlayers);

            if (s.sort.field !== 'last_updated') params.set('sort', s.sort.field);
            if (s.sort.dir !== 'desc') params.set('sort_direction', s.sort.dir);

            if (type.toLowerCase() === 'mom') params.set('type', 'mom');

            window.location.hash = params.toString();

            renderScenarios(filtered, (s.data ? s.data.length : 0), `scenarios-${type.toLowerCase()}-list`, userLang);

            // Filter Content Packs List
            if (s.packs) {
                renderPacks(s.packs, `scenarios-${type.toLowerCase()}-packs`, userLang, s.filters.search);
            }
        };

        const renderFilters = (type, data, packs, userLang) => {
            const containerId = `scenarios-${type.toLowerCase()}-list`;
            const listContainer = document.getElementById(containerId);
            if (!listContainer) return;

            // Check if filters already exist
            const parent = listContainer.parentElement;
            if (parent.querySelector('.filter-bar')) return;

            // Collect Languages, Authors & Expansions
            const languages = new Set(['English']);
            const expansionCodes = new Set();
            const authors = new Set();

            data.forEach(item => {
                Object.keys(item).forEach(key => {
                    if (key.startsWith('name.')) {
                        languages.add(key.split('.')[1]);
                    }
                });
                // Collect expansions from item.packs string (space separated)
                if (item.packs) {
                    item.packs.split(' ').forEach(ex => {
                        if (ex.trim()) expansionCodes.add(ex.trim());
                    });
                }
                // Collect Authors
                let auth = getLocalizedValue(item, 'authors_short', userLang) || getLocalizedValue(item, 'authors', userLang) || 'Community';
                // Sanitize [b], [i], etc.
                if (auth) {
                    auth = formatText(auth);
                    authors.add(auth);
                }
            });

            // Resolve Expansions
            const expansionOptions = [];
            expansionCodes.forEach(code => {
                // Find pack name
                const pack = packs.find(p => p.id === code);
                let name = code;
                if (pack) {
                    // Try to get localized name
                    name = getLocalizedValue(pack, 'name', userLang) || code;
                }
                expansionOptions.push({ code, name });
            });
            expansionOptions.sort((a, b) => a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }));

            // Sort Authors
            const authorOptions = Array.from(authors).sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' }));

            // Translations
            const lblDuration = getTransitionLabel('Duration', userLang);
            const lblDifficulty = getTransitionLabel('Difficulty', userLang);
            const lblLanguage = getTransitionLabel('Language', userLang);
            const lblExpansions = getTransitionLabel('Expansions', userLang);
            const lblAny = getTransitionLabel('Any', userLang);
            const lblSortBy = getTransitionLabel('SortBy', userLang);
            const lblName = getTransitionLabel('Name', userLang);
            const lblLastUpdated = getTransitionLabel('LastUpdated', userLang);
            const lblAuthor = getTransitionLabel('Author', userLang);
            const lblAsc = getTransitionLabel('Ascending', userLang);
            const lblDesc = getTransitionLabel('Descending', userLang);

            const lblSearch = getTransitionLabel('Search', userLang);
            const lblSearchHint = getTransitionLabel('SearchHint', userLang);

            const lblRating = getTransitionLabel('Rating', userLang);
            const lblPlayCount = getTransitionLabel('PlayCount', userLang);
            const lblWinRatio = getTransitionLabel('WinRatio', userLang);

            const lblCommunityRating = getTransitionLabel('CommunityRating', userLang); // Re-use label for Title if needed, or just Rating
            const lblAvgDuration = getTransitionLabel('AverageDuration', userLang);
            const lblReset = getTransitionLabel('Reset', userLang);
            const lblPlayCountLabel = getTransitionLabel('PlayCount', userLang);
            const lblPlayCountTooltip = getTransitionLabel('PlayCountTooltip', userLang);
            const lblMinPlayerCount = getTransitionLabel('LabelMinPlayerCount', userLang);
            const lblMaxPlayerCount = getTransitionLabel('LabelMaxPlayerCount', userLang);

            // Calculate Avg Duration Options (30+, 60+ ... 600+)
            let avgDurOptions = `<option value="">${lblAny}</option>`;
            for (let i = 30; i <= 600; i += 30) {
                avgDurOptions += `<option value="${i}">${i}+ min</option>`;
            }

            // Calculate Play Count Options (10-50, 100-2000)
            let playCountOptions = `<option value="">${lblAny}</option>`;
            for (let i = 10; i <= 50; i += 10) {
                playCountOptions += `<option value="${i}">${i}+</option>`;
            }
            for (let i = 100; i <= 2000; i += 100) {
                playCountOptions += `<option value="${i}">${i}+</option>`;
            }


            const filterBar = document.createElement('div');
            // Added filter-bar-sticky class
            filterBar.className = 'filter-bar filter-bar-sticky mb-3 p-3 bg-dark rounded';

            // Row 1
            const row1 = document.createElement('div');
            row1.className = 'd-flex flex-wrap align-items-center';
            row1.style.gap = '1rem';

            const lblNone = getTransitionLabel('LabelNone', userLang);

            // Expansion Dropdown - Prepend "None" option
            const noneOptionHtml = `
                <div class="form-check">
                    <input class="form-check-input exp-checkbox" type="checkbox" value="NONE" id="exp-${type}-NONE">
                    <label class="form-check-label small" for="exp-${type}-NONE" style="cursor:pointer;">
                        ${lblNone}
                    </label>
                </div>
                <div class="dropdown-divider border-secondary my-1"></div>
            `;

            row1.innerHTML = `
                <style>
                    .filter-search::placeholder {
                        color: rgba(255, 255, 255, 0.5) !important;
                    }
                </style>
                <div class="d-flex flex-column" style="min-width: 150px;">
                    <label class="mb-1 text-muted small">${lblSearch}</label>
                    <input type="text" class="form-control form-control-sm bg-secondary text-light border-0 filter-search" placeholder="${lblSearchHint}...">
                </div>
                <div class="d-flex flex-column">
                    <label class="mb-1 text-muted small">${lblDuration} (Manifest)</label>
                    <select class="form-control form-control-sm bg-secondary text-light border-0 filter-duration">
                        <option value="">${lblAny}</option>
                        <option value="0-60">&lt; 60 min</option>
                        <option value="60-120">60 - 120 min</option>
                        <option value="120-180">120 - 180 min</option>
                        <option value="180+">&gt; 180 min</option>
                    </select>
                </div>
                <div class="d-flex flex-column">
                    <label class="mb-1 text-muted small">${lblDifficulty}</label>
                    <select class="form-control form-control-sm bg-secondary text-light border-0 filter-difficulty">
                        <option value="">${lblAny}</option>
                        <option value="0">0 &#9733;</option>
                        <option value="1">1 &#9733;</option>
                        <option value="2">2 &#9733;</option>
                        <option value="3">3 &#9733;</option>
                        <option value="4">4 &#9733;</option>
                        <option value="5">5 &#9733;</option>
                    </select>
                </div>
                <div class="d-flex flex-column">
                    <label class="mb-1 text-muted small">${lblLanguage}</label>
                    <select class="form-control form-control-sm bg-secondary text-light border-0 filter-language">
                        <option value="">${lblAny}</option>
                        ${Array.from(languages).sort().map(l => `<option value="${l}">${LANG_FLAGS[l] || ''} ${l}</option>`).join('')}
                    </select>
                </div>
                <!-- Author Filter -->
                <div class="d-flex flex-column" style="max-width: 150px;">
                    <label class="mb-1 text-muted small">${lblAuthor}</label>
                    <select class="form-control form-control-sm bg-secondary text-light border-0 filter-author">
                        <option value="">${lblAny}</option>
                        ${authorOptions.map(a => `<option value="${a}">${a}</option>`).join('')}
                    </select>
                </div>
                 <!-- Expansions Multi-Select -->
                <div class="d-flex flex-column position-relative" style="min-width: 180px;">
                    <label class="mb-1 text-muted small">${lblExpansions}</label>
                    <button class="form-control form-control-sm bg-secondary text-light border-0 text-left d-flex justify-content-between align-items-center exp-dropdown-btn">
                        <span>${lblAny}</span>
                        <span class="small">&#9662;</span>
                    </button>
                    <div class="exp-dropdown-menu bg-dark border border-secondary p-2 rounded shadow text-light" style="display:none; position:absolute; top: 100%; left:0; width:100%; z-index:1000; max-height: 300px; overflow-y: auto;">
                        ${noneOptionHtml}
                        ${expansionOptions.map(e => `
                            <div class="form-check">
                                <input class="form-check-input exp-checkbox" type="checkbox" value="${e.code}" id="exp-${type}-${e.code}">
                                <label class="form-check-label small" for="exp-${type}-${e.code}" style="cursor:pointer;">
                                    ${e.name}
                                </label>
                            </div>
                        `).join('')}
                    </div>
                </div>
                
                </div>
            `;

            // Row 2
            const row2 = document.createElement('div');
            row2.className = 'd-flex flex-wrap align-items-center mt-3 pt-3 border-top';
            row2.style.borderColor = '#444';
            row2.style.gap = '1rem';

            row2.innerHTML = `
                <!-- Min Player Count -->
                <div class="d-flex flex-column">
                    <label class="mb-1 text-muted small">${lblMinPlayerCount}</label>
                    <select class="form-control form-control-sm bg-secondary text-light border-0 filter-min-players">
                        <option value="">${lblAny}</option>
                        <option value="1">1</option>
                        <option value="2">2</option>
                        <option value="3">3</option>
                        <option value="4">4</option>
                        <option value="5">5</option>
                        <option value="6">6</option>
                    </select>
                </div>
                <!-- Max Player Count -->
                <div class="d-flex flex-column">
                    <label class="mb-1 text-muted small">${lblMaxPlayerCount}</label>
                    <select class="form-control form-control-sm bg-secondary text-light border-0 filter-max-players">
                        <option value="">${lblAny}</option>
                        <option value="1">1</option>
                        <option value="2">2</option>
                        <option value="3">3</option>
                        <option value="4">4</option>
                        <option value="5">5</option>
                        <option value="6">6</option>
                    </select>
                </div>
                <!-- Min Rating -->
                <div class="d-flex flex-column">
                    <label class="mb-1 text-muted small">${lblRating} (Min)</label>
                    <select class="form-control form-control-sm bg-secondary text-light border-0 filter-min-rating">
                        <option value="">${lblAny}</option>
                        <option value="9">9+ &#9733;</option>
                        <option value="8">8+ &#9733;</option>
                        <option value="7">7+ &#9733;</option>
                        <option value="6">6+ &#9733;</option>
                        <option value="5">5+ &#9733;</option>
                    </select>
                </div>
                <!-- Min Win Ratio -->
                <div class="d-flex flex-column">
                    <label class="mb-1 text-muted small">${lblWinRatio} (Min)</label>
                    <select class="form-control form-control-sm bg-secondary text-light border-0 filter-min-win-ratio">
                        <option value="">${lblAny}</option>
                        <option value="90">90%+</option>
                        <option value="75">75%+</option>
                        <option value="50">50%+</option>
                        <option value="25">25%+</option>
                    </select>
                </div>
                <!-- Average Duration -->
                <div class="d-flex flex-column">
                    <label class="mb-1 text-muted small">${lblAvgDuration}</label>
                    <select class="form-control form-control-sm bg-secondary text-light border-0 filter-avg-duration">
                        ${avgDurOptions}
                    </select>
                </div>
                <!-- Play Count (Min) -->
                <div class="d-flex flex-column">
                    <label class="mb-1 text-muted small">
                        ${lblPlayCountLabel}
                        <i class="fas fa-info-circle ml-1 info-playcount" style="cursor: pointer;"></i>
                    </label>
                    <select class="form-control form-control-sm bg-secondary text-light border-0 filter-min-play-count">
                        ${playCountOptions}
                    </select>
                </div>
                <!-- Sort (Moved to Row 2) -->
                <div class="d-flex flex-column ml-auto border-left pl-3" style="border-color: #444 !important;">
                    <label class="mb-1 text-muted small">${lblSortBy}</label>
                    <div class="d-flex" style="gap: 0.5rem">
                         <select class="form-control form-control-sm bg-secondary text-light border-0 sort-field" style="width: auto;">
                            <option value="last_updated">${lblLastUpdated}</option>
                            <option value="name">${lblName}</option>
                            <option value="author">${lblAuthor}</option>
                            <option value="rating">${lblRating}</option>
                            <option value="play_count">${lblPlayCount}</option>
                            <option value="duration">${lblDuration}</option>
                            <option value="win_ratio">${lblWinRatio}</option>
                        </select>
                        <select class="form-control form-control-sm bg-secondary text-light border-0 sort-dir" style="width: auto;">
                            <option value="asc">${lblAsc}</option>
                            <option value="desc">${lblDesc}</option>
                        </select>
                    </div>
                </div>
                <!-- Reset Button -->
                <div class="d-flex flex-column ml-3" style="margin-top: auto; margin-bottom: 2px;">
                     <button class="btn btn-sm btn-outline-danger border-0 btn-reset" title="${lblReset}">
                        <i class="fas fa-undo mr-1"></i> ${lblReset}
                     </button>
                </div>
            `;

            filterBar.appendChild(row1);
            filterBar.appendChild(row2);
            parent.insertBefore(filterBar, listContainer);

            // Set initial sort values from state
            filterBar.querySelector('.sort-field').value = state[type].sort.field;
            filterBar.querySelector('.sort-dir').value = state[type].sort.dir;

            // Set initial filter values from state
            filterBar.querySelector('.filter-search').value = state[type].filters.search;
            filterBar.querySelector('.filter-duration').value = state[type].filters.duration;
            filterBar.querySelector('.filter-difficulty').value = state[type].filters.difficulty;
            filterBar.querySelector('.filter-language').value = state[type].filters.language;
            filterBar.querySelector('.filter-author').value = state[type].filters.author;
            filterBar.querySelector('.filter-min-rating').value = state[type].filters.minRating;
            filterBar.querySelector('.filter-min-win-ratio').value = state[type].filters.minWinRatio;
            filterBar.querySelector('.filter-avg-duration').value = state[type].filters.avgDuration;
            filterBar.querySelector('.filter-min-play-count').value = state[type].filters.minPlayCount;
            filterBar.querySelector('.filter-min-players').value = state[type].filters.minPlayers;
            filterBar.querySelector('.filter-max-players').value = state[type].filters.maxPlayers;

            // Listeners
            filterBar.querySelector('.filter-search').addEventListener('input', (e) => {
                state[type].filters.search = e.target.value;
                applyFilters(type, userLang);
            });
            filterBar.querySelector('.filter-duration').addEventListener('change', (e) => {
                state[type].filters.duration = e.target.value;
                applyFilters(type, userLang);
            });
            filterBar.querySelector('.filter-difficulty').addEventListener('change', (e) => {
                state[type].filters.difficulty = e.target.value;
                applyFilters(type, userLang);
            });
            filterBar.querySelector('.filter-language').addEventListener('change', (e) => {
                state[type].filters.language = e.target.value;
                applyFilters(type, userLang);
            });
            filterBar.querySelector('.filter-author').addEventListener('change', (e) => {
                state[type].filters.author = e.target.value;
                applyFilters(type, userLang);
            });
            filterBar.querySelector('.filter-min-rating').addEventListener('change', (e) => {
                state[type].filters.minRating = e.target.value;
                applyFilters(type, userLang);
            });
            filterBar.querySelector('.filter-min-win-ratio').addEventListener('change', (e) => {
                state[type].filters.minWinRatio = e.target.value;
                applyFilters(type, userLang);
            });
            filterBar.querySelector('.filter-avg-duration').addEventListener('change', (e) => {
                state[type].filters.avgDuration = e.target.value;
                applyFilters(type, userLang);
            });
            filterBar.querySelector('.filter-min-play-count').addEventListener('change', (e) => {
                state[type].filters.minPlayCount = e.target.value;
                applyFilters(type, userLang);
            });

            filterBar.querySelector('.filter-min-players').addEventListener('change', (e) => {
                state[type].filters.minPlayers = e.target.value;
                applyFilters(type, userLang);
            });

            filterBar.querySelector('.filter-max-players').addEventListener('change', (e) => {
                state[type].filters.maxPlayers = e.target.value;
                applyFilters(type, userLang);
            });

            // Info Dialog Listener
            filterBar.querySelector('.info-playcount').addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                showInfoDialog(lblPlayCountLabel, lblPlayCountTooltip);
            });

            // Expansion Logic
            const btn = filterBar.querySelector('.exp-dropdown-btn');
            const menu = filterBar.querySelector('.exp-dropdown-menu');
            const checkboxes = filterBar.querySelectorAll('.exp-checkbox');
            const btnSpan = btn.querySelector('span');

            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                menu.style.display = menu.style.display === 'none' ? 'block' : 'none';
            });

            // Close on outside click
            document.addEventListener('click', (e) => {
                if (!filterBar.contains(e.target)) {
                    menu.style.display = 'none';
                }
            });

            checkboxes.forEach(cb => {
                // Initialize Checkbox state
                if (state[type].filters.expansion.includes(cb.value)) {
                    cb.checked = true;
                }

                cb.addEventListener('change', () => {
                    if (cb.checked) {
                        if (cb.value === 'NONE') {
                            // If NONE selected, uncheck everything else
                            checkboxes.forEach(c => {
                                if (c !== cb) c.checked = false;
                            });
                        } else {
                            // If other selected, uncheck NONE
                            checkboxes.forEach(c => {
                                if (c.value === 'NONE') c.checked = false;
                            });
                        }
                    }

                    const selected = Array.from(checkboxes).filter(c => c.checked).map(c => c.value);
                    state[type].filters.expansion = selected;
                    applyFilters(type, userLang);

                    // Update Button Text
                    updatePacksButtonText(selected);
                });
            });

            // Initial Button Text Update
            const updatePacksButtonText = (selected) => {
                if (selected.length === 0) {
                    btnSpan.textContent = lblAny;
                } else if (selected.length === 1) {
                    const code = selected[0];
                    const opt = expansionOptions.find(o => o.code === code);
                    const name = opt ? opt.name : code;
                    // Truncate if long
                    btnSpan.textContent = name.length > 20 ? name.substring(0, 18) + '...' : name;
                } else {
                    btnSpan.textContent = `${selected.length} Selected`;
                }
            };

            // Set initial label
            updatePacksButtonText(state[type].filters.expansion);


            // Sort Listeners
            filterBar.querySelector('.sort-field').addEventListener('change', (e) => {
                state[type].sort.field = e.target.value;
                applyFilters(type, userLang);
            });
            filterBar.querySelector('.sort-dir').addEventListener('change', (e) => {
                state[type].sort.dir = e.target.value;
                applyFilters(type, userLang);
            });

            // Reset Button Listener
            filterBar.querySelector('.btn-reset').addEventListener('click', () => {
                resetFilters(type);
                applyFilters(type, userLang);
            });
        };

        // Loaders
        const loadD2E = () => {
            if (loaded.D2E) return;
            loaded.D2E = true;
            Promise.all([
                loadData(URLS.D2E),
                loadData(URLS.D2E_PACKS)
            ]).then(([scenarios, packs]) => {
                state.D2E.data = scenarios;
                state.D2E.packs = packs;
                renderFilters('D2E', scenarios, packs, userLang);
                applyFilters('D2E', userLang); // Initial render via filter
                renderPacks(packs, 'scenarios-d2e-packs', userLang);
            }).catch(e => console.error(e));
        };

        const loadMOM = () => {
            if (loaded.MOM) return;
            loaded.MOM = true;
            Promise.all([
                loadData(URLS.MOM),
                loadData(URLS.MOM_PACKS)
            ]).then(([scenarios, packs]) => {
                state.MOM.data = scenarios;
                state.MOM.packs = packs;
                renderFilters('MOM', scenarios, packs, userLang);
                applyFilters('MOM', userLang); // Initial render via filter
                renderPacks(packs, 'scenarios-mom-packs', userLang);
            }).catch(e => console.error(e));
        };


        // Activate Tab Helper
        const activateTab = (type) => { // type: 'd2e' or 'mom'
            const tabId = `tab-${type}`;
            const targetPaneId = `scenarios-${type}-tab`;

            // UI Update
            document.querySelectorAll('#scenarioTabs .nav-link').forEach(t => t.classList.remove('active'));
            document.querySelectorAll('.tab-pane').forEach(p => p.classList.remove('show', 'active'));

            const tab = document.getElementById(tabId);
            const pane = document.getElementById(targetPaneId);

            if (tab) tab.classList.add('active');
            if (pane) pane.classList.add('show', 'active');

            // Load Data
            if (type === 'mom') {
                loadMOM();
            } else {
                loadD2E();
            }
        };

        const resetFilters = (type) => {
            // Reset State
            state[type].filters = {
                search: '',
                duration: '',
                difficulty: '',
                language: '',
                expansion: [],
                author: '',
                minRating: '',
                minWinRatio: '',
                avgDuration: '',
                minPlayCount: '',
                minPlayers: '',
                maxPlayers: ''
            };
            // Reset Sort
            state[type].sort = {
                field: 'last_updated',
                dir: 'desc'
            };

            // Update UI Inputs if they exist
            const containerId = `scenarios-${type.toLowerCase()}-list`;
            const listContainer = document.getElementById(containerId);
            if (listContainer && listContainer.parentElement) {
                const filterBar = listContainer.parentElement.querySelector('.filter-bar');
                if (filterBar) {
                    filterBar.querySelector('.filter-search').value = '';
                    filterBar.querySelector('.filter-duration').value = '';
                    filterBar.querySelector('.filter-difficulty').value = '';
                    filterBar.querySelector('.filter-language').value = '';
                    filterBar.querySelector('.filter-author').value = '';
                    filterBar.querySelector('.filter-min-rating').value = '';
                    filterBar.querySelector('.filter-min-win-ratio').value = '';
                    filterBar.querySelector('.filter-avg-duration').value = '';
                    filterBar.querySelector('.filter-avg-duration').value = '';
                    filterBar.querySelector('.filter-avg-duration').value = '';
                    filterBar.querySelector('.filter-min-play-count').value = '';
                    filterBar.querySelector('.filter-min-players').value = '';
                    filterBar.querySelector('.filter-max-players').value = '';
                    filterBar.querySelector('.filter-min-players').value = '';
                    filterBar.querySelector('.filter-max-players').value = '';

                    // Reset checkboxes
                    filterBar.querySelectorAll('.exp-checkbox').forEach(cb => cb.checked = false);
                    const btnSpan = filterBar.querySelector('.exp-dropdown-btn span');
                    const lblAny = getTransitionLabel('Any', userLang);
                    if (btnSpan) btnSpan.textContent = lblAny;

                    // Reset Sort UI
                    filterBar.querySelector('.sort-field').value = 'last_updated';
                    filterBar.querySelector('.sort-dir').value = 'desc';
                }
            }
        };

        // Event Listeners
        document.querySelectorAll('#scenarioTabs .nav-link').forEach(tab => {
            tab.addEventListener('click', (e) => {
                e.preventDefault();
                const type = tab.id.replace('tab-', '');
                const typeUpper = type.toUpperCase();

                resetFilters(typeUpper);
                activateTab(type);

                // Update List and Hash
                applyFilters(typeUpper, userLang);
            });
        });

        // Initial Load
        if (initialParams.type === 'mom') {
            activateTab('mom');
        } else {
            activateTab('d2e');
        }
    }

    init();

});
