---
trigger: model_decision
description: Guide for working with the Valkyrie website (web folder and root HTML files)
---

# Valkyrie Website Guide

This guide outlines the technical structure and development guidelines for the Valkyrie project website.

## Overview
The website is a static site built with HTML, CSS (Bootstrap 4), and Vanilla JavaScript.
**Crucially, it does NOT use any external JavaScript libraries (like jQuery, React, Vue).** All logic is written in plain ES6+ JavaScript.

## File Structure
- **Root Directory**: Contains the main HTML files.
    - `index.html`: Landing page with features and download links.
    - `scenarios.html`: The scenario browser application.
    - *Localized versions*: `index-de.html`, `scenarios-es.html`, etc. exist for supported languages.
- **web/css/**: Stylesheets.
    - `bootstrap.css`: Standard Bootstrap 4 framework.
    - `styles.css`: Custom styles for the landing page.
    - `scenarios.css`: Specific styles for the scenario browser.
- **web/js/**: JavaScript logic.
    - `vanilla-scripts.js`: Logic for `index.html` (scroll, tabs, mobile menu). Replaces previous jQuery scripts.
    - `scenarios.js`: Core logic for `scenarios.html`. Fetches manifest data, parses INI-like content, and renders the list.
    - `translations.js`: Contains the `TRANSLATIONS` object for localizing dynamic UI elements on the scenarios page.
    - `dialog.js`: Simple modal/dialog implementation for showing scenario details.

## Pages

### 1. Index Page (`index.html`)
- **Purpose**: Landing page, feature showcase, and download portal.
- **Tech**: HTML5, Bootstrap Grid.
- **Logic**: `vanilla-scripts.js` handles:
    - Sticky navbar transition.
    - Smooth scrolling for anchor links.
    - Mobile menu toggle.
    - Tab switching (Create/Play sections).

### 2. Scenarios Page (`scenarios.html`)
- **Purpose**: Browser for community-created content.
- **Logic**: `scenarios.js` is a single-page app (SPA) like script that:
    - Fetches the data (e.g., from GitHub raw content).
    - Parses the custom INI/manifest format.
    - Renders the list of scenarios and content packs.
    - Implements filtering (search, duration, difficulty, language, etc.).
    - Implements localization using `translations.js`.
- **Localization**:
    - The HTML shell (navbar, footer) is localized via separate HTML files (`scenarios-de.html`).
    - The dynamic content (filters, labels) is localized via `translations.js`.

## Development Guidelines

### General
- **No Frameworks**: Do not introduce frameworks or heavy libraries. Use standard DOM APIs (`querySelector`, `addEventListener`, `fetch`).
- **Bootstrap**: Use Bootstrap utility classes (`d-flex`, `text-center`, `mb-3`) for layout and spacing.
- **Responsiveness**: Ensure all changes work on mobile (Bootstrap `col-md-`, `col-lg-` classes).

### Scenarios Page (`scenarios.js`)
- **Rendering**: The list is re-rendered when filters change. Avoid heavy DOM operations in the render loop if possible.
- **Localization**:
    - When adding new UI text, ALWAYS add it to `TRANSLATIONS` in `translations.js`.
    - Use `getTransitionLabel('Key', lang)` to retrieve text.
- **Data Parsing**: The data source is often an INI-style plain text format. Parsing logic matches this specific format.

### CSS
- **Modifications**:
    - For general styling, use `web/css/styles.css`.
    - For scenario-list specific styling, use `web/css/scenarios.css`.
- **Icons**: Use FontAwesome (`fas fa-search`, etc.).

### HTML Updates
- **Syncing**: If you make structural changes to `index.html` or `scenarios.html`, verify if the localized versions (e.g., `index-de.html`) need similar updates. (Check if there is an automated process or if you need to apply edits to all files).