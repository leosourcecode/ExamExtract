using UglyToad.PdfPig.Content;
using System.Text;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using UglyToad.PdfPig.XObjects;
using ExamExtract.Model;

namespace ExamExtract.Services
{
    public class PdfService
    {
        public PdfContent LoadPdf(string pdfPath, string outputFolder)
        {
            var result = new PdfContent
            {
                Images = new List<Image>(),
                Pages = new Dictionary<int, string>()
            };

            using (var stream = File.OpenRead(pdfPath))
            using (UglyToad.PdfPig.PdfDocument document = UglyToad.PdfPig.PdfDocument.Open(stream))
            {
                var countImages = 0;
                foreach (var page in document.GetPages())
                {
                    result.Pages.Add(page.Number, AdvancedTextExtraction(page));

                    IEnumerable<IPdfImage> pdfImages = page.GetImages();

                    foreach (IPdfImage image in pdfImages)
                    {
                        if (!image.TryGetBytes(out var b))
                        {
                            b = image.RawBytes;
                        }

                        var type = string.Empty;
                        switch (image)
                        {
                            case XObjectImage ximg:
                                type = "XObject";
                                break;
                            case InlineImage inline:
                                type = "Inline";
                                break;
                        }

                        if (type == "XObject" && b.Count != 5617)
                        {
                            countImages++;
                            byte[] bitmap = image.RawBytes.ToArray();
                            var path = Path.Combine(outputFolder, $"{page.Number}_{countImages}.jpg");
                            File.WriteAllBytes(path, bitmap);
                            result.Images.Add(new Image { Path = path, Page = page.Number, Width = image.WidthInSamples , Height = image.HeightInSamples });
                        }
                    }
                }
            }            

            return result;
        }

        private string AdvancedTextExtraction(Page page)
        {
            var sb = new StringBuilder();

            // 0. Preprocessing
            var letters = page.Letters; // no preprocessing

            // 1. Extract words
            var wordExtractor = NearestNeighbourWordExtractor.Instance;
            var wordExtractorOptions = new NearestNeighbourWordExtractor.NearestNeighbourWordExtractorOptions()
            {
                Filter = (pivot, candidate) =>
                {
                    // check if white space (default implementation of 'Filter')
                    if (string.IsNullOrWhiteSpace(candidate.Value))
                    {
                        // pivot and candidate letters cannot belong to the same word 
                        // if candidate letter is null or white space.
                        // ('FilterPivot' already checks if the pivot is null or white space by default)
                        return false;
                    }

                    // check for height difference
                    var maxHeight = Math.Max(pivot.PointSize, candidate.PointSize);
                    var minHeight = Math.Min(pivot.PointSize, candidate.PointSize);
                    if (minHeight != 0 && maxHeight / minHeight > 2.0)
                    {
                        // pivot and candidate letters cannot belong to the same word 
                        // if one letter is more than twice the size of the other.
                        return false;
                    }

                    // check for colour difference
                    var pivotRgb = pivot.Color.ToRGBValues();
                    var candidateRgb = candidate.Color.ToRGBValues();
                    if (!pivotRgb.Equals(candidateRgb))
                    {
                        // pivot and candidate letters cannot belong to the same word 
                        // if they don't have the same colour.
                        return false;
                    }

                    return true;
                }
            };

            var words = wordExtractor.GetWords(letters);//, wordExtractorOptions);

            // 2. Segment page
            var pageSegmenter = DocstrumBoundingBoxes.Instance;
            var pageSegmenterOptions = new DocstrumBoundingBoxes.DocstrumBoundingBoxesOptions()
            {

            };

            var textBlocks = pageSegmenter.GetBlocks(words);//, pageSegmenterOptions);

            // 3. Postprocessing
            var readingOrder = UnsupervisedReadingOrderDetector.Instance;
            var orderedTextBlocks = readingOrder.Get(textBlocks);

            // 4. Extract text
            foreach (var block in orderedTextBlocks)
            {
                sb.Append(block.Text.Normalize(NormalizationForm.FormKC)); // normalise text
                sb.AppendLine();
            }

            sb.AppendLine();

            return sb.ToString();
        }

        //public static void ExtractTextWithNewlines(string filePath)
        //{
        //    using (var document = PdfDocument.Open(filePath))
        //    {
        //        foreach (var page in document.GetPages())
        //        {
        //            var text = ContentOrderTextExtractor.GetText(page, true);

        //            Console.WriteLine(text);
        //        }
        //    }
        //}

        //public static string AdvancedTextExtraction(string filePath)
        //{
        //    var sb = new StringBuilder();

        //    using (var document = PdfDocument.Open(filePath))
        //    {
        //        foreach (var page in document.GetPages())
        //        {
        //            // 0. Preprocessing
        //            var letters = page.Letters; // no preprocessing

        //            // 1. Extract words
        //            var wordExtractor = NearestNeighbourWordExtractor.Instance;
        //            var wordExtractorOptions = new NearestNeighbourWordExtractor.NearestNeighbourWordExtractorOptions()
        //            {
        //                Filter = (pivot, candidate) =>
        //                {
        //                    // check if white space (default implementation of 'Filter')
        //                    if (string.IsNullOrWhiteSpace(candidate.Value))
        //                    {
        //                        // pivot and candidate letters cannot belong to the same word 
        //                        // if candidate letter is null or white space.
        //                        // ('FilterPivot' already checks if the pivot is null or white space by default)
        //                        return false;
        //                    }

        //                    // check for height difference
        //                    var maxHeight = Math.Max(pivot.PointSize, candidate.PointSize);
        //                    var minHeight = Math.Min(pivot.PointSize, candidate.PointSize);
        //                    if (minHeight != 0 && maxHeight / minHeight > 2.0)
        //                    {
        //                        // pivot and candidate letters cannot belong to the same word 
        //                        // if one letter is more than twice the size of the other.
        //                        return false;
        //                    }

        //                    // check for colour difference
        //                    var pivotRgb = pivot.Color.ToRGBValues();
        //                    var candidateRgb = candidate.Color.ToRGBValues();
        //                    if (!pivotRgb.Equals(candidateRgb))
        //                    {
        //                        // pivot and candidate letters cannot belong to the same word 
        //                        // if they don't have the same colour.
        //                        return false;
        //                    }

        //                    return true;
        //                }
        //            };

        //            var words = wordExtractor.GetWords(letters);//, wordExtractorOptions);

        //            // 2. Segment page
        //            var pageSegmenter = DocstrumBoundingBoxes.Instance;
        //            var pageSegmenterOptions = new DocstrumBoundingBoxes.DocstrumBoundingBoxesOptions()
        //            {

        //            };

        //            var textBlocks = pageSegmenter.GetBlocks(words);//, pageSegmenterOptions);

        //            // 3. Postprocessing
        //            var readingOrder = UnsupervisedReadingOrderDetector.Instance;
        //            var orderedTextBlocks = readingOrder.Get(textBlocks);

        //            // 4. Extract text
        //            foreach (var block in orderedTextBlocks)
        //            {
        //                sb.Append(block.Text.Normalize(NormalizationForm.FormKC)); // normalise text
        //                sb.AppendLine();
        //            }

        //            sb.AppendLine();
        //        }
        //    }

        //    return sb.ToString();
        //}
    }
}
