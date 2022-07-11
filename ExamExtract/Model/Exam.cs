namespace ExamExtract.Model
{
    public class Exam
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string PassingScore { get; set; }
        public string TimeLimit { get; set; }
        public List<Questions>? Questions { get; set; }
    }

    public class Questions
    {
        public int? Id { get; set; }
        public string? Description { get; set; }        
        public List<string>? Correct { get; set; }
        public string? Section { get; set; }
        public List<Answers>? Answers { get; set; }
        public List<ImageFull>? Images { get; set; }
    }

    public class Answers
    {
        public int? Id { get; set; }
        public string? Description { get; set; }
        public string? Letter { get; set; }        
    }

    public class ImageFull
    {
        public string Image { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
