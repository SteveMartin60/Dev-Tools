@echo off
setlocal enabledelayedexpansion

:: Create folders
mkdir Models
mkdir ViewModels
mkdir Views
mkdir Services
mkdir Converters

:: Create files with placeholder content

:: Models/ChatMessage.cs
(
echo using System;
echo
echo namespace LLMApp.Models
echo {
echo     public class ChatMessage
echo     {
echo         public string Sender { get; set; } = string.Empty;
echo         public string Content { get; set; } = string.Empty;
echo         public DateTime Timestamp { get; set; } = DateTime.Now;
echo         public bool IsUserMessage => Sender == "You";
echo     }
echo }
) > Models\ChatMessage.cs

:: ViewModels/ViewModelBase.cs
(
echo using System.ComponentModel;
echo using System.Runtime.CompilerServices;
echo
echo namespace LLMApp.ViewModels
echo {
echo     public class ViewModelBase : INotifyPropertyChanged
echo     {
echo         public event PropertyChangedEventHandler? PropertyChanged;
echo
echo         protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
echo         {
echo             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
echo         }
echo
echo         protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
echo         {
echo             if (EqualityComparer<T>.Default.Equals(field, value)) return false;
echo             field = value;
echo             OnPropertyChanged(propertyName);
echo             return true;
echo         }
echo     }
echo }
) > ViewModels\ViewModelBase.cs

:: ViewModels/MainViewModel.cs
(
echo using System.Collections.ObjectModel;
echo using System.Windows.Input;
echo using LLMApp.Models;
echo using LLMApp.Services;
echo
echo namespace LLMApp.ViewModels
echo {
echo     public class MainViewModel : ViewModelBase
echo     {
echo         private string _userInput = string.Empty;
echo         private readonly ILlmService _llmService;
echo         
echo         public ObservableCollection<ChatMessage> Messages { get; } = new();
echo         
echo         public string UserInput
echo         {
echo             get => _userInput;
echo             set => SetField(ref _userInput, value);
echo         }
echo         
echo         public ICommand SendCommand { get; }
echo         
echo         public MainViewModel(ILlmService llmService)
echo         {
echo             _llmService = llmService;
echo             SendCommand = new RelayCommand(async () => await SendMessage());
echo             
echo             // Welcome message
echo             Messages.Add(new ChatMessage { Sender = "AI", Content = "Hello! How can I help you today?" });
echo         }
echo         
echo         private async Task SendMessage()
echo         {
echo             if (string.IsNullOrWhiteSpace(UserInput)) return;
echo             
echo             var userMessage = new ChatMessage { Sender = "You", Content = UserInput };
echo             Messages.Add(userMessage);
echo             
echo             string input = UserInput;
echo             UserInput = string.Empty; // Clear input box
echo             
echo             try
echo             {
echo                 string response = await _llmService.GetResponseAsync(input);
echo                 Messages.Add(new ChatMessage { Sender = "AI", Content = response });
echo             }
echo             catch (Exception ex)
echo             {
echo                 Messages.Add(new ChatMessage { Sender = "System", Content = $"Error: {ex.Message}" });
echo             }
echo         }
echo     }
echo }
) > ViewModels\MainViewModel.cs

:: ViewModels/RelayCommand.cs
(
echo using System;
echo using System.Windows.Input;
echo
echo namespace LL.ViewModels
echo {
echo     public class RelayCommand : ICommand
echo     {
echo         private readonly Action _execute;
echo         private readonly Func<bool>? _canExecute;
echo
echo         public event EventHandler? CanExecuteChanged;
echo
echo         public RelayCommand(Action execute, Func<bool>? canExecute = null)
echo         {
echo             _execute = execute;
echo             _canExecute = canExecute;
echo         }
echo
echo         public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
echo
echo         public void Execute(object? parameter) => _execute();
echo
echo         public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
echo     }
echo }
) > ViewModels\RelayCommand.cs

:: Services/ILlmService.cs
(
echo using System.Threading.Tasks;
echo
echo namespace LLMApp.Services
echo {
echo     public interface ILlmService
echo     {
echo         Task<string> GetResponseAsync(string prompt);
echo     }
echo }
) > Services\ILlmService.cs

:: Services/MockLlmService.cs
(
echo using System.Threading.Tasks;
echo
echo namespace LLMApp.Services
echo {
echo     public class MockLlmService : ILlmService
echo     {
echo         public async Task<string> GetResponseAsync(string prompt)
echo         {
echo             await Task.Delay(500); // Simulate network delay
echo             
echo             return $"I received your message: '{prompt}'. This is a mock response.";
echo         }
echo     }
echo }
) > Services\MockLlmService.cs

:: Converters/BoolToColorConverter.cs
(
echo using System;
echo using System.Globalization;
echo using System.Windows.Data;
echo using System.Windows.Media;
echo
echo namespace LLMApp.Converters
echo {
echo     public class BoolToColorConverter : IValueConverter
echo     {
echo         public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
echo         {
echo             return (bool)value ? Brushes.DodgerBlue : Brushes.DarkOrange;
echo         }
echo
echo         public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
echo         {
echo             throw new NotImplementedException();
echo         }
echo     }
echo }
) > Converters\BoolToColorConverter.cs

:: Views/MainWindow.xaml
(
echo ^<Window x:Class="LLMApp.MainWindow"
echo         xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
echo         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
echo         xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
echo         xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
echo         xmlns:local="clr-namespace:LLMApp"
echo         mc:Ignorable="d"
echo         Title="LLM Chat" Height="600" Width="800"^>
echo     ^<Grid^>
echo         ^<Grid.RowDefinitions^>
echo             ^<RowDefinition Height="*"/^>
echo             ^<RowDefinition Height="Auto"/^>
echo         ^</Grid.RowDefinitions^>
echo         
echo         ^<!-- Message Display --^>
echo         ^<ListView ItemsSource="{Binding Messages}" 
echo                   Background="Transparent" 
echo                   BorderThickness="0"
echo                   ScrollViewer.HorizontalScrollBarVisibility="Disabled"^>
echo             ^<ListView.ItemTemplate^>
echo                 ^<DataTemplate^>
echo                     ^<StackPanel Margin="10,5"^>
echo                         ^<TextBlock Text="{Binding Sender}" 
echo                                    FontWeight="Bold"
echo                                    Foreground="{Binding IsUserMessage, Converter={StaticResource BoolToColorConverter}}"/^>
echo                         ^<TextBlock Text="{Binding Content}" 
echo                                    TextWrapping="Wrap"
echo                                    Margin="20,0,0,0"/^>
echo                         ^<TextBlock Text="{Binding Timestamp, StringFormat='hh:mm tt'}" 
echo                                    FontStyle="Italic"
echo                                    Foreground="Gray"/^>
echo                     ^</StackPanel^>
echo                 ^</DataTemplate^>
echo             ^</ListView.ItemTemplate^>
echo         ^</ListView^>
echo         
echo         ^<!-- Input Area --^>
echo         ^<Grid Grid.Row="1" Margin="10"^>
echo             ^<Grid.ColumnDefinitions^>
echo                 ^<ColumnDefinition Width="*"/^>
echo                 ^<ColumnDefinition Width="Auto"/^>
echo             ^</Grid.ColumnDefinitions^>
echo             
echo             ^<TextBox Text="{Binding UserInput, UpdateSourceTrigger=PropertyChanged}" 
echo                      VerticalContentAlignment="Center"
echo                      Padding="10"
echo                      FontSize="14"
echo                      AcceptsReturn="True"
echo                      MinHeight="50"
echo                      MaxHeight="150"
echo                      VerticalScrollBarVisibility="Auto"/^>
echo             
echo             ^<Button Grid.Column="1"                     Content="Send" 
echo                     Command="{Binding SendCommand}"
echo                     Margin="10,0,0,0"
echo                     Padding="20,10"
echo                     FontWeight="Bold"/^>
echo         ^</Grid^>
echo     ^</Grid^>
echo ^</Window^>
) > Views\MainWindow.xaml

:: Views/MainWindow.xaml.cs
(
echo using LLMApp.Services;
echo using LLMApp.ViewModels;
echo using System.Windows;
echo
echo namespace LLMApp
echo {
echo     public partial class MainWindow : Window
echo     {
echo         public MainWindow()
echo         {
echo             InitializeComponent();
echo             
echo             // In a real app, you would use DI here
echo             var llmService = new MockLlmService();
echo             DataContext = new MainViewModel(llmService);
echo         }
echo     }
echo }
) > Views\MainWindow.xaml.cs

:: Create App.xaml and App.xaml.cs
(
echo ^<Application x:Class="LLMApp.App"
echo              xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
echo              xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
echo              xmlns:local="clr-namespace:LLMApp"
echo              StartupUri="Views/MainWindow.xaml"^>
echo     ^<Application.Resources^>
echo         ^<local:BoolToColorConverter x:Key="BoolToColorConverter"/^>
echo     ^</Application.Resources^>
echo ^</Application^>
) > App.xaml

(
echo using System.Windows;
echo
echo namespace LLMApp
echo {
echo     public partial class App : Application
echo     {
echo     }
echo }
) > App.x

echo Project structure created successfully!
pause
