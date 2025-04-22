public class GitHubActionReference
{
  public string Action { get; set; } = string.Empty;
  public string Version { get; set; } = string.Empty;
  public long Line { get; set; }
  public long Column { get; set; }

  public override string ToString() => $"{Action}@{Version} (line {Line}, col {Column})";
}
