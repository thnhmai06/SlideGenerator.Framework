# Contributing to SlideGenerator.Framework

This library is the core engine of the SlideGenerator project. Contributions here have a significant impact on the performance and reliability of the entire application.

## Development Workflow

### Prerequisites

- **.NET 10.0 SDK** (or newer)
- **Visual Studio** or **VS Code** with C# extension.

### Getting Started

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/thnhmai06/SlideGenerator.Framework
    cd SlideGenerator.Framework
    ```

2.  **Restore dependencies:**
    ```bash
    dotnet restore
    ```

3.  **Run Tests:**
    Ensure all unit tests pass before making changes.
    ```bash
    dotnet test
    ```

## Project Structure

- `src/SlideGenerator.Framework`: The main library code.
  - `Cloud/`: URL resolution logic.
  - `Sheet/`: Excel/CSV parsing logic.
  - `Slide/`: OpenXML SDK wrappers for PPTX manipulation.
  - `Image/`: EmguCV integration for image processing.
- `src/SlideGenerator.Framework.Tests`: Unit and integration tests.

## Coding Guidelines

- **Performance First:** This is a low-level library. Avoid unnecessary allocations (use `Span<T>`, `Memory<T>`).
- **Async/Await:** Use async APIs for all I/O bound operations (File, Network).
- **Clean Architecture:** Keep dependencies minimal. Avoid tight coupling between modules (e.g., `Sheet` should not know about `Image`).

## Pull Request Process

1.  Create a new branch: `git checkout -b feature/my-awesome-feature`
2.  Implement your changes.
3.  Add unit tests to cover your new code.
4.  Run `dotnet format` to ensure code style consistency.
5.  Push and submit a Pull Request.

## Reporting Issues

If you find a bug in the framework, please open an issue in the [main repository](https://github.com/thnhmai06/SlideGenerator/issues) and tag it with `component:framework`.
