---
trigger: always_on
---

- Generate unit tests for each file and each method
   - Exception: UI related classes in `valkyrie\unity\Assets\Scripts\UI`
- Unit Tests should always be located in `valkyrie\unity\Assets\UnitTests`

### Testing
The project uses NUnit for unit testing, integrated into the Unity Test Runner.
**IMPORTANT**: Do NOT try to run these tests using `dotnet test` or creating separate test projects. These tests rely on the Unity Engine and must be run within the Unity environment.

#### Running Tests via Unity Editor (Preferred)
1. Open the **Test Runner** window (`Window > General > Test Runner`).
2. Select **EditMode** tab.
3. Click **Run All** to execute all editor tests.

#### Running Tests via Command Line (Windows / CI Procedure)
The CI pipeline (`.github/workflows/CodeAndSecurityValidation.yml`) uses a PowerShell helper script (`workflowScripts/workflowHelper.ps1`) to run tests. You can emulate this locally using the following command (adjusting paths to your Unity installation):

```powershell
# Example command similar to CI (Run-UnityTests)
& 'C:\Program Files\Unity\Hub\Editor\2019.4.41f1\Editor\Unity.exe' -batchmode -nographics -projectPath ".\unity" -runTests -testPlatform EditMode -testResults "test-results.xml" -logFile "test-results.log"
```

*Note: Ensure the Unity Editor is closed to prevent file lock issues.*

#### Test Structure
- Tests are located in `Assets/UnitTests/Editor`.
- Tests generally verify parsing logic, content loading, and game rules (e.g., `QuestData`, `PuzzleCode`).
- **Library Tests**: Code in the `libraries/` folder is part of the project. Tests for these libraries should be created in `Assets/UnitTests/Editor` and run via Unity, not as standalone projects.
- Use `CultureInfo.InvariantCulture` for all locale-dependent parsing (e.g., `float.TryParse`) to ensure tests pass on all system locales.