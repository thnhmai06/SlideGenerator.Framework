# Copilot Instructions (Framework)

## Scope
- This file applies to `SlideGenerator.Framework` only.
- For cross-solution architecture decisions, also read `../construction.md` and root `.github/copilot-instructions.md`.

## Purpose of Framework
- Keep this project as a reusable low-level library for:
  - `Cloud` URI resolution
  - `Sheet` parsing helpers
  - `Slide` OpenXML manipulation/replacement
  - `Image` processing and face detection
- Do not add app orchestration, workflow orchestration, JSON-RPC transport, or endpoint concerns here.

## Public API Discipline
- Preserve stable public APIs unless a breaking change is explicitly requested.
- Add XML documentation for public types and members in touched files.
- Prefer extension-based utility APIs already used in `Sheet` and `Slide` services.

## Face Detection Contract
- Keep model lifecycle explicit and caller-driven:
  - `FaceDetectorModelManager` owns model selection/init/de-init.
  - Detection model implementations must not silently auto-init in `DetectAsync`.
  - Throw `InvalidOperationException` when detect is called without initialized model.
- Return complete detection output; score filtering belongs to caller.

## Concurrency and Resource Safety
- Treat native/OpenCV resources as first-class lifecycle concerns (`IDisposable`/`IAsyncDisposable`).
- Keep thread-safety guarantees explicit for shared services (for example lock/semaphore around detector calls).
- Avoid hidden global mutable state except intentional singleton patterns already present (for example `CloudResolver.Instance`).

## Dependency and Portability Rules
- Keep dependencies framework-level and reusable; avoid leaking application/domain concepts.
- Preserve existing cross-platform OpenCvSharp runtime expectations (configured by host project).
- Do not introduce dependencies that force transport, storage, or workflow runtime choices.

## Code Style
- Use clear C# 12+ style matching current codebase.
- Validate external inputs early.
- Prefer async I/O and async APIs end-to-end.
- Keep methods focused; extract private helpers for multi-step algorithms.

## Validation Before Finalizing
- Build `SlideGenerator.Framework` after changes.
- If API surface changed, verify referenced projects compile (`Application`, `Domain`, `Ipc`) and mention migration notes.

