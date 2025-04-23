# CLI and VS Code Extension with Language Server Protocol (LSP)

San Marino is a command-line interface (CLI) and Visual Studio Code (VS Code) extension that provides a language server protocol (LSP) for GitHub Action yaml files. It is designed to enhance the development experience by providing features such as; checking if the steps are at latest version, auto-updating the steps to the latest version, and provide updating steps with safer approaches. The CLI can be used independently, while the VS Code extension integrates the CLI into the VS Code editor for a seamless development experience.

## Features

- **Check for Latest Version**: The CLI and VS Code extension can check if the steps in your GitHub Action yaml files are at the latest version. This helps ensure that you are using the most up-to-date and secure versions of the actions.

- **Auto-Update Steps**: The CLI and VS Code extension can automatically update the steps in your GitHub Action yaml files to the latest version. This saves you time and effort in manually checking and updating each step.

- **Safer Update Approaches**: The CLI and VS Code extension provide safer approaches (using Commit Sha) for updating steps in your GitHub Action yaml files. This helps prevent breaking changes and ensures that your workflows continue to function as expected.

- **VS Code Integration**: The VS Code extension integrates the CLI into the VS Code editor, providing a seamless development experience. You can use the features of the CLI directly within the editor, making it easy to check and update your GitHub Action yaml files without leaving the editor.

- **Language Server Protocol (LSP)**: The CLI and VS Code extension use the language server protocol (LSP) to provide features such as code completion, linting, error checking, and schema validation for GitHub Action yaml files. This helps improve the development experience and ensures that your workflows are valid and error-free.

## Installation

To install the CLI, you can use the following command:

```bash
npm install -g san-marino-cli
```

To install the VS Code extension, you can search for "San Marino" in the VS Code marketplace or install it directly from the command line:

```bash
code --install-extension san-marino
```

