using Octokit;
using Semver;

public class ActionVersionChecker
{
  private readonly GitHubClient _client;
  private readonly Dictionary<string, string?> _cache = new();

  public ActionVersionChecker(string userAgent = "SanMarino-Core")
  {
    _client = new GitHubClient(new ProductHeaderValue(userAgent));
  }

  public async Task<string?> GetLatestVersionAsync(string action)
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
      var versions = tags
          .Select(t => t.Name)
          .Where(name => name.StartsWith("v"))
          .Select(name => SemVersion.TryParse(name.TrimStart('v'), SemVersionStyles.Any, out var ver) ? ver : null)
          .Where(v => v != null)
          .Cast<SemVersion>()
          .OrderByDescending(v => v)
          .ToList();

      var latest = versions.FirstOrDefault();
      if (latest == null)
        return null;

      var latestStr = "v" + latest.ToString();
      _cache[action] = latestStr;
      return latestStr;
    }
    catch
    {
      return null;
    }
  }

  public static bool IsNewer(string currentVersion, string latestVersion)
  {
    if (SemVersion.TryParse(currentVersion.TrimStart('v'), SemVersionStyles.Any, out var current) &&
        SemVersion.TryParse(latestVersion.TrimStart('v'), SemVersionStyles.Any, out var latest))
    {
      return latest.ComparePrecedenceTo(current) > 0;
    }

    return false;
  }
}
