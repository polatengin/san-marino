public class DirectoryOperations
{
  public static (bool, List<string>) GetWorkflowFiles(DirectoryInfo path)
  {
    if (path == null || !path.Exists)
    {
      return (false, []);
    }

    return (
      true,
      [.. Directory.GetFiles(path.FullName, "*.yml", SearchOption.AllDirectories).Concat(Directory.GetFiles(path.FullName, "*.yaml", SearchOption.AllDirectories)).Where(f => f.Contains(".github/workflows") && !f.Contains("node_modules"))]
    );
  }
}
