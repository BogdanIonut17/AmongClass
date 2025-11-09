using System.Text.Json;
namespace AmongClass.Helpers
{
    public class RagService
    {
        private readonly HttpClient _client;
        private readonly List<RagDocument> _rules;
        public RagService(IHttpClientFactory factory)
        {
            _client = factory.CreateClient();
            _rules = new List<RagDocument>
            {
                new RagDocument { Id = 1, Description = "Your name is Razvan, you always respond and talk only in romanian, be respectful and professional. Explain code clearly." },
                new RagDocument { Id = 2, Description = "Always give me response in plain text purely. Dont use characters from markdown format" },
                new RagDocument { Id = 3, Description = "Do not use markdown format, just plain text and short response" }, // Do not generate in markdown format, use MAX 10k Tokens
                new RagDocument { Id = 4, Description = "Use async/await for I/O operations. Keep code simple and readable." },
                new RagDocument { Id = 5, Description = "Always validate user input and handle exceptions properly." }
            };
        }
        public async Task InitAsync()
        {
            foreach (var rule in _rules)
            {
                rule.Embedding = await GetEmbedding(rule.Description);
            }
        }
        private async Task<float[]> GetEmbedding(string text)
        {
            var body = new { model = "nomic-embed-text", prompt = text };
            var response = await _client.PostAsJsonAsync("http://localhost:11434/api/embeddings", body);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine(json);

            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("embeddings", out var embeddings))
            {
                var arr = embeddings[0];
                var result = new float[arr.GetArrayLength()];
                int i = 0;
                foreach (var item in arr.EnumerateArray())
                {
                    result[i++] = item.GetSingle();
                }
                return result;
            }

            if (doc.RootElement.TryGetProperty("embedding", out var embedding))
            {
                var result = new float[embedding.GetArrayLength()];
                int i = 0;
                foreach (var item in embedding.EnumerateArray())
                {
                    result[i++] = item.GetSingle();
                }
                return result;
            }

            throw new Exception($"Unexpected response format: {json}");
        }
        public async Task<string> GetRelevantRules(string question)
        {
            var questionEmbed = await GetEmbedding(question);
            var top = _rules
                .Select(r => new { Rule = r, Score = CosineSim(questionEmbed, r.Embedding) })
                .OrderByDescending(x => x.Score)
                .Take(2)
                .Select(x => x.Rule.Description)
                .ToList();
            return string.Join("\n", top);
        }
        private float CosineSim(float[] a, float[] b)
        {
            float dot = 0, m1 = 0, m2 = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                m1 += a[i] * a[i];
                m2 += b[i] * b[i];
            }
            return dot / ((float)Math.Sqrt(m1) * (float)Math.Sqrt(m2));
        }
    }
}