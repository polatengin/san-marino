public class DirectoryOperations
{
  public static List<string> GetWorkflowFiles(DirectoryInfo path)
  {
    return Directory.GetFiles(path.FullName, "*.yml", SearchOption.AllDirectories)
        .Concat(Directory.GetFiles(path.FullName, "*.yaml", SearchOption.AllDirectories))
        .Where(f => f.Contains(".github/workflows") && !f.Contains("node_modules"))
        .ToList();
  }
}
