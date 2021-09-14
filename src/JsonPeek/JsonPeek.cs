﻿using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json.Linq;

/// <summary>
/// Returns values as specified by a JSONPath Query from a JSON file.
/// </summary>
public class JsonPeek : Task
{
    /// <summary>
    /// Specifies the JSON input as a string.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Specifies the JSON input as a file path.
    /// </summary>
    public ITaskItem? ContentPath { get; set; }

    /// <summary>
    /// Specifies the JSONPath query.
    /// </summary>
    [Required]
    public string Query { get; set; } = "$";

    /// <summary>
    /// Contains the results that are returned by the task.
    /// </summary>
    [Output]
    public ITaskItem[] Result { get; private set; } = new ITaskItem[0];

    /// <summary>
    /// Executes the <see cref="Query"/> against either the 
    /// <see cref="Content"/> or <see cref="ContentPath"/> JSON.
    /// </summary>
    public override bool Execute()
    {
        if (Content == null && ContentPath == null)
            return Log.Error("JPE01", $"Either {nameof(Content)} or {nameof(ContentPath)} must be provided.");

        if (ContentPath != null && !File.Exists(ContentPath.GetMetadata("FullPath")))
            return Log.Error("JPE02", $"Specified {nameof(ContentPath)} not found at {ContentPath.GetMetadata("FullPath")}.");

        if (Content != null && ContentPath != null)
            return Log.Error("JPE03", $"Cannot specify both {nameof(Content)} and {nameof(ContentPath)}.");

        var content = ContentPath != null ?
            File.ReadAllText(ContentPath.GetMetadata("FullPath")) : Content;

        if (string.IsNullOrEmpty(content))
            return Log.Warn("JPE04", $"Empty JSON content.", true);

        var json = JObject.Parse(content!);

        Result = json.SelectTokens(Query).SelectMany(x => x.AsItems()).ToArray();

        return true;
    }
}
