using AmongClass.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AmongClass.Controllers
{
    public class OllamaController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly SimpleRagService _rag;

        public OllamaController(IHttpClientFactory httpClientFactory, SimpleRagService rag)
        {
            _httpClient = httpClientFactory.CreateClient();
            _rag = rag;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ask(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                ViewBag.Error = "Prompt is required.";
                return View("Index");
            }

            string rules = await _rag.GetRelevantRules(prompt);
            string fullPrompt = $"Rules:\n{rules}\n\nQuestion: {prompt}";

            var request = new
            {
                model = "gwen2.5",
                prompt = fullPrompt,
                stream = false
            };

            var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/generate", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ViewBag.Error = $"Ollama Error: {error}";
                return View("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            string modelResponse = doc.RootElement.GetProperty("response").GetString();

            ViewBag.ModelResponse = modelResponse;
            ViewBag.Prompt = prompt;

            return View("Index");
        }
    }
}