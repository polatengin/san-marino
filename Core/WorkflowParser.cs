using YamlDotNet.RepresentationModel;

public static class WorkflowParser
{
  public static List<GitHubActionReference> ExtractUsesFields(string yamlContent)
  {
    var references = new List<GitHubActionReference>();
    var yaml = new YamlStream();

    using var reader = new StringReader(yamlContent);
    yaml.Load(reader);

    if (yaml.Documents.Count == 0)
      return references;

    var root = (YamlMappingNode)yaml.Documents[0].RootNode;
    if (!root.Children.TryGetValue("jobs", out var jobsNode))
      return references;

    foreach (var job in (YamlMappingNode)jobsNode)
    {
      if (job.Value is not YamlMappingNode jobDetail)
        continue;

      if (!jobDetail.Children.TryGetValue("steps", out var stepsNode))
        continue;

      if (stepsNode is not YamlSequenceNode steps)
        continue;

      foreach (var stepNode in steps)
      {
        if (stepNode is not YamlMappingNode stepMapping)
          continue;

        foreach (var stepItem in stepMapping.Children)
        {
          if (stepItem.Key.ToString() != "uses")
            continue;

          var full = stepItem.Value.ToString();
          var split = full.Split('@');
          if (split.Length != 2)
            continue;

          var action = split[0];
          var version = split[1];

          var location = stepItem.Value.Start;

          references.Add(new GitHubActionReference
          {
            Action = action,
            Version = version,
            Line = location.Line,
            Column = location.Column
          });
        }
      }
    }

    return references;
  }
}
