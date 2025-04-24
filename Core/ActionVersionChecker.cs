using Octokit;
using Semver;

public record LatestVersion(SemVersion Version, string CommitSha);

public class ActionVersionChecker
{
  private readonly GitHubClient _client;
  private readonly Dictionary<string, LatestVersion> _cache = new();

  public ActionVersionChecker(string userAgent = "SanMarino-Core")
  {
    _client = new GitHubClient(new ProductHeaderValue(userAgent));
    if (Environment.GetEnvironmentVariable("GITHUB_TOKEN") is string token)
    {
      _client.Credentials = new Credentials(token, AuthenticationType.Bearer);
    }
  }

  public async Task<LatestVersion?> GetLatestVersionAsync(string action)
  {
    if (_cache.TryGetValue(action, out var cached))
      return cached;

    var parts = action.Split('/');
    if (parts.Length != 2)
      return null;

    var (owner, repo) = (parts[0], parts[1]);

    try
    {
      var tags = await _client.Repository.GetAllTags(owner, repo);
      var latest = tags
          .Where(e => e.Name != null && e.Commit != null && e.Commit.Sha != null && e.Name.StartsWith("v"))
          .Select(e => new { Version = SemVersion.TryParse(e.Name.TrimStart('v'), SemVersionStyles.Strict, out var ver) ? ver : null, CommitSha = e.Commit.Sha })
          .Where(e => e != null && e.Version != null && e.CommitSha != null)
          .OrderByDescending(e => e.Version, new SemVersionNullableComparer())
          .FirstOrDefault();
      _cache[action] = new LatestVersion(latest.Version, latest?.CommitSha ?? string.Empty);
      return _cache[action];
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error fetching latest version for {action}: {ex.Message}");
      return null;
    }
  }
}

public class SemVersionNullableComparer : IComparer<SemVersion?>
{
  public int Compare(SemVersion? x, SemVersion? y)
  {
    if (x == null && y == null) return 0;
    if (x == null) return -1;
    if (y == null) return 1;

    return x.ComparePrecedenceTo(y);
  }
}
