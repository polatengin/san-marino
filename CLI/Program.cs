using Semver;
using Spectre.Console;
using System.CommandLine;

var rootCommand = new RootCommand("GitHub Action Version Checker");

rootCommand.SetHandler(async (bool updateVersion, bool updateSha) =>
{
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
      table.AddColumn("Latest");
      table.AddColumn("Comments");

      foreach (var (r, latest) in outdated.DistinctBy(e => e.Ref.Action + e.Ref.Version))
      {
        var _comments = string.Empty;
        if (updateVersion)
        {
          yaml = yaml.Replace($"{r.Action}@{r.Version}", $"{r.Action}@{latest.Version}");
          File.WriteAllText(file, yaml);
          _comments = $"[green]{r.Version} -> {latest.Version}[/]";
        }
        else if (updateSha)
        {
          yaml = yaml.Replace($"{r.Action}@{r.Version}", $"{r.Action}@{latest.CommitSha}");
          File.WriteAllText(file, yaml);
          _comments = $"[green]{r.Version} -> {latest.CommitSha}[/]";
        }

        var _changes = string.Empty;
        if (!r.Version.StartsWith("v"))
        {
          _changes = $"[red]{r.Version}[/] -> [green]{latest.CommitSha}[/]";
        }
        else
        {
          var _currentVersion = SemVersion.Parse(r.Version.TrimStart('v'), SemVersionStyles.Any);

          var isMajor = _currentVersion.Major != latest.Version.Major;
          var isMinor = _currentVersion.Minor != latest.Version.Minor;
          var isPatch = _currentVersion.Patch != latest.Version.Patch;

          var majorColor = isMajor ? "red" : "white";
          var minorColor = isMajor ? majorColor : isMinor ? "yellow" : "white";
          var patchColor = isMajor ? majorColor : isMinor ? minorColor : isPatch ? "blue" : "white";

          _changes = $"[{majorColor}]{latest.Version.Major}.[/][{minorColor}]{latest.Version.Minor}.[/][{patchColor}]{latest.Version.Patch}[/]";
        }

        table.AddRow(new Markup($"[link=https://github.com/{r.Action}]{r.Action}[/]"), new Markup($"v{_changes} (Sha: {latest.CommitSha})"), new Markup(_comments));
      }

      AnsiConsole.Write(table);
    }
    else
    {
      AnsiConsole.MarkupLine($"[blue]{file}[/] [green]No updates found.[/]");
    }
  }
},
  new Option<bool>(["--update-version", "-uv"], "Update the version in place"),
  new Option<bool>(["--update-sha", "-us"], "Update the sha in place")
);
await rootCommand.InvokeAsync(args);
