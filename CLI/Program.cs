using Semver;
using Spectre.Console;

if (args.Length == 0)
{
  Console.WriteLine("Usage: smc <path-to-repo> [--update-version | --update-sha]");
  return;
}

var root = args[0];
if (!Directory.Exists(root))
{
  Console.WriteLine($"Directory not found: {root}");
  return;
}

bool updateVersionInPlace = args.Contains("--update-version");
bool updateShaInPlace = args.Contains("--update-sha");

var workflowFiles = Directory.GetFiles(root, "*.yml", SearchOption.AllDirectories)
    .Concat(Directory.GetFiles(root, "*.yaml", SearchOption.AllDirectories))
    .Where(f => f.Contains(".github/workflows") && !f.Contains("node_modules"))
    .ToList();

if (!workflowFiles.Any())
{
  AnsiConsole.MarkupLine("[yellow]No workflow files found.[/]");
  return;
}

var checker = new ActionVersionChecker();
AnsiConsole.MarkupLine($"[green]Scanning {workflowFiles.Count} workflow files in {root} folder[/]");

foreach (var file in workflowFiles)
{
  var yaml = File.ReadAllText(file);
  var refs = WorkflowParser.ExtractUsesFields(yaml);

  var outdated = new List<(GitHubActionReference Ref, LatestVersion Latest)>();

  foreach (var r in refs)
  {
    var latest = await checker.GetLatestVersionAsync(r.Action);
    if (latest != null && r.Version != latest.Version.ToString() && r.Version != latest.CommitSha)
    {
      outdated.Add((r, latest));
    }
  }

  if (outdated.Any())
  {
    AnsiConsole.MarkupLine($"\n[blue]{file}[/]");
    var table = new Table();
    table.AddColumn("Action");
    table.AddColumn("Current");
    table.AddColumn("Latest");
    table.AddColumn("More Secure");

    foreach (var (r, latest) in outdated)
    {
      if (updateVersionInPlace)
      {
        yaml = yaml.Replace($"{r.Action}@{r.Version}", $"{r.Action}@{latest.Version}");
        File.WriteAllText(file, yaml);
        AnsiConsole.MarkupLine($"[green]{r.Action}@{r.Version} updated to {r.Action}@{latest.Version}[/]");
      }
      else if (updateShaInPlace)
      {
        yaml = yaml.Replace($"{r.Action}@{r.Version}", $"{r.Action}@{latest.CommitSha}");
        File.WriteAllText(file, yaml);
        AnsiConsole.MarkupLine($"[green]{r.Action}@{r.Version} updated to {r.Action}@{latest.CommitSha}[/]");
      }

      table.AddRow(r.Action, r.Version, latest.Version, latest.CommitSha);
    }

    AnsiConsole.Write(table);
  }
}
