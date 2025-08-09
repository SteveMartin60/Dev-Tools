using LanguageDetection;
using System.IO;
using System.Net.Http;
using System.Windows;
namespace WebScraper
{
    public partial class MainWindow : Window
    {
        LanguageDetector Detector = new LanguageDetector();

        //....................................................................
        #region begin Region Read-Only Properties
        //....................................................................
        static string projectDir = AppDomain.CurrentDomain.BaseDirectory;
        public static string SourceRoot { get => Directory.GetParent(projectDir)?.Parent?.Parent?.Parent?.FullName; }          
        static string AppFolder       { get => AppDomain.CurrentDomain.BaseDirectory;}
        static string ListPath        { get => Path.Combine(SourceRoot, "lists");}
        static string ExeYTDLP        { get => $"yt-dlp.exe";}
        static string PathYTDLP       { get => $@"D:\yt-dl\{ExeYTDLP}";}
        static string FileConfigYTDLP { get => @"yt-dlp.txt";}
        static string PathConfigYTDLP { get => Path.Combine($"{PathYTDLP}{FileConfigYTDLP}"); }
        static string ChannelsFile    { get => Path.Combine(ListPath, "channels.txt"    );}
        static string CategoriesFile  { get => Path.Combine(ListPath, "categories.txt"  );}
        static string TagsFile        { get => Path.Combine(ListPath, "tags.txt"        );}
        static string SearchTermsFile { get => Path.Combine(ListPath, "search-terms.txt"); }
        List<string> SearchTerms      { get => File.ReadAllLines(SearchTermsFile).ToList();}
        List<string> Channels         { get => File.ReadAllLines(ChannelsFile   ).ToList();}
        List<string> Categories       { get => File.ReadAllLines(CategoriesFile ).ToList();}
        List<TagEntry> Tags           { get => ParseTagFile(TagsFile).OrderBy(tag => tag.TagName).ToList();}
        


        //....................................................................
        #endregion End Region Read-Only Properties
        //....................................................................
        #region Begin Region Read-Write Properties
        //....................................................................
        List<string>         Links              { get; set; } = new List<string>();
        string?              SelectedSearchTerm { get; set; }
        string?              BaseUrl            { get; set; }
        HttpResponseMessage? response           { get; set; }

        //....................................................................
        #endregion End Region Read-Write Properties
        //....................................................................

        //....................................................................
        public MainWindow()
        {
            InitializeComponent();

            DoWebPage("https://youdao.com/result?word=quantum%20chromodynamics&lang=en");
            DoWebPage("https://youdao.com/result?word=Schrödinger%20Equation&lang=en");

            string listsFolder = Path.Combine(AppFolder, "lists");
            string filePath = Path.Combine(listsFolder, "channels.txt");
            
            Left = 4500;

            Task Result = null;
        }
        //.....................................................................

        //.....................................................................
        private void DoWebPage(string BaseUrl)
        {
            Task Result;

            string ReferrenceString = "量子色动力学";

            Result = ProcessYouDaoPage(BaseUrl, ReferrenceString);

            while (Result.Status != TaskStatus.RanToCompletion)
            {
                MeshDoEvents();
            }

            DoLog("");
        }
        //.....................................................................

        //.....................................................................
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                ComboBox_Search    .ItemsSource = SearchTerms;
                ComboBox_Channels  .ItemsSource = Channels;
                ComboBox_Tags      .ItemsSource = Tags;
                ComboBox_Categories.ItemsSource = Categories;

                ComboBox_Tags.DisplayMemberPath = "TagName";
                ComboBox_Tags.SelectedValuePath = "Url";
                ComboBox_Tags.SelectedIndex = 1;

                DoLog($"Added {Tags.Count} Tags");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tags: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //.....................................................................

        //.....................................................................
        private List<string> DoTranslate(string text)
        {
            DoLog($"Translated: {text}");

            return Links;
        }

        //.....................................................................

        //.....................................................................
        private void ButtonDoTranslate_Click(object sender, RoutedEventArgs e)
        {
            DoTranslate("");
        }
        //.....................................................................

        //.....................................................................
    }
}