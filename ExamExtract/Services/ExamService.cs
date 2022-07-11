using ExamExtract.Model;
using System.Text;

namespace ExamExtract.Services
{
    public class ExamService
    {
        public ExamData BuildExamData(Exam exam)
        {
            var htmlService = new HtmlService();

            var result = new ExamData();

            result.PassingScore = exam.PassingScore;
            result.FileName = exam.FileName;
            result.Name = exam.Name;
            result.TimeLimit = exam.TimeLimit;

            result.Questions = new List<Question>();

            Console.WriteLine("Generating questions ......");
            var total = exam.Questions.Count;
            var count = 0;
            foreach (var item in exam.Questions)
            {
                count++;
                Console.WriteLine($"Question {count}/{total} ");
                result.Questions.Add(htmlService.BuildQuestionHtml(item));
            }

            return result;
        }

        public Exam BuildExam(PdfContent pdf, string fileName)
        {
            var firstPage = pdf.GetFirstPage();
            var exam = new Exam
            {
                Questions = new List<Questions>()
            };

            exam.FileName = fileName;

            foreach (var line in firstPage.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Contains("Number: "))
                {
                    var list = line.Split('\n');
                    exam.Name = list[0].Replace("Number: ", string.Empty).Trim();
                    exam.PassingScore = list[1].Replace("Passing Score: ", string.Empty).Trim();
                    exam.TimeLimit = list[2].Replace("Time Limit: ", string.Empty).Trim();
                }
            }

            BuildQuestions(ref exam, pdf.ToString());
            AddImages(ref exam, pdf);

            return exam;
        }

        private void AddImages(ref Exam exam, PdfContent pdf)
        {
            foreach (var img in pdf.Images)
            {
                var imgPage = pdf.GetPage(img.Page);
                var breakLoop = false;

                foreach (var item in imgPage.Split("\n"))
                {
                    if (item.Contains("QUESTION "))
                    {
                        int questionId;
                        var successfullyParsed = int.TryParse($"{item.Replace("QUESTION ", string.Empty)}", out questionId);

                        foreach (var q in exam.Questions)
                        {
                            if (q.Id == questionId)
                            {
                                if (q.Images == null)
                                {
                                    q.Images = new List<ImageFull>();
                                }

                                var imageFull = new ImageFull
                                {
                                    Image = img.Path,
                                    Width = img.Width,
                                    Height = img.Height
                                };
                                q.Images.Add(imageFull);
                                breakLoop = true;
                                break;                                
                            }
                        }
                    }

                    if (breakLoop)
                    {
                        break;
                    }
                }
            }
        }

        private void BuildQuestions(ref Exam exam, string fullExam)
        {
            var result = new List<Questions>();

            var question = new Questions
            {
                Answers = new List<Answers>()
            };

            var isAnswerSection = false;
            var section = new StringBuilder();
            var description = new StringBuilder();
            var count = 0;

            var questions = fullExam.Split("QUESTION ");
            foreach (var lineOfQuestions in questions.Skip(1))
            {
                foreach (var line in lineOfQuestions.Split("\n"))
                {
                    if (line.Contains("www.gratisexam.com")) continue;
                    if (line == "885CB989129A5F974833949052CFB2F2" || line == ("885CB989129A5F974833949052CFB2F2" + '\r')) continue;//(line.Length == 33) continue; // 885CB989129A5F974833949052CFB2F2
                    if (string.IsNullOrWhiteSpace(line.Trim())) continue;

                    int questionId;
                    var successfullyParsed = int.TryParse($"{line.Trim()}", out questionId);
                    if (successfullyParsed)
                    {
                        if (question.Answers.Count > 0)
                        {
                            question.Section = section.ToString();
                            result.Add(question);
                            question = new Questions
                            {
                                Answers = new List<Answers>()
                            };
                        }
                        question.Id = questionId;
                        isAnswerSection = false;
                        description = new StringBuilder();
                        section = new StringBuilder();
                        count = 0;
                        continue;
                    }

                    if (line != null && line.Length > 1 && $"{line[0]}{line[1]}" == "A.")
                    {
                        isAnswerSection = true;
                    }

                    if (isAnswerSection == false)
                    {
                        description.AppendLine(line);
                    }

                    if (isAnswerSection)
                    {
                        if (line != null && line.Length > 2 && $"{line[1]}{line[2]}" == ". ")
                        {
                            count++;
                            var answer = new Answers();

                            var newItem = line.Split(". ");
                            answer.Letter = newItem[0].Trim();
                            answer.Description = newItem[1].Trim();
                            answer.Id = count;
                            question.Answers.Add(answer);
                        }
                        else if (line.Contains("Correct Answer:"))
                        {
                            question.Description = description.ToString();
                            var answers = line.Replace("Correct Answer: ", string.Empty).Trim();
                            question.Correct = new List<string>();
                            foreach (char c in answers)
                            {
                                question.Correct.Add(c.ToString());
                            }
                        }
                        else
                        {
                            section.AppendLine(line);
                        }
                    }
                }
            }

            question.Section = section.ToString();
            result.Add(question);

            exam.Questions = result;
        }
    }
}
