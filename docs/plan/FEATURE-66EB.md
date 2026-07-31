# FEATURE-66EB — Accessibility baseline

**Status:** TODO — **DEFERRED. Do not build yet.**
**Branch (when built):** `feature/feature-66eb-accessibility-baseline`

## 1. Why this row exists

The controls in `src/Enigma.Avalonia.Desktop/` were delivered as a faithful 1:1 port
(`FEATURE-22A5`), and the port source has **no `AutomationPeer` implementations and no documented
keyboard model**. That gap therefore ships in 1.0.0 by decision, and this row keeps it visible
instead of losing it.

**Do not pick this up while `FEATURE-22A5` is unbuilt or unpublished.** Adding automation peers and
keyboard handling mid-port would break the 1:1 contract that every one of that item's phases is
verified against.

## 2. Objective (when un-deferred)

Give every interactive control a defined, tested accessibility surface — screen-reader visibility and
keyboard operability without a pointer.

## 3. Scope sketch

- **Automation peers:** `NavigationView` / `NavigationItem` (selection pattern),
  `Ribbon` / `RibbonTab` / `RibbonButton` / `RibbonToggleButton` / `RibbonDropDownButton`,
  `ContentDialog` (dialog control type, correct name from its title), `InfoBar` (live-region
  semantics per `InfoBarSeverity`), `DockPane` / `DockTabGroup`, the editors (edit control type,
  validation state exposed), `SettingsCard` / `SettingsCardExpander` (expand/collapse pattern).
- **Keyboard model:** arrow keys within a `NavigationView` rail and across `RibbonTab`s; `Escape`
  dismissing `ContentDialog` and `Overlay`; `Enter`/`Space` activating ribbon buttons; a sane `Tab`
  order through docked panes; expander toggle by keyboard; focus visuals that survive both theme
  variants.
- **Focus management:** `ContentDialog` traps focus while open and restores it to the previously
  focused element on close; `Overlay` blocks focus reaching the content beneath it.
- **Decorative vs meaningful:** icons in the showcase get `AutomationProperties.Name` where they carry
  meaning of their own (`Enigma.Icons.Avalonia`'s `Icon` is decorative and skipped by screen readers
  by default, which is the right default).
- **Tests:** extend `tests/Enigma.Avalonia.Desktop.UnitTests` with headless assertions per control —
  `OnCreateAutomationPeer` returns the expected peer and control type, and each documented key
  produces the documented effect.
- **Docs:** an accessibility section per affected guide, plus a summary in the README.

## 4. Release impact

This is a **behaviour-additive** change to a published API surface: new peers and new key handling,
no signature changes. It fits a `1.1.0` minor release. If any change would alter existing behaviour
(e.g. a key that currently does nothing starting to consume the event in a way a consumer relied on),
record it in `RELEASENOTES.md` under *Breaking Changes & Migration* and reconsider the version.

## 5. Acceptance criteria (to be firmed up when un-deferred)

- Every interactive control exposes an `AutomationPeer` with the correct `AutomationControlType`.
- Every control family is fully operable by keyboard alone, and that is asserted by tests.
- `ContentDialog` focus trap and restore are tested.
- Build clean at zero warnings; full suite green.
- The affected guides document the keyboard model.
