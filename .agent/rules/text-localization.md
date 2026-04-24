---
trigger: model_decision
description: When adding ANY text that is visible in the User interface (ALWAYS check for any changes in folders unity\Assets\Scripts\QuestEditor or unity\Assets\Scripts\UI)
---

### Localization files
UI text MUST ALWAYS be localized. Do NOT use hardcoded strings for UI elements. Localization files are located in `Assets/StreamingAssets/text/`.
- `Localization.English.txt` is the master file.
- The format is `KEY,Value`.
- When adding new text:
1. Add the `KEY,English Value` to `Localization.English.txt`.
2. **CRITICAL**: Add a translated version `KEY,Translated Value` to *all* other relevant files (`Localization.German.txt`, `Localization.French.txt`, `Localization.Spanish.txt`, `Localization.Italian.txt`, `Localization.Chinese.txt`, `Localization.Czech.txt`, `Localization.Japanese.txt`, `Localization.Korean.txt`, `Localization.Polish.txt`, `Localization.Portuguese.txt`, `Localization.Russian.txt`, `Localization.Ukrainian.txt`) where the value is translated to the language specified in the filename **IMMEDIATELY**. You must not just copy the English text; you must provide a translation. Do not defer this task. Failing to do so will result in missing text for users of those languages.
3. In C# code, use `new StringKey("val", "KEY")` to reference the text.
4. For keys used inside a screen class (e.g. `QuestSelectionScreen`), declare a `private readonly StringKey` field at the top of the class alongside the other StringKey fields — do NOT inline `new StringKey(...)` at the call site. Example:
   ```csharp
   private readonly StringKey MY_KEY = new StringKey("val", "MY_KEY");
   ```
   Then use `ui.SetText(MY_KEY)` or `MY_KEY.Translate()` at the call site.
5. For commonly used keys, add a static reference in `Assets/Scripts/Content/CommonStringKeys.cs`.
6. **VERIFICATION**: Before finishing the task, use `find_by_name` or `list_dir` to list all `Localization.*.txt` files. Confirm that the new key has been added and translated to the respective file language to EACH file. Do not assume; verify.

### StringKey with parameters
When a translated string contains `{0}` placeholders (e.g. `Could not open "{0}".`):
- If constructing from a dict/key string: `new StringKey("val", "MY_KEY", someParam)` — sets parameters to `{0}:someParam`.
- If using a class-level `StringKey` field as template: `new StringKey(MY_KEY_FIELD, "{0}", someParam)` — uses the two-StringKey constructor which sets `parameters = "{0}:" + someParam`.
- For multi-part text that needs a `\n` between two separate keys, concatenate via `.Translate()`:
  ```csharp
  new StringKey(MY_KEY_FIELD, "{0}", param).Translate() + "\n" + OTHER_KEY.Translate()
  ```

### Line endings in localization files — CRITICAL
All `Localization.*.txt` files use **Windows CRLF (`\r\n`) line endings**. The parser (`DictionaryI18n.AddDataFromFile`) splits on `\r` only. If any inserted lines use `\n`-only endings, those lines are not split and end up merged into the previous key's value — the new keys are silently ignored and the previous key gets garbage in its value.

**Rules:**
- Never use PowerShell here-strings or string concatenation with bare `\n` to insert content into localization files. The Edit tool (which preserves the file's existing line endings) is safe to use.
- If using PowerShell to modify localization files, verify line endings with a hex check afterward, and replace lone `\n` before keys with `\r\n`.
- Each non-English file has its own **localized** value for `QUEST_NAME_UPDATE` (e.g. `[Aktualisierung]` in German, `[Mise à jour]` in French). Do not assume the English value is shared — always grep for the actual key in each file before using it as an anchor.