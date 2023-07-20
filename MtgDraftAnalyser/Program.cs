using IO;
using Util;

var fileName = args[0];
if (fileName.EndsWith(".csv"))
{
    fileName = fileName.Replace(".csv", "");
}

var fullPath = Path.GetFullPath($"./17lands/{fileName}.csv");

var (parsedRows, parseDuration) = Timing.Time(() => DraftCsvParser.Parse(fullPath));
var parsedDrafts = parsedRows.GroupBy(r => r.DraftId).ToDictionary(g => g.Key, g => g.ToList());

Console.WriteLine($"Done Parsing in {parseDuration.TotalSeconds}s");
Console.WriteLine($"Parsed {parsedRows.Count} rows, {parsedDrafts.Count} drafts");

var (modelDrafts, transformDuration) = Timing.Time(() => parsedDrafts.Select(kvp => DraftRow.ToDraftModel(kvp.Key, kvp.Value)).ToList());
Console.WriteLine($"Done Transforming in {transformDuration.TotalSeconds}s");

// Clear memory before writing JSON
MemoryProfiling.Profile("Clearing CSV parsing resources", () =>
{
    parsedRows.Clear();
    parsedDrafts.Clear();
});

var outputDuration = Timing.Time(() => JsonIO.SerializeDraftsToFile(fileName, modelDrafts));
Console.WriteLine($"Done Serialising in {outputDuration.TotalSeconds}s");