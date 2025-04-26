import { LanguageClient, LanguageClientOptions, ServerOptions } from 'vscode-languageclient/node';
import * as vscode from 'vscode';

let client: LanguageClient;

export function activate(context: vscode.ExtensionContext) {
  const serverExe = context.asAbsolutePath('./dist/LanguageServer/LanguageServer');

  const serverOptions: ServerOptions = {
    run: { command: serverExe },
    debug: { command: serverExe }
  };

  const clientOptions: LanguageClientOptions = {
    documentSelector: [{ scheme: 'file', language: 'yaml' }],
    outputChannelName: 'San Marino',
  };

  client = new LanguageClient('san-marino-lsp', 'San Marino Language Server', serverOptions, clientOptions);
  client.start();

  context.subscriptions.push(
    vscode.languages.registerCodeLensProvider(
      { scheme: 'file', language: 'yaml' },
      {
        async provideCodeLenses(document, token) {
          try {
            const buildOutput: string = await client.sendRequest("workspace/executeCommand", {
              command: "build",
              arguments: [document.uri.toString()],
            });
            client.outputChannel.appendLine(buildOutput);
          } catch (err) {
            client.outputChannel.appendLine("Bicep build failed: " + JSON.stringify(err));
            client.error("Bicep build failed", JSON.stringify(err), true);
          }

          const codeLensList: vscode.CodeLens[] = [];

          const range = new vscode.Range(
            new vscode.Position(0, 0),
            new vscode.Position(1, 0)
          );
          codeLensList.push(new vscode.CodeLens(range, {
            title: 'San Marino LSP',
            command: 'san-marino-lsp.command',
            arguments: [document.uri]
          }));
          codeLensList.push(new vscode.CodeLens(range, {
            title: 'Secure',
            command: 'san-marino-secure.command',
            arguments: [document.uri]
          }));

          return codeLensList;
        }
      }
    )
  );
}

export async function deactivate(): Promise<void> {
  await client?.stop();
}
