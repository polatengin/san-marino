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
    outputChannelName: 'YourProject LSP',
  };

  client = new LanguageClient('yourProjectLsp', 'YourProject Language Server', serverOptions, clientOptions);
  client.start();
}

export function deactivate(): Thenable<void> | undefined {
  return client ? client.stop() : undefined;
}
