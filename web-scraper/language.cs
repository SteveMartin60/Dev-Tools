using System.Diagnostics;
using System.Text.RegularExpressions;

namespace WebScraper
{
    public partial class MainWindow
    {
        //......................................................................
        #region Begin Region Language Utils
        //.....................................................................
        private static void DoLanguageDetection(List<string> FaceList)
        {
            int FoundCount = 0;
            var StringList = new List<string>();

            for (int i = 0; i < FaceList.Count; i++)
            {
                var SourceLine = FaceList[i];

                if (SourceLine.Length > 3)
                {
                    string PartA = SourceLine.Split(">>")[1].Replace(".json", "");
                    string PartB = PartA.Substring(PartA.LastIndexOf("\\", PartA.Length - 1));
                    string PartC = PartB.Replace(")",  "" )
                                        .Replace("\\", "" )
                                        .Replace("\\", "" )
                                        .Replace("#",  "" )
                                        .Replace(".",  "" )
                                        .Replace("-",  " ")
                                        .Replace("\"", " ")
                                        .Replace("'",  " ");

                    string PartD = PartC.Substring(0, PartC.Length - 3);

                    string Line = Regex.Replace(PartC, @"[\d-]", " ").Trim();

                    while (Line.Contains("  "))
                    {
                        Line = Line.Replace("  ", " ");
                    }

                    Line = Line.Trim();


                    var Language = GetLanguage(Line);

                    if (HasChinese(Line) || (Language != "eng" && Language != "Unknown"
                                          ))
                    {
                        FoundCount++;

                        StringList.Add(Line);

                    }
                }
            }

            string result = string.Join(Environment.NewLine, StringList.ToArray());

        }
        //.....................................................................

        //.....................................................................
        public static bool HasChinese(string text)
        {
            string regExpression = "[\u4e00-\u9fa5]";

            DoLanguageDetection(new List<string>());

            return Regex.IsMatch(text, regExpression);
        }
        //.....................................................................

        //.....................................................................
        private bool IsEnglish(string Text)
        {
            return Detector.Detect(Text) == "eng";
        }
        //.....................................................................

        //.....................................................................
        private bool IsChinese(string Text)
        {
            return Detector.Detect(Text) == "zho";
        }
        //.....................................................................

        //.....................................................................
        private static string GetLanguage(string Text)
        {
            return "";
        }
        //.....................................................................
        #endregion End Region Language Utils
        //......................................................................
    }
}


