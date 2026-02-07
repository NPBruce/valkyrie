---
trigger: model_decision
description: When implementing/changing UI components (app only not Valkyrie website)
---

# UI Guide

This document outlines the UI system used in the Valkyrie application. The UI is primarily code-driven using a custom `UIElement` usage wrapper around Unity's native UI system.

## Core UI Classes

The UI system is built upon a few key classes located in `Assets/Scripts/UI`.

### UIElement
`UIElement` is the fundamental building block for all UI components. It wraps Unity's `Image`, `Text`, and `Button` components and handles positioning and styling.

- **Usage**: Used for buttons, labels, and background images.
- **Positioning**: Uses a custom "UIScaler unit" system to ensure resolution independence.
- **Key Methods**:
    -   `new UIElement(parent_transform)`: Creates a new element.
    -   `SetLocation(x, y, width, height)`: Sets position and size in UIScaler units.
    -   `SetText(string_key)`: Sets localized text.
    -   `SetButton(action)`: Assigns a click action.
    -   `SetImage(texture)`: Sets the background image.
    -   `SetBGColor(color)`: Sets background color.

### UIElementEditable
Inherits from `UIElement`. Provides a text input field.
- **Usage**: User input forms.
- **Key Methods**:
    -   `SetText(text)`: Initial value.
    -   `GetText()`: Retrieve current value.
    -   `SetSingleLine()`: Restricts input to a single line.

### UIElementScrollVertical
Inherits from `UIElement`. Creates a vertically scrollable area.
- **Usage**: Lists, long descriptions.
- **Key Methods**:
    -   `GetScrollTransform()`: Returns the transform where child elements should be attached.
    -   `SetScrollSize(size)`: Sets the total height of the scrollable content.

### UIWindowSelectionList
A helper class to create a popup selection window.
- **Usage**: Dropdown replacements, file pickers.
- **Key Methods**:
    -   `AddItem(text, action)`: Adds a selectable item.
    -   `Draw()`: Renders the popup.

## Screen Structure

Screens are typically `MonoBehaviour` classes that manage the lifecycle of a specific view (e.g., `MainMenuScreen`, `QuestSelectionScreen`).

- **Lifecycle**:
    -   `Start()`: Initialization.
    -   `Show()`: Builds the UI elements.
    -   `Clean()`: Destroys UI elements (often by tag).
- **Tagging**: Elements are often tagged (e.g., `Game.QUESTUI`) to facilitate bulk destruction when switching screens.

## Quest Editor UI

The Quest Editor uses a component-based approach for its UI, defined in `Assets/Scripts/QuestEditor`.

### EditorComponent
The base class for all editor components (Quest, Monster, Token, etc.).
- **Responsibility**: Manages the UI for editing properties of a `QuestComponent`.
- **Update Cycle**:
    -   `Update()`: Re-renders the entire component UI.
    -   `DrawComponentSelection()`: Standard buttons (Rename, Delete).
    -   `AddSubComponents()`: Override this to add component-specific fields.

### Example: Adding a Field to an Editor Component
To add a new editable field to an `EditorComponent`:
1.  Define a `UIElementEditable` field in the class.
2.  In `AddSubComponents()`, instantiate the element and set its location.
3.  Bind a button action (usually on the element itself or a separate button) to a method that updates the underlying data model.
4.  Implement the update method (e.g., `UpdateQuestName()`) which reads the value from the UI element, updates the data, and calls `Update()` to refresh.

## Styling & resources
- **Fonts**: Accessed via `Game.Get().gameType.GetFont()`.
- **Colors**: Standard Unity `Color` structs. `Color.clear` is often used for invisible click targets.
- **UIScaler**: Provides helper methods for responsive sizing (`GetPixelsPerUnit()`, `GetSmallFont()`, `GetLargeFont()`).