---
trigger: model_decision
description: When adding ANY text that is visible in the UI
---

### Localization files
UI text MUST ALWAYS be localized. Do NOT use hardcoded strings for UI elements. Localization files are located in `Assets/StreamingAssets/text/`.
- `Localization.English.txt` is the master file.
- The format is `KEY,Value`.
- When adding new text:
1. Add the `KEY,English Value` to `Localization.English.txt`.
2. **CRITICAL**: Add a translated version `KEY,Translated Value` to *all* other relevant files (`Localization.German.txt`, `Localization.French.txt`, `Localization.Spanish.txt`, `Localization.Italian.txt`, `Localization.Chinese.txt`, `Localization.Czech.txt`, `Localization.Japanese.txt`, `Localization.Korean.txt`, `Localization.Polish.txt`, `Localization.Portuguese.txt`, `Localization.Russian.txt`) where the value is translated to the language specified in the filename **IMMEDIATELY**. You must not just copy the English text; you must provide a translation. Do not defer this task. Failing to do so will result in missing text for users of those languages.
3. In C# code, use `new StringKey("val", "KEY")` to reference the text.
4. For commonly used keys, add a static reference in `Assets/Scripts/Content/CommonStringKeys.cs`.
5. **VERIFICATION**: Before finishing the task, use `find_by_name` or `list_dir` to list all `Localization.*.txt` files. Confirm that the new key has been added and translated to the respective file language to EACH file. Do not assume; verify.