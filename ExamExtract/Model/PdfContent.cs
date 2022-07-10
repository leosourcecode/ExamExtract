using System.Text;

namespace ExamExtract.Model
{
    public class PdfContent
    {
        public Dictionary<int, string> Pages { get; set; }
        public List<Image> Images { get; set; }

        public int TotalPages()
        {
            return Pages.Count;
        }

        public string GetFirstPage()
        {
            return Pages.Count > 0 ? Pages.Where(p => p.Key == 1).Select(s => s.Value).FirstOrDefault() : string.Empty;
        }

        public string GetPage(int page)
        {
            if(Pages != null && Pages.Where(p => p.Key == page) != null)
                return Pages.Where(p => p.Key == page).Select(s => s.Value).FirstOrDefault();

            return string.Empty;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach(var page in Pages.OrderBy(o => o.Key))
            {
                builder.AppendLine(page.Value);
            }

            return builder.ToString();
        }
    }

    public class Image
    {
        public string Path { get; set; }
        public int Page { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
