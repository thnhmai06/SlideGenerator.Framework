# Framework Overview

Vietnamese version: [Vietnamese](../vi/overview.md)

## Table of contents

- [Framework Overview](#framework-overview)
  - [Table of contents](#table-of-contents)
  - [Purpose](#purpose)
  - [Modules](#modules)
  - [Threading and disposal](#threading-and-disposal)

## Purpose

`SlideGenerator.Framework` provides the core processing engine for:

- Workbook parsing and worksheet access
- Template slide loading and cloning
- Text replacement
- Image replacement (ROI + crop + resize)
- Cloud URL resolution

The backend orchestrates jobs and I/O; the framework performs the processing.

## Modules

- `Cloud`: resolve supported cloud URLs to direct download links.
- `Sheet`: workbook and worksheet access for Excel/CSV data.
- `Slide`: template loading, slide cloning, text replacement, image replacement.
- `Image`: ROI detection, cropping, resizing, and helpers.

## Threading and disposal

- `Workbook` and presentation models are `IDisposable`.
- Dispose objects when finished to avoid file locks.
