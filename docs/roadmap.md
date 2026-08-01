# Enigma.Avalonia — Roadmap

Single persistent registry of every tracked work item in this solution. Summary only — full details
live in the linked plan files (`docs/plan/<ID>.md`); completion records in `docs/done/<ID>.md`.

Status vocabulary: `TODO`, `IN PROGRESS`, `DONE`, `ABANDONED`.
Row order is the intended build order — `/build` surfaces the topmost `TODO` first.

| ID           | Title                                                                   | Status      | Plan                      |
|--------------|-------------------------------------------------------------------------|-------------|---------------------------|
| FEATURE-28E8 | Repository & solution scaffolding (root config, .slnx, project skeletons) | DONE        | docs/plan/FEATURE-28E8.md |
| - PHASE01    | Root configuration & solution file                                       | DONE        | (in FEATURE-28E8.md)      |
| - PHASE02    | Three buildable project skeletons + smoke test                           | DONE        | (in FEATURE-28E8.md)      |
| FEATURE-22A5 | Control library — Enigma.Avalonia.Desktop                                | IN PROGRESS | docs/plan/FEATURE-22A5.md |
| - PHASE01    | Theme foundation & Enigma* resource keys                                 | DONE        | (in FEATURE-22A5.md)      |
| - PHASE02    | CollectionView data subsystem                                            | DONE        | (in FEATURE-22A5.md)      |
| - PHASE03    | Editors (16 controls, Enigma.Core encoding)                              | DONE        | (in FEATURE-22A5.md)      |
| - PHASE04    | Navigation controls & navigation service                                 | TODO        | (in FEATURE-22A5.md)      |
| - PHASE05    | ContentDialog, Overlay, InfoBar + their services                         | TODO        | (in FEATURE-22A5.md)      |
| - PHASE06    | Settings cards + file/folder dialog services                             | TODO        | (in FEATURE-22A5.md)      |
| - PHASE07    | Ribbon                                                                   | TODO        | (in FEATURE-22A5.md)      |
| - PHASE08    | Docking                                                                  | TODO        | (in FEATURE-22A5.md)      |
| FEATURE-6EB0 | Test suite (headless + unit)                                             | TODO        | docs/plan/FEATURE-6EB0.md |
| - PHASE01    | CollectionView & service-contract unit tests                             | TODO        | (in FEATURE-6EB0.md)      |
| - PHASE02    | Headless control smoke & resource-key tests                              | TODO        | (in FEATURE-6EB0.md)      |
| FEATURE-57C8 | Showcase app — Enigma.Avalonia.Desktop.Showcase                          | TODO        | docs/plan/FEATURE-57C8.md |
| - PHASE01    | App shell & host wiring                                                  | TODO        | (in FEATURE-57C8.md)      |
| - PHASE02    | Pages: Base controls, Editors, Dialogs, Services                         | TODO        | (in FEATURE-57C8.md)      |
| - PHASE03    | Pages: Ribbon, Docking, Navigation, CollectionView, Charts, Settings     | TODO        | (in FEATURE-57C8.md)      |
| FEATURE-2802 | Documentation (guides, packed README, SECURITY.md, CLAUDE.md)            | TODO        | docs/plan/FEATURE-2802.md |
| - PHASE01    | Per-family guides + index                                                | TODO        | (in FEATURE-2802.md)      |
| - PHASE02    | Packed README, SECURITY.md, CLAUDE.md                                    | TODO        | (in FEATURE-2802.md)      |
| FEATURE-1702 | Release preparation & NuGet publish runbook (1.0.0)                      | TODO        | docs/plan/FEATURE-1702.md |
| - PHASE01    | Package metadata & third-party license audit                             | TODO        | (in FEATURE-1702.md)      |
| - PHASE02    | RELEASENOTES, PackageReleaseNotes, README what's-new callout             | TODO        | (in FEATURE-1702.md)      |
| - PHASE03    | docs/RELEASE.md, pre-flight, pack-verify, printed runbook                | TODO        | (in FEATURE-1702.md)      |
| FEATURE-16A9 | `enigma-avalonia-desktop` house skill (target is outside this repo)      | TODO        | docs/plan/FEATURE-16A9.md |
| FEATURE-66EB | Accessibility baseline — **DEFERRED, do not build yet**                  | TODO        | docs/plan/FEATURE-66EB.md |

`FEATURE-16A9` and `FEATURE-66EB` are single-phase items — no phase rows. Everything above them is
multi-phase: one branch, one commit and one `docs/done/<ID>-PHASENN.md` per phase.

`FEATURE-66EB` is the one row `/build` must **skip** until the 1.0.0 line is published: it is
deliberately deferred, and picking it up early would break the 1:1 port contract that
`FEATURE-22A5` depends on.
