using CoreHtmlToImage;
using ExamExtract.Model;
using System.Text;

namespace ExamExtract.Services
{
    public class HtmlService
    {
        public string BuildHtml(Exam exam)
        {
            var elements = BuildHtmlElements();

            var html = new StringBuilder();

            var element = elements.Where(s => s.Type == HtmlElements.TagType.FileName).FirstOrDefault();
            html.AppendLine(element.GetHtmlText().Replace(element.Placeholder, exam.FileName));

            element = elements.Where(s => s.Type == HtmlElements.TagType.Name).FirstOrDefault();
            html.AppendLine(element.GetHtmlText().Replace(element.Placeholder, exam.Name));

            element = elements.Where(s => s.Type == HtmlElements.TagType.Score).FirstOrDefault();
            html.AppendLine(element.GetHtmlText().Replace(element.Placeholder, exam.PassingScore));

            element = elements.Where(s => s.Type == HtmlElements.TagType.Time).FirstOrDefault();
            html.AppendLine(element.GetHtmlText().Replace(element.Placeholder, exam.TimeLimit));

            var newLine = elements.Where(s => s.Type == HtmlElements.TagType.NewLine).FirstOrDefault();
            html.AppendLine(newLine.GetHtmlText());

            foreach (var q in exam.Questions)
            {
                element = elements.Where(s => s.Type == HtmlElements.TagType.Question).FirstOrDefault();
                html.AppendLine(element.GetHtmlText().Replace(element.Placeholder, $"Question {q.Id}"));
                html.AppendLine(newLine.GetHtmlText());

                element = elements.Where(s => s.Type == HtmlElements.TagType.Description).FirstOrDefault();
                foreach (var item in q.Description.Split("\r\n"))
                {
                    html.AppendLine(element.GetHtmlText().Replace(element.Placeholder, item));
                }


                if (q.Images != null)
                    foreach (var item in q.Images)
                    {
                        element = elements.Where(s => s.Type == HtmlElements.TagType.Image).FirstOrDefault();
                        html.AppendLine(element.GetHtmlText().Replace(element.Placeholder, item.Image).Replace(element.Placeholder2, $"{item.Height}").Replace(element.Placeholder3, $"{item.Width}"));
                    }

                if (q.Answers != null)
                    foreach (var a in q.Answers)
                    {
                        element = elements.Where(s => s.Type == HtmlElements.TagType.Answer).FirstOrDefault();
                        html.AppendLine(element.GetHtmlText().Replace(element.Placeholder, $"{a.Letter} - {a.Description}"));
                    }

                element = elements.Where(s => s.Type == HtmlElements.TagType.Correct).FirstOrDefault();
                html.AppendLine(element.GetHtmlText().Replace(element.Placeholder, string.Join(" - ", q.Correct.ToArray())));

                if (q.Section != null)
                {
                    element = elements.Where(s => s.Type == HtmlElements.TagType.Section).FirstOrDefault();
                    foreach (var item in q.Section.Split("\r\n"))
                    {

                        html.AppendLine(element.GetHtmlText().Replace(element.Placeholder, item));
                    }
                }

            }

            return html.ToString();
        }

        public Question BuildQuestionHtml(Questions question)
        {
            var elements = BuildHtmlElements();

            var result = new Question();
            var questionHtml = new StringBuilder();
            var openDiv = $"<div style={(char)34}max-width:330px;max-height:500px;{(char)34}>";
            //var size = $"<font size={(char)34}1{(char)34}>";
            questionHtml.AppendLine(openDiv);
            //questionHtml.AppendLine(size);
            var answers = new StringBuilder();
            answers.AppendLine(openDiv);
            //answers.AppendLine(size);

            var element = elements.Where(s => s.Type == HtmlElements.TagType.Question).FirstOrDefault();
            questionHtml.AppendLine(element.GetHtmlText().Replace(element.Placeholder, $"Question {question.Id}"));
            var newLine = elements.Where(s => s.Type == HtmlElements.TagType.NewLine).FirstOrDefault();

            element = elements.Where(s => s.Type == HtmlElements.TagType.Description).FirstOrDefault();
            foreach (var item in question.Description.Split("\r\n"))
            {
                questionHtml.AppendLine(element.GetHtmlText().Replace(element.Placeholder, item));
            }

            if (question.Images != null)
                foreach (var item in question.Images)
                {
                    element = elements.Where(s => s.Type == HtmlElements.TagType.Image).FirstOrDefault();
                    questionHtml.AppendLine(element.GetHtmlText().Replace(element.Placeholder, item.Image).Replace(element.Placeholder2, $"{item.Height}").Replace(element.Placeholder3, $"{item.Width}"));
                }

            if (question.Answers != null)
                foreach (var a in question.Answers)
                {
                    element = elements.Where(s => s.Type == HtmlElements.TagType.Answer).FirstOrDefault();
                    questionHtml.AppendLine(element.GetHtmlText().Replace(element.Placeholder, $"{a.Letter} - {a.Description}"));
                }

            element = elements.Where(s => s.Type == HtmlElements.TagType.Correct).FirstOrDefault();
            answers.AppendLine(element.GetHtmlText().Replace(element.Placeholder, string.Join(" - ", question.Correct.ToArray())));

            if (question.Section != null)
            {
                element = elements.Where(s => s.Type == HtmlElements.TagType.Section).FirstOrDefault();
                foreach (var item in question.Section.Split("\r\n"))
                {

                    answers.AppendLine(element.GetHtmlText().Replace(element.Placeholder, item));
                }
            }

            questionHtml.AppendLine("</div>");
            answers.AppendLine("</div>");
            //questionHtml.AppendLine("</font>");
            //answers.AppendLine("</font>");

            result.Id = question.Id;
            result.ImageFront = BuildImageToBase64(questionHtml.ToString());
            result.ImageBack = BuildImageToBase64(answers.ToString());

            return result;
        }

        private string BuildImageToBase64(string html)
        {
            var converter = new HtmlConverter();

            return $"{Convert.ToBase64String(converter.FromHtmlString(html, 330, ImageFormat.Png, 100))}";
        }

        private List<HtmlElements> BuildHtmlElements()
        {
            var result = new List<HtmlElements>();

            var htmlElement = new HtmlElements
            {
                Type = HtmlElements.TagType.FileName,
                Placeholder = "|FILENAME|",
                Html = "PHAgc3R5bGU9InRleHQtYWxpZ246Y2VudGVyIj48c3Ryb25nPkZpbGUgTmFtZTo8L3N0cm9uZz4mbmJzcDt8RklMRU5BTUV8PC9wPg=="
            };
            result.Add(htmlElement);

            htmlElement = new HtmlElements
            {
                Type = HtmlElements.TagType.Name,
                Placeholder = "|NAME|",
                Html = "PHAgc3R5bGU9InRleHQtYWxpZ246Y2VudGVyIj48c3Ryb25nPk5hbWU6PC9zdHJvbmc+IHxOQU1FfDxiciAvPg=="
            };
            result.Add(htmlElement);

            htmlElement = new HtmlElements
            {
                Type = HtmlElements.TagType.Score,
                Placeholder = "|SCORE|",
                Html = "PHN0cm9uZz5QYXNzaW5nIFNjb3JlOjwvc3Ryb25nPiB8U0NPUkV8PGJyIC8+"
            };
            result.Add(htmlElement);

            htmlElement = new HtmlElements
            {
                Type = HtmlElements.TagType.Time,
                Placeholder = "|TIME|",
                Html = "PHN0cm9uZz5UaW1lIExpbWl0OiA8L3N0cm9uZz58VElNRXw8L3A+"
            };
            result.Add(htmlElement);

            htmlElement = new HtmlElements
            {
                Type = HtmlElements.TagType.NewLine,
                Placeholder = "",
                Html = "PHAgc3R5bGU9InRleHQtYWxpZ246Y2VudGVyIj4mbmJzcDs8L3A+"
            };
            result.Add(htmlElement);

            htmlElement = new HtmlElements
            {
                Type = HtmlElements.TagType.Question,
                Placeholder = "|QUESTION|",
                Html = "PHAgc3R5bGU9InRleHQtYWxpZ246Y2VudGVyIj48c3Ryb25nPnxRVUVTVElPTnw8L3N0cm9uZz48L3A+"
            };
            result.Add(htmlElement);

            htmlElement = new HtmlElements
            {
                Type = HtmlElements.TagType.Image,
                Placeholder = "|IMGURL|",
                Placeholder2 = "|HEIGHT|",
                Placeholder3 = "|WIDTH|",
                Html = "PHAgc3R5bGU9InRleHQtYWxpZ246Y2VudGVyIj48c3Ryb25nPjxpbWcgYWx0PSIiIHNyYz0ifElNR1VSTHwiIHN0eWxlPSJoZWlnaHQ6fEhFSUdIVHxweDsgd2lkdGg6fFdJRFRIfHB4IiAvPjwvc3Ryb25nPjwvcD4="
            };
            result.Add(htmlElement);

            htmlElement = new HtmlElements
            {
                Type = HtmlElements.TagType.Description,
                Placeholder = "|DESCRIPTION|",
                Html = "PHAgc3R5bGU9InRleHQtYWxpZ246Y2VudGVyIj58REVTQ1JJUFRJT058PC9wPg=="
            };
            result.Add(htmlElement);

            htmlElement = new HtmlElements
            {
                Type = HtmlElements.TagType.Answer,
                Placeholder = "|ANSWER|",
                Html = "PHAgc3R5bGU9InRleHQtYWxpZ246Y2VudGVyIj58QU5TV0VSfDwvcD4="
            };
            result.Add(htmlElement);

            htmlElement = new HtmlElements
            {
                Type = HtmlElements.TagType.Correct,
                Placeholder = "|CORRECT|",
                Html = "PHAgc3R5bGU9InRleHQtYWxpZ246Y2VudGVyIj5Db3JyZWN0IEFuc3dlcjogfENPUlJFQ1R8PC9wPg=="
            };
            result.Add(htmlElement);

            htmlElement = new HtmlElements
            {
                Type = HtmlElements.TagType.Section,
                Placeholder = "|SECTION|",
                Html = "PHAgc3R5bGU9InRleHQtYWxpZ246Y2VudGVyIj58U0VDVElPTnw8L3A+"
            };
            result.Add(htmlElement);

            return result;
        }

        //private string BuildImage(string html)
        //{
        //    var converter = new HtmlConverter();
        //    var d = DateTime.Now;
        //    var bytes = converter.FromHtmlString(html, 330, ImageFormat.Png, 100);
        //    Guid guid = Guid.NewGuid();
        //    Directory.CreateDirectory(Path.Combine(outputFolder, "images"));
        //    File.WriteAllBytes(Path.Combine(Path.Combine(outputFolder, "images"), $"{guid}.png"), bytes);

        //    //return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        //    return $"../../assets/images/{guid}.png";
        //}
    }
}
