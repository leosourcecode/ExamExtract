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
            const string pdfPath = @"C:\Users\LeandroCarliniMingor\Downloads\gratisexam.com-Microsoft.test4prep.AI-900.v2020-09-07.by.abdullah.25q.pdf";

            // Path to the Output folder
            const string outputFolderPath = @"C:\Output";

            // Path to the merged files folder
            const string mergedFilesPath = @"C:\Output\merge";

            // Operation
            const FlowEnum flow = FlowEnum.MergeExam;

            /*
             * Setup
            */

            Console.WriteLine("Starting ......");
            Console.WriteLine($"Type of flow: {flow.ToString()}");

            var folderService = new FolderService();
            var pdfService = new PdfService();
            var htmlService = new HtmlService();
            var examService = new ExamService();

            var fileName = Path.GetFileNameWithoutExtension(pdfPath);
            var outputFolder = Path.Combine(outputFolderPath, fileName);

            switch (flow)
            {
                case FlowEnum.BuildExam:
                    Console.WriteLine($"File Name: {fileName}");
                    Console.WriteLine($"Output Folder: {outputFolder}");
                    Console.WriteLine("Creating folders ......");
                    folderService.CreateFolderDeleteIfExists(outputFolder);

                    Console.WriteLine("Loading PDF ......");
                    var pdfContent = pdfService.LoadPdf(pdfPath, outputFolder);

                    Console.WriteLine("Building Exam ......");
                    var examLoad = examService.BuildExam(pdfContent, fileName);

                    Console.WriteLine("Building HTML ......");
                    var htmlLoad = htmlService.BuildHtml(examLoad);

                    Console.WriteLine("Saving data.json ......");
                    var examsLoad = new List<Exam>();
                    examsLoad.Add(examLoad);
                    File.WriteAllText(Path.Combine(outputFolder, $"data.json"), JsonSerializer.Serialize(examsLoad));

                    Console.WriteLine("Saving Full.html ......");
                    File.WriteAllText(Path.Combine(outputFolder, $"Full.html"), htmlLoad);
                    break;
                case FlowEnum.LoadExam:
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
                    break;
                case FlowEnum.MergeExam:

                    var result = new List<ExamData>();
                    string[] fileEntries = Directory.GetFiles(mergedFilesPath);
                    foreach (string file in fileEntries)
                    {
                        var json = File.ReadAllText(file);
                        foreach (var i in JsonSerializer.Deserialize<List<ExamData>>(json))
                        {
                            result.Add(i);
                        }
                    }
                    File.WriteAllText(Path.Combine(mergedFilesPath, $"examData.json"), JsonSerializer.Serialize(result));
                    break;
                default:
                    break;
            }

            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}
