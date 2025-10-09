using PasswordGenerator.Models;
using PasswordGenerator.Services;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PasswordGenerator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly PassphraseGenerator _generator;
        private string _passphrase;
        private int _passphraseCount = 5;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Passphrase
        {
            get => _passphrase;
            set
            {
                _passphrase = value;
                OnPropertyChanged();
            }
        }

        public int PassphraseCount
        {
            get => _passphraseCount;
            set
            {
                _passphraseCount = value;
                OnPropertyChanged();
            }
        }

        public ICommand GenerateCommand { get; }

        public MainViewModel()
        {
            var wordListService = new WordListService();
            var wordList = wordListService.GetDefaultWordLists();
            _generator = new PassphraseGenerator(wordList);

            GenerateCommand = new RelayCommand(GeneratePassphrases);
        }

        private void GeneratePassphrases()
        {
            string PassPhraseFile = @"D:\Dev-Tools\password-generator\passphrases.txt";

            var passphrases = new List<string>();

            for (int i = 0; i < PassphraseCount; i++)
            {
                passphrases.Add(_generator.GeneratePassphrase());
            }

            Passphrase = string.Join("\n", passphrases);

            if (File.Exists(PassPhraseFile))
            {
                File.AppendAllLines(PassPhraseFile, passphrases);
            }
            else
            {
                File.WriteAllLines(PassPhraseFile, passphrases);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
