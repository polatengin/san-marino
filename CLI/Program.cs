using Semver;
using Spectre.Console;
using System.CommandLine;

var pathOption = new Option<DirectoryInfo>(["--path", "-p"], description: "Directory to scan for workflow files", getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory()));
var updateVersionOption = new Option<bool>(["--update-version", "-uv"], "Update the version in place");
var updateShaOption = new Option<bool>(["--update-sha", "-us"], "Update the sha in place");

var rootCommand = new RootCommand("Check and update GitHub Action versions in workflow files")
{
  pathOption,
  updateVersionOption,
  updateShaOption
};
rootCommand.Name = "gacu";

rootCommand.SetHandler(async (DirectoryInfo path, bool updateVersion, bool updateSha) =>
{
  AnsiConsole.MarkupLine($"[yellow]Scanning [green]{path.FullName}[/] for workflow files[/]");

  var workflowFiles = Directory.GetFiles(path.FullName, "*.yml", SearchOption.AllDirectories)
      .Concat(Directory.GetFiles(path.FullName, "*.yaml", SearchOption.AllDirectories))
      .Where(f => f.Contains(".github/workflows") && !f.Contains("node_modules"))
      .ToList();

  if (!workflowFiles.Any())
  {
    AnsiConsole.MarkupLine("[yellow]No workflow files found.[/]");
    return;
  }

  var checker = new ActionVersionChecker();
  AnsiConsole.MarkupLine($"[green]{workflowFiles.Count} workflow files found[/]");

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
      table.AddColumn("Status");
      table.AddColumn("Comments");

      foreach (var (r, latest) in outdated.DistinctBy(e => e.Ref.Action + e.Ref.Version))
      {
        var _action = $"[link=https://github.com/{r.Action}]{r.Action}[/]";
        var _status = string.Empty;
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

        if (!r.Version.StartsWith("v"))
        {
          _status = $"[red]{r.Version}[/] -> [green]{latest.CommitSha}[/]";
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

          _status = $"v[{majorColor}]{latest.Version.Major}.[/][{minorColor}]{latest.Version.Minor}.[/][{patchColor}]{latest.Version.Patch}[/] (Sha: {latest.CommitSha})";
        }

        table.AddRow(new Markup(_action), new Markup(_status), new Markup(_comments));
      }

      AnsiConsole.Write(table);
    }
    else
    {
      AnsiConsole.MarkupLine($"[blue]{file}[/] [green]No updates found.[/]");
    }
  }
}, pathOption, updateVersionOption, updateShaOption);

await rootCommand.InvokeAsync(args);
