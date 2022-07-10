using System.Text;

namespace ExamExtract.Model
{
    public class HtmlElements
    {
        public TagType Type { get; set; }
        public string Placeholder { get; set; }
        public string Placeholder2 { get; set; }
        public string Placeholder3 { get; set; }
        public string Html { get; set; }

        public string GetHtmlText()
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(Html));
        }

        public enum TagType
        {
            FileName = 0,
            Name = 1,
            Score = 2,
            NewLine = 3,
            Question = 4,
            Answer = 5,
            Correct = 6,
            Section = 7,
            Image = 8,
            Time = 9,
            Description = 10
        }
    }
}
