using System.Text.Json;
using ExamExtract.Services;
using ExamExtract.Model;
using System.Text;

namespace ExamExtract
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /*
             * Setup
            */

            // Path to the PDF
            const string pdfPath = @"C:\Users\LeandroCarliniMingor\Downloads\gratisexam.com-Microsoft.braindumps.AI-900.v2021-04-29.by.mohammed.47q.pdf";

            // Path to the Output folder
            const string outputFolderPath = @"C:\Output";

            // Operation
            const bool isLoad = false;

            /*
             * Setup
            */

            Console.WriteLine("Starting ......");
            Console.WriteLine($"Is a Load from JSON? {isLoad}");

            var folderService = new FolderService();
            var pdfService = new PdfService();
            var htmlService = new HtmlService();
            var examService = new ExamService();

            var fileName = Path.GetFileNameWithoutExtension(pdfPath);
            var outputFolder = Path.Combine(outputFolderPath, fileName);

            if (isLoad)
            {
                Console.WriteLine("Loading Data ......");
                var jsonData = File.ReadAllText(Path.Combine(outputFolder, $"data.json"));
                var exams = JsonSerializer.Deserialize<List<Exam>>(jsonData);

                Console.WriteLine("Building ExamData ......");
                var examDatas = new List<ExamData>();
                foreach (var exam in exams)
                {
                    var examData = examService.BuildExamData(exam);
                    examDatas.Add(examData);
                }              

                Console.WriteLine("Saving examData.json ......");
                File.WriteAllText(Path.Combine(outputFolder, $"examData.json"), JsonSerializer.Serialize(examDatas));

                Console.WriteLine("Building HTML ......");
                var html = new StringBuilder();
                foreach (var exam in exams)
                {
                    html.AppendLine(htmlService.BuildHtml(exam));
                    html.AppendLine();
                    html.AppendLine();
                    html.AppendLine();
                    html.AppendLine("-------------------------");
                }                

                Console.WriteLine("Saving Final.html ......");
                File.WriteAllText(Path.Combine(outputFolder, $"Final.html"), html.ToString());
            }
            else
            {               
                Console.WriteLine($"File Name: {fileName}");
                Console.WriteLine($"Output Folder: {outputFolder}");
                Console.WriteLine("Creating folders ......");
                folderService.CreateFolderDeleteIfExists(outputFolder);

                Console.WriteLine("Loading PDF ......");
                var pdfContent = pdfService.LoadPdf(pdfPath, outputFolder);

                Console.WriteLine("Building Exam ......");
                var exam = examService.BuildExam(pdfContent, fileName);

                Console.WriteLine("Building HTML ......");
                var html = htmlService.BuildHtml(exam);

                Console.WriteLine("Saving data.json ......");
                var exams = new List<Exam>();
                exams.Add(exam);
                File.WriteAllText(Path.Combine(outputFolder, $"data.json"), JsonSerializer.Serialize(exams));

                Console.WriteLine("Saving Full.html ......");
                File.WriteAllText(Path.Combine(outputFolder, $"Full.html"), html);
            }

            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}
