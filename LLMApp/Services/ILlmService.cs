namespace LLMApp
{
    public interface ILlmService
    {
        Task<string> GetResponseAsync(string prompt);
    }
}
