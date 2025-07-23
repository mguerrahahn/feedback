using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Grader;
public partial class UnitGrader
{
    protected static void ExportToCsv(string outputFilePath, List<(string FileName, double Grade, int GradeGPT, double time)> results)
    {
        using StreamWriter writer = new(outputFilePath + ".csv");
        writer.WriteLine("#,Data-set path,Unit Test Grade,ChatGPT Grade,Execution Time");
        int counter = 1;
        foreach ((string file, double grade, int gradeGPT, double time) in results)
        {
            writer.WriteLine($"{counter++},{file},{grade},{gradeGPT},{time}");
        }
        Console.WriteLine($"\nResults exported to: {Path.GetFullPath(outputFilePath)}");
    }

    protected static int RunCode(DirectoryInfo root, string code, string progLanguage, string version)
    {
        var request = new
        {
            clientId = "...",
            clientSecret = "...",
            script = code,
            language = progLanguage,
            versionIndex = version
        };

        string json = JsonConvert.SerializeObject(request);
        StringContent content = new(json, Encoding.UTF8, "application/json");
        HttpClient http = new();
        HttpResponseMessage response = http.PostAsync("https://api.jdoodle.com/v1/execute", content).Result;
        string responseBody = response.Content.ReadAsStringAsync().Result;

        JObject jsonresult = JObject.Parse(responseBody);
        string actualOutput = jsonresult["output"]?.ToString() 
            ?? throw new Exception("Cloud Execution Failed");
        int maxPassed = 0, total = 0;

        string expected = "";

        foreach (FileInfo fi in root.GetFiles("expected*"))
        {
            string expectedOutput = File.ReadAllText(fi.FullName);
            if (expected == "")
                expected = expectedOutput;
            Grade(actualOutput, expectedOutput, out int passed, out total);
            maxPassed = Math.Max(maxPassed, passed);
        }
        int grade = (int)(100.00 * maxPassed / total);

        return grade;
    }

    protected static void Grade(string actualOutput, string expectedOutput, out int passed, out int total)
    {
        Dictionary<string, List<string>> expected = ParseTests(expectedOutput);
        Dictionary<string, List<string>> actual = ParseTests(actualOutput);

        passed = 0;
        total = expected.Count;
        foreach (KeyValuePair<string, List<string>> test in expected)
        {
            if (actual.TryGetValue(test.Key, out List<string>? actualResult) &&
                test.Value.SequenceEqual(actualResult))
            {
                passed++;
            }
        }
    }
    protected static Dictionary<string, List<string>> ParseTests(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return [];

        input = ConsecutiveNewlinesRegex().Replace(input, "\n");
        Dictionary<string, List<string>> result = [];
        string currentTest = "";
        foreach (string line in input.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("Test "))
            {
                currentTest = trimmed;
                result[currentTest] = [];
            }
            else if (currentTest != null)
            {
                result[currentTest].Add(trimmed);
            }
        }
        return result;
    }

    [GeneratedRegex(@"(\r?\n){2,}")]
    private static partial Regex ConsecutiveNewlinesRegex();
}
