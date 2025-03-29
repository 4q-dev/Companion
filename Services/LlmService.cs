using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace Zazagram.Services;
internal static class LlmService {
    private static HttpClient httpClient = ConstructClient();

    private static HttpClient ConstructClient() {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("ZAZAGROK_TOKEN")}");
        return client;
    }
    // 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
    public async static Task<String> Recognize(String userinput, List<(String command, String desc)> commands) {
        var res = await httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", new StringContent(JsonSerializer.Serialize(new {
            model = "llama-3.3-70b-versatile",
            messages = new List<Object> {
                new {
                    role = "user",
                    content = userinput,
                }
            }
        })));

        Log.Information($"Recieved answer {res}");

        var jsonResponse = await res.Content.ReadAsStringAsync();

        var responseObject = JsonSerializer.Deserialize<ChatCompletionResponse>(jsonResponse);

        var content = responseObject?.choices?[0].message?.content switch {
            null => "",
            var c => c
        };

        return content;
    }
}

public class ChatCompletionResponse {
    public String? id { get; set; }
    public String? @object { get; set; }
    public Int64 created { get; set; }
    public String? model { get; set; }
    public List<Choice>? choices { get; set; }
    public Usage? usage { get; set; }
    public String? systemFingerprint { get; set; }
    public XGroq? xGroq { get; set; }
}

public class Choice {
    public Int32 index { get; set; }
    public Message? message { get; set; }
    public Object? logprobs { get; set; }
    public String? finishReason { get; set; }
}

public class Message {
    public String? role { get; set; }
    public String? content { get; set; }
}

public class Usage {
    public Double queueTime { get; set; }
    public Int32 promptTokens { get; set; }
    public Double promptTime { get; set; }
    public Int32 completionTokens { get; set; }
    public Double completionTime { get; set; }
    public Int32 totalTokens { get; set; }
    public Double totalTime { get; set; }
}

public class XGroq {
    public String? id { get; set; }
}
