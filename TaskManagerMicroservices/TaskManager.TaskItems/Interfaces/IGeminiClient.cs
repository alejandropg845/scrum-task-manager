
using TaskManager.Common.DTOs;

namespace TaskManager.TaskItems.Interfaces
{
    public interface IGeminiClient
    {
        Task<string> AskToGeminiAsync(AskToAssistantDto dto);
    }
}
