# Framework Overview

Vietnamese version: [Vietnamese](../vi/overview.md)

## Table of contents

1. [Purpose](#purpose)
2. [Modules](#modules)
3. [Key constraints](#key-constraints)
4. [Threading and disposal](#threading-and-disposal)

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
