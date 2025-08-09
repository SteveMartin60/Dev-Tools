namespace LLMApp
{
    public class ChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsUserMessage => Sender == "You";
    }
}
