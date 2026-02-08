---
description: Workflow to increase the quest format version
---

1. Open `unity/Assets/Scripts/Content/FormatVersions.cs`.
2. Locate the `Versions` enum at the bottom of the class.
3. Identify the last entry in the `Versions` enum (e.g., `RELEASE_3_1_5 = 20`).
4. Read the current version of the application from `version.txt` to determine the new version string (e.g., `3.1.6`).
5. Add a new entry to the `Versions` enum:
    - Name: `RELEASE_<MAJOR>_<MINOR>_<PATCH>` (e.g., `RELEASE_3_1_6`)
    - Value: Increment the last value by 1 (e.g., `21`)
6. Locate the `CURRENT_VERSION` constant at the top of the class.
7. Update the assignment to use the new enum value:
    ```csharp
    public const int CURRENT_VERSION = (int) Versions.RELEASE_<MAJOR>_<MINOR>_<PATCH>;
    ```
8. Save the file.
