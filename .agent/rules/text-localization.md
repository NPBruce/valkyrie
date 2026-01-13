---
trigger: model_decision
description: When it is necessary to add UI related texts
---

### Localization files
UI text should always get localized. Localization files are located in `Assets/StreamingAssets/text/`.
- `Localization.English.txt` is the master file.
- The format is `KEY,Value`.
- When adding new text:
1. Add the `KEY,English Value` to `Localization.English.txt`.
2. **CRITICAL**: Add a translated version `KEY,Translated Value` to *all* other relevant files (`Localization.German.txt`, `Localization.French.txt`, `Localization.Spanish.txt`, `Localization.Italian.txt`, etc.) where the value is translated to the language specified in the filename **IMMEDIATELY**. Do not defer this task. Failing to do so will result in missing text for users of those languages.
3. In C# code, use `new StringKey("val", "KEY")` to reference the text.
4. For commonly used keys, add a static reference in `Assets/Scripts/Content/CommonStringKeys.cs`.
5. **VERIFICATION**: Before finishing the task, use find_by_name or list_dir to list all Localization.*.txt files. Confirm that the new key has been added and translated to the respective file language to EACH file. Do not assume; verify.