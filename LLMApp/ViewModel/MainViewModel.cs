using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LLMApp
{
    public class MainViewModel : ViewModelBase
    {
        private string _userInput = string.Empty;
        private readonly ILlmService _llmService;

        public ObservableCollection<ChatMessage> Messages { get; } = new();

        public string UserInput
        {
            get => _userInput;
            set => SetField(ref _userInput, value);
        }

        public ICommand SendCommand { get; }

        public MainViewModel(ILlmService llmService)
        {
            _llmService = llmService;
            SendCommand = new RelayCommand(async () => await SendMessage());

            // Welcome message
            Messages.Add(new ChatMessage { Sender = "AI", Content = "Hello! How can I help you today?" });
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(UserInput)) return;

            var userMessage = new ChatMessage { Sender = "You", Content = UserInput };
            Messages.Add(userMessage);

            string input = UserInput;
            UserInput = string.Empty; // Clear input box

            try
            {
                string response = await _llmService.GetResponseAsync(input);
                Messages.Add(new ChatMessage { Sender = "AI", Content = response });
            }
            catch (Exception ex)
            {
                Messages.Add(new ChatMessage { Sender = "System", Content = $"Error: {ex.Message}" });
            }
        }
    }
}
