using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quipbot.Providers
{
    public class OpenAIProvider : ResultProviderBase<DummyBrowserContainer>
    {
        private readonly OpenAIService _service = new(new OpenAiOptions
        {
            ApiKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY")!
        });

        public string Model { get; set; } = Models.TextDavinciV3;

        public string PromptAdjective { get; set; } = "funny";

        protected override async Task SetupAsync() => await Task.CompletedTask; // NOP

        public override async Task<string?> ProvideResult(string input, TimeSpan? timeout = null)
        {
            await foreach (var item in ProvideResult(input, 1, timeout))
            {
                return item;
            }

            return null;
        }

        public override async IAsyncEnumerable<string> ProvideResult(string input, int count, TimeSpan? timeout = null)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count));

            var prompt = count == 1
                ? $"Give a {PromptAdjective} answer for the following question with a maximum of 45 characters: {input}"
                : $"Give {count} short and {PromptAdjective} answers seperated by semicolons for the following question: {input}";

            var completionResult = await _service.Completions.CreateCompletion(new CompletionCreateRequest
            {
                Prompt = prompt,
                Model = Model,
                Temperature = 0.9f
            });

            if (completionResult.Successful)
            {
                var result = completionResult.Choices.FirstOrDefault()?.Text
                    .Replace("\"", string.Empty)
                    .Replace("&amp;", "&")
                    .Trim()
                    .TrimStart('?')
                    .TrimEnd('.')
                    .Trim();

                if (string.IsNullOrWhiteSpace(result))
                    yield break;

                if (count == 1)
                {
                    yield return result;
                    yield break;
                }   

                foreach (var item in result.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    yield return item;
                }

                yield break;
            }

            throw new Exception($"{completionResult.Error?.Code}: {completionResult.Error?.Message}");
        }
    }
}