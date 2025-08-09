namespace WebScraper
{
    public static partial class helper
    {
        public static List<string> MakeLinkList(List<string> Source, List<string> Filter)
        {
            List<string> Result;

            if (Filter.Count == 1)
            {
                return MakeLinkList(Source, Filter[0]);
            }
            else
            {
                Result = new List<string>();

                for (int i = 0; i < Filter.Count; i++)
                {
                    string Word = Filter[i];

                    List<string> sublist = Source.Where(Link => Link.Contains(Word)).ToList();

                    Result = Result.Concat(sublist).ToList();
                }
            }

            return Result;
        }
        //.....................................................................

        //.....................................................................
        public static List<string> MakeLinkList(List<string> Source, string Filter)
        {
            return Source.Where(Link => Link.Contains(Filter)).ToList();
        }
        //.....................................................................

        //.....................................................................
        public static List<string> Categories = new List<string>
        {
            "",
            ""
        };
        //.....................................................................

        //.....................................................................
    }
}