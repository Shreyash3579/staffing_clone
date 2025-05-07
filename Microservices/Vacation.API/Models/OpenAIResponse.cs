using System.Collections.Generic;

namespace Vacation.API.Models
{
    class OpenAIResponse
    {
        public OpenAIChoice[] choices { get; set; }
    }

    class OpenAIChoice
    {
        public string text { get; set; }
    }

    public class GptRequest
    {
        public string model { get; set; }
        public List<PromptMessage> messages { get; set; }
        public double temperature { get; set; }
        public double presence_penalty { get; set; }
        public double frequency_penalty { get; set; }
        public int max_tokens { get; set; }
        public double top_p { get; set; }
        public string user { get; set; }
    }

    public class PromptMessage
    {
        public string role { get; set; }
        public string content { get; set; }

    }

    public class Message
    { 
        public string Role { get; set; }
        public string Content { get; set; }

    }

    public class Choice
    {
        public int Index { get; set; }
        public Message Message { get; set; }
        public string FinishReason { get; set; }
    }

    public class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    public class ChatCompletion
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public int Created { get; set; }
        public string Model { get; set; }
        public List<Choice> Choices { get; set; }
        public Usage Usage { get; set; }
    }

    public class EmbeddingResult
    {
        public string Object { get; set; }
        public List<EmbeddingData> Data { get; set; }
        public string Model { get; set; }
        public Usage Usage { get; set; }
    }

    public class EmbeddingData
    {
        public string Object { get; set; }
        public int Index { get; set; }
        public List<float> Embedding { get; set; }
    }

}
