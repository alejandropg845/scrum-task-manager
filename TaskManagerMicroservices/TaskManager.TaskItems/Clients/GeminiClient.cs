
using TaskManager.Common.DTOs;
using TaskManager.TaskItems.Interfaces;

namespace TaskManager.TaskItems.Clients
{
    public class GeminiClient : IGeminiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseAddress;
        private readonly string _defaultPrompt;
        public GeminiClient(HttpClient c, IConfiguration config)
        {
            _httpClient = c;
            _baseAddress = config["Gemini:BaseAddress"]!;
            _defaultPrompt = config["Gemini:Default_Prompt"]!;
        }

        public async Task<string> AskToGeminiAsync(AskToAssistantDto dto)
        {
            string? previous = null;

            if (dto.PreviousResponse is not null && dto.Prompt is not null)
            {
                previous = $"Tu respuesta anterior fue: {dto.PreviousResponse} " +
                    $"y el usuario ha enviado el siguiente mensaje: {dto.Prompt}";
            }

            string prompt = $"{_defaultPrompt}\nDescripción de la tarea a completar: {dto.TaskContent}\n{previous ?? string.Empty}";

            var objectBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };

            var content = JsonContent.Create(objectBody);

            var request = new HttpRequestMessage(HttpMethod.Post, _baseAddress);

            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            return await response.Content.ReadAsStringAsync();
        }
    }

}
