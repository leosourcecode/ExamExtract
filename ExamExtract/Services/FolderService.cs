namespace ExamExtract.Services
{
    public class FolderService
    {
        public void CreateFolderDeleteIfExists(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            Directory.CreateDirectory(folderPath);
        }
    }
}
