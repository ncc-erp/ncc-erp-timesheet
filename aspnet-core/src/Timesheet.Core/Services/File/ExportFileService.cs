using Abp.UI;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;

namespace Timesheet.Services.File
{
    public class ExportFileService
    {
        private readonly ILogger<ExportFileService> logger;

        public ExportFileService(ILogger<ExportFileService> logger)
        {
            this.logger = logger;
        }

        public async System.Threading.Tasks.Task<MemoryStream> ExportFileWord(string fullPath, Dictionary<string, string> bookmarks, string saveAsToPath)
        {
            Application app = new Application();
            Document doc = new Document();
            object objectPath = fullPath;
            object miss = System.Reflection.Missing.Value;
            try
            {
                doc = app.Documents.Open(ref objectPath, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss,
                ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss);
                foreach (var bookmark in bookmarks)
                {
                    if (doc.Bookmarks.Exists(bookmark.Key))
                    {
                        try
                        {
                            Bookmark bm = doc.Bookmarks[bookmark.Key];
                            Range range = bm.Range;
                            range.Text = bookmark.Value;
                            doc.Bookmarks.Add(bookmark.Key, range);
                        }
                        catch (Exception)
                        {
                            throw new UserFriendlyException(string.Format("This file cannot be overwritten."));
                        }
                    }
                }
                doc.SaveAs2(FileName: saveAsToPath);
            }
            catch (System.Exception)
            {
                throw new UserFriendlyException(string.Format("This file cannot be export."));
            }
            finally
            {
                doc.Close();
                app.Quit();
            }
            var memoryStream = new MemoryStream();

            using (var stream = new FileStream(saveAsToPath, FileMode.Open))
            {
                await stream.CopyToAsync(memoryStream);
            }
            memoryStream.Position = 0;
            logger.LogInformation($"Export file to path {saveAsToPath}");
            return memoryStream;
        }
    }
}