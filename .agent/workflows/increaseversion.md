---
description: Increase the app version of Valkyrie
---

Increase version numbers in files:
- unity\Assets\Resources\version.txt
- unity\Assets\Resources\prod_version.txt

1. Ensure to keep numbers for both files in sync.
2. ALWAYS ask the user if this release is a "BETA", "MAJOR", or a normal release if it is not explicitly specified.
3. If specified "major" then increase major version number e.g. from 3.14 to 4.14. Otherwise if not specified always increase minor version.
4. Add a second line to both version files with the build label: `BETA` for beta releases, `MAJOR` for major releases (e.g. line 1: `3.14`, line 2: `BETA`). Do not add a second line for normal releases unless requested.
