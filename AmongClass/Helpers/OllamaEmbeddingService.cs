using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using System.Text.Json;

namespace AmongClass.Helpers
{
    // Suppress the SKEXP0001 diagnostic for the ITextEmbeddingGenerationService usage
    #pragma warning disable SKEXP0001
    public class OllamaEmbeddingService : ITextEmbeddingGenerationService
    {
        private readonly HttpClient _client;
        private readonly string _model;

        public OllamaEmbeddingService(HttpClient client, string model)
        {
            _client = client;
            _model = model;
            _client.Timeout = TimeSpan.FromSeconds(30);
        }

        public IReadOnlyDictionary<string, object?> Attributes =>
            new Dictionary<string, object?> { ["ModelId"] = _model };

        public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
            IList<string> data,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
            var embeddings = new List<ReadOnlyMemory<float>>();

            foreach (var text in data)
            {
                var embedding = await GetEmbeddingAsync(text, cancellationToken);
                embeddings.Add(embedding);
            }

            return embeddings;
        }

        private async Task<ReadOnlyMemory<float>> GetEmbeddingAsync(
            string text,
            CancellationToken cancellationToken)
        {
            var body = new { model = _model, prompt = text };

            var response = await _client.PostAsJsonAsync(
                "http://localhost:11434/api/embeddings",
                body,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"Ollama API error: {response.StatusCode} - {error}"
                );
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("embedding", out var embeddingElement))
            {
                return ParseEmbedding(embeddingElement);
            }

            if (doc.RootElement.TryGetProperty("embeddings", out var embeddingsElement) &&
                embeddingsElement.GetArrayLength() > 0)
            {
                return ParseEmbedding(embeddingsElement[0]);
            }

            throw new Exception($"Unexpected Ollama response format: {json}");
        }

        private ReadOnlyMemory<float> ParseEmbedding(JsonElement element)
        {
            var length = element.GetArrayLength();
            if (length == 0)
            {
                throw new Exception("Empty embedding array received");
            }

            var result = new float[length];
            int i = 0;

            foreach (var item in element.EnumerateArray())
            {
                result[i++] = item.GetSingle();
            }

            return new ReadOnlyMemory<float>(result);
        }
    }
    #pragma warning restore SKEXP0001
}
