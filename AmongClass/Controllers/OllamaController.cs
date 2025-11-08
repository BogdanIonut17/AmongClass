using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AmongClass.Controllers
{
    public class OllamaController : Controller
    {
        private readonly HttpClient _httpClient;

        public OllamaController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // GET: Ollama
        public IActionResult Index()
        {
            return View();
        }

        // POST: Ollama/Ask
        [HttpPost]
        public async Task<IActionResult> Ask(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                ViewBag.Error = "Prompt is required.";
                return View("Index");
            }

            var ollamaRequest = new
            {
                model = "llama3.2",
                prompt = prompt,
                stream = false
            };

            var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/generate", ollamaRequest);

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