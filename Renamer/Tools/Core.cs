using System.IO;
using System.Text.RegularExpressions;

namespace Renamer.Tools
{
    internal static class Core
    {
        #region 配置和变量

        public static string path = string.Empty;
        public static string pattern = string.Empty;
        public static string firstN = string.Empty;
        public static string find = string.Empty;
        public static string replace = string.Empty;
        public static string filt_ext = string.Empty;
        public static int ruleID = 0;
        public static bool reverse = false, filter = false, useReg = false;

        public static bool CheckA()
            => !string.IsNullOrEmpty(path)
               && !string.IsNullOrEmpty(pattern)
               && !string.IsNullOrEmpty(firstN);

        public static bool CheckB()
            => !string.IsNullOrEmpty(path)
               && !string.IsNullOrEmpty(find);

        public static void Reset()
        {
            path = pattern = firstN = find = replace = filt_ext = string.Empty;
            ruleID = 0;
            reverse = filter = useReg = false;
        }

        #endregion

        #region 模式递推

        /// <summary> 按模式递推来执行重命名 </summary>
        public static void RunA() => Run(GetNewNamesA);

        private static string[] GetNewNamesA(FileInfo[] origin)
        {
            var newNames = new string[origin.Length];
            var Convert = Dele.ConvName(pattern, DateTime.Now);
            var n = int.Parse(firstN);
            var format = $"D{firstN.Length}";
            _ = Parallel.For(0, origin.Length, i
                => newNames[i] = Convert(origin[i], (n + i).ToString(format)));
            return newNames;
        }

        #endregion

        #region 查找替换

        /// <summary> 按查找替换来执行重命名 </summary>
        public static void RunB() => Run(GetNewNamesB);

        private static string[] GetNewNamesB(FileInfo[] origin)
        {
            var newNames = new string[origin.Length];
            static string Convert(string name)
                => useReg
                    ? Regex.Replace(name, find, replace)
                    : name.Replace(find, replace);
            _ = Parallel.For(0, origin.Length, i
                => newNames[i] = Convert(File.TrimName(origin[i])));
            return newNames;
        }

        #endregion

        #region 共同算法

        private static void Run(Func<FileInfo[], string[]> getNewNames)
        {
            try
            {
                var oldFiles = GetOldFiles();
                var newNames = getNewNames(oldFiles);
                if (File.NeedRename(oldFiles, newNames))
                    Rename(oldFiles, newNames);
                else MsgB.Ok("文件名已完全符合要求，无需重命名。", "提示");
            }
            catch (Exception e)
            {
                MsgB.OkErr($"出错：{e.Message}", "错误");
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

        private static void Rename(FileInfo[] oldFiles, string[] newNames)
        {
            int total = 0;
            _ = Parallel.For(0, oldFiles.Length, i =>
            {
                string newName = Path.Combine(path, newNames[i] + oldFiles[i].Extension);
                if (string.IsNullOrWhiteSpace(newNames[i])
                    || System.IO.File.Exists(newName))
                    return;
                oldFiles[i].MoveTo(newName);
                _ = Interlocked.Increment(ref total);
            });
            if (total == oldFiles.Length)
                MsgB.Ok($"成功重命名全部{total}个文件。", "提示");
            else MsgB.Ok($"成功重命名{total}个文件，跳过{oldFiles.Length - total}个名称被占或新名为空的文件。", "提示");
        }

        #endregion
    }
}
