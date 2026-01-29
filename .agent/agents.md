# AGENTS Guidelines for this repository
You are an expert in C#, Unity, and scalable application development.

This repository contains a Unity engine application located in the root of this repository. When working on the project interactively with an agent (e.g. the Codex CLI) please follow the guidelines below so that the development experience continues to work smoothly.

## Development principles

### Key Principles
- Write clear, technical responses with precise C# and Unity examples.
- Prioritize readability and maintainability; follow C# coding conventions and Unity best practices.
- Use descriptive variable and function names.
- Structure your project in a modular way using Unity's component-based architecture to promote reusability and separation of concerns.

#### 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

#### 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

#### 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

#### 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

### C#/Unity
- Use MonoBehaviour for script components attached to GameObjects; prefer ScriptableObjects for data containers and shared resources.
- Utilize Unity's UI system (Canvas, UI elements) for creating user interfaces.
- Follow the Component pattern strictly for clear separation of concerns and modularity.
- Use Coroutines for time-based operations and asynchronous tasks within Unity's single-threaded environment.

### Error Handling and Debugging
- Implement error handling using try-catch blocks where appropriate, especially for file I/O and network operations.
- Use the projects custom logger class for logging and debugging.
- Utilize Unity's profiler and frame debugger to identify and resolve performance issues.
- Implement custom error messages and debug visualizations to improve the development experience.
- Use Unity's assertion system (Debug.Assert) to catch logical errors during development.

### Dependencies
- Unity Engine
- .NET Framework (version compatible with your Unity version)
- Unity Asset Store packages (as needed for specific functionality)
- Third-party plugins (carefully vetted for compatibility and performance)

### Key Conventions
1. Follow Unity's component-based architecture for modular and reusable game elements.
2. Prioritize performance optimization and memory management in every stage of development.
3. Maintain a clear and logical project structure to enhance readability and asset management.
4. Always use a test driven approach when implementing new features (except for UI related features).

Refer to Unity documentation and C# programming guides for best practices in scripting, application/game architecture, and performance optimization.

## Development environment
Coding is intended to happen in a Windows environment.

- This is mainly due to the build.bat and build.ps1 files which help with building the application for different operating systems.
- Some Unix-style commands (for example, `grep`) may not be available or function as expected.
- When providing scripts or command-line instructions, prefer cross-platform or Windows-compatible solutions where possible.

## Unity version
- Unity version can be found here: unity\ProjectSettings\ProjectVersion.txt
- Before implementing or suggesting any Unity-related changes, always verify compatibility with the current Unity version.