namespace JH.Service.Reddit.Models
{
    public class SubredditEventArgs : EventArgs
    {
        public string After { get; set; }

        public string Before { get; set; }
    }
}