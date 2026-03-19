import * as vscode from 'vscode';

/** Map of command IDs to their corresponding `rvr` CLI invocations. */
const COMMANDS: Record<string, { cli: string; prompt?: string }> = {
  'rivora.newSolution':  { cli: 'rvr new',           prompt: 'Enter solution name' },
  'rivora.addModule':    { cli: 'rvr module add',    prompt: 'Enter module name' },
  'rivora.removeModule': { cli: 'rvr module remove', prompt: 'Enter module name to remove' },
  'rivora.aiReview':     { cli: 'rvr ai review' },
  'rivora.generateTest': { cli: 'rvr ai test' },
  'rivora.doctor':       { cli: 'rvr doctor' },
  'rivora.migrate':      { cli: 'rvr db migrate' },
  'rivora.seed':         { cli: 'rvr db seed' },
  'rivora.publish':      { cli: 'rvr publish' },
  'rivora.upgrade':      { cli: 'rvr upgrade' },
  'rivora.envList':      { cli: 'rvr env list' },
};

/**
 * Returns an existing terminal named "RIVORA" or creates a new one.
 */
function getRivoraTerminal(): vscode.Terminal {
  const existing = vscode.window.terminals.find(t => t.name === 'RIVORA');
  if (existing) {
    return existing;
  }
  return vscode.window.createTerminal('RIVORA');
}

/**
 * Resolves the current workspace folder path, or asks the user to pick one
 * when multiple workspace folders are open.
 */
async function getWorkspacePath(): Promise<string | undefined> {
  const folders = vscode.workspace.workspaceFolders;
  if (!folders || folders.length === 0) {
    vscode.window.showWarningMessage('RIVORA: No workspace folder is open.');
    return undefined;
  }
  if (folders.length === 1) {
    return folders[0].uri.fsPath;
  }
  const picked = await vscode.window.showWorkspaceFolderPick({
    placeHolder: 'Select the workspace folder for the RIVORA command',
  });
  return picked?.uri.fsPath;
}

/**
 * Executes a `rvr` CLI command in the integrated terminal.
 */
async function runCliCommand(commandId: string): Promise<void> {
  const spec = COMMANDS[commandId];
  if (!spec) {
    vscode.window.showErrorMessage(`RIVORA: Unknown command "${commandId}".`);
    return;
  }

  let cliArgs = spec.cli;

  // If the command accepts a user-provided argument, prompt for it.
  if (spec.prompt) {
    const input = await vscode.window.showInputBox({
      prompt: spec.prompt,
      placeHolder: spec.prompt,
      ignoreFocusOut: true,
    });
    if (input === undefined) {
      return; // user cancelled
    }
    if (input.trim().length > 0) {
      cliArgs += ` ${input.trim()}`;
    }
  }

  // For AI review, append the workspace path so the CLI knows what to scan.
  if (commandId === 'rivora.aiReview') {
    const wsPath = await getWorkspacePath();
    if (wsPath) {
      cliArgs += ` --path "${wsPath}"`;
    }
  }

  const terminal = getRivoraTerminal();
  terminal.show(true);
  terminal.sendText(cliArgs);
}

// ── Activation ──────────────────────────────────────────────────────────────

export function activate(context: vscode.ExtensionContext): void {
  // Register every command from the map.
  for (const commandId of Object.keys(COMMANDS)) {
    const disposable = vscode.commands.registerCommand(commandId, () =>
      runCliCommand(commandId),
    );
    context.subscriptions.push(disposable);
  }

  vscode.window.showInformationMessage('RIVORA Framework extension activated.');
}

export function deactivate(): void {
  // nothing to clean up
}
