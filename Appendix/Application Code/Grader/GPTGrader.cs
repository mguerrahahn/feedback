using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Grader;
public partial class GPTGrader
{
    public static int Grade(string language, string code, string input, string output)
    {

        string endpoint = "...";
        string deploymentName = "...";
        string apiKey = "...";
        string apiVersion = "...";

        HttpClient httpClient = new()
        {
            BaseAddress = new Uri(endpoint)
        };

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

        var requestBody = new
        {
            messages = new[]
            {
                    new { role = "system", content = $"You are a {language} teacher and will be provided with a student's solution written in {language} to calculate the grade. You must simulate compiling and executing the student's solution using the given input arguments to obtain the actual output, then compare it with the provided expected output, and afterward assign a percentage grade based on how well they match. Your response should include only the percentage with the phrase: percentage:" },
                    new { role = "user", content = $"Your student's solution in {language} is the following: {code}, the input arguments are: {input}, and the expected output is: {output}. If the grade is below 100%, provide suggestions." }
                },
            max_tokens = 100,
            temperature = 0.7
        };

        string json = JsonConvert.SerializeObject(requestBody);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        string url = $"/openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}";

        HttpResponseMessage response = httpClient.PostAsync(url, content).Result;
        string responseBody = response.Content.ReadAsStringAsync().Result;

        JObject result = JObject.Parse(responseBody);
        string reply = result["choices"]?[0]?["message"]?["content"]?.ToString() 
            ?? throw new Exception("Cloud Execution Failed");

        Console.WriteLine("Chatbot reply:");
        Console.WriteLine(reply);

        int grade = 0;

        Match match = PercentageRegex().Match(reply);

        if (match.Success)
        {
            grade = int.Parse(match.Groups[1].Value);

        }

        return grade;
    }

    [GeneratedRegex(@"percentage:\s*(\d+)")]
    private static partial Regex PercentageRegex();
}
