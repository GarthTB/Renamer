using System.IO;

namespace Renamer.Tools
{
    internal static class Core
    {
        public static string path = string.Empty;
        public static string pattern = string.Empty;
        public static string firstN = string.Empty;
        public static string find = string.Empty;
        public static string replace = string.Empty;
        public static string filt_ext = string.Empty;
        public static int ruleID = 0;
        public static bool reverse = false, filter = false;

        public static bool CheckA()
            => !string.IsNullOrEmpty(path)
               && !string.IsNullOrEmpty(pattern)
               && !string.IsNullOrEmpty(firstN);

        public static bool CheckB()
            => !string.IsNullOrEmpty(path)
               && !string.IsNullOrEmpty(find)
               && !string.IsNullOrEmpty(replace);

        public static void RunA()
        {
            try
            {
                var oldFiles = GetOldFiles();
                var newNames = GetNewNames(oldFiles);
                if (File.NeedRename(oldFiles, newNames))
                {
                    string Method(FileInfo f, int i)
                        => Path.Combine(path, newNames[i] + f.Extension);
                    Rename(oldFiles, Method);
                }
                else MsgB.Ok("文件名已完全符合要求，无需重命名。", "提示");
            }
            catch (Exception e)
            {
                MsgB.OkErr(e.Message, "错误");
            }
        }

        private static FileInfo[] GetOldFiles()
        {
            var files = filter
                ? Directory.GetFiles(path).Select(f => new FileInfo(f)).Where(f => f.Extension.Equals(filt_ext, StringComparison.CurrentCultureIgnoreCase))
                : Directory.GetFiles(path).Select(f => new FileInfo(f));
            var Sort = Dele.SortBy(ruleID, reverse);
            var oldFiles = Sort(files).ToArray();
            return oldFiles;
        }

        private static string[] GetNewNames(FileInfo[] origin)
        {
            var newNames = new string[origin.Length];
            var Convert = Dele.ConvName(pattern, DateTime.Now);
            var n = int.Parse(firstN);
            var format = $"D{firstN.Length}";
            _ = Parallel.For(0, origin.Length, i
                => newNames[i] = Convert(origin[i], (n + i).ToString(format)));
            return newNames;
        }

        private static void Rename(FileInfo[] oldFiles, Func<FileInfo, int, string> method)
        {
            int total = 0;
            _ = Parallel.For(0, oldFiles.Length, i =>
            {
                string newName = method(oldFiles[i], i);
                if (System.IO.File.Exists(newName))
                    return;
                oldFiles[i].MoveTo(newName);
                _ = Interlocked.Increment(ref total);
            });
            if (total == oldFiles.Length)
                MsgB.Ok($"成功重命名全部{total}个文件。", "提示");
            else MsgB.Ok($"成功重命名{total}个文件，跳过{oldFiles.Length - total}个名称被占的文件。", "提示");
        }

        public static void RunB()
        {

        }

        public static void Reset()
        {
            path = pattern = firstN = find = replace = filt_ext = string.Empty;
            ruleID = 0;
            reverse = filter = false;
        }
    }
}
