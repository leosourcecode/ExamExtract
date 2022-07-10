namespace ExamExtract.Model
{
    public class ExamData
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string PassingScore { get; set; }
        public string TimeLimit { get; set; }
        public List<Question>? Questions { get; set; }
    }

    public class Question
    {
        public int? Id { get; set; }
        public string? ImageFront { get; set; }

        public string? ImageBack { get; set; }
    }
}
