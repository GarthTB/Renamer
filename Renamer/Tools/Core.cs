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

        private static List<string> GetNewNamesA(List<FileInfo> origin)
        {
            var newNames = Enumerable.Repeat(string.Empty, origin.Count).ToList();
            var Convert = Dele.ConvName(pattern, DateTime.Now);
            var n = int.Parse(firstN);
            var format = $"D{firstN.Length}";
            _ = Parallel.For(0, origin.Count, i
                => newNames[i] = Convert(origin[i], (n + i).ToString(format)));
            return newNames;
        }

        #endregion

        #region 查找替换

        /// <summary> 按查找替换来执行重命名 </summary>
        public static void RunB() => Run(GetNewNamesB);

        private static List<string> GetNewNamesB(List<FileInfo> origin)
        {
            var newNames = Enumerable.Repeat(string.Empty, origin.Count).ToList();
            static string Convert(string name)
                => useReg
                    ? Regex.Replace(name, find, replace)
                    : name.Replace(find, replace);
            _ = Parallel.For(0, origin.Count, i
                => newNames[i] = Convert(File.TrimName(origin[i])));
            return newNames;
        }

        #endregion

        #region 共同算法

        private static void Run(Func<List<FileInfo>, List<string>> getNewNames)
        {
            try
            {
                var oldFiles = GetOldFiles();
                var newNames = getNewNames(oldFiles);

                // 正则表达式没匹配到的会在这步被筛除
                var shitNum = File.FiltNames(oldFiles, newNames);
                if (shitNum == -1)
                    MsgB.Ok("旧文件名全部符合或新文件名全不可用，未进行任何修改。", "提示");
                else Rename(oldFiles, newNames, shitNum);
            }
            catch (Exception e)
            {
                MsgB.OkErr($"出错：{e.Message}", "错误");
            }
        }

        private static List<FileInfo> GetOldFiles()
        {
            var files = filter
                ? Directory.GetFiles(path).Select(f => new FileInfo(f)).Where(f => f.Extension.Equals(filt_ext, StringComparison.CurrentCultureIgnoreCase))
                : Directory.GetFiles(path).Select(f => new FileInfo(f));
            var Sort = Dele.SortBy(ruleID, reverse);
            var oldFiles = Sort(files).ToList();
            return oldFiles;
        }

        private static void Rename(List<FileInfo> oldFiles, List<string> newNames, int shitNum)
        {
            int total = 0;
            _ = Parallel.For(0, oldFiles.Count, i =>
            {
                string newName = Path.Combine(path, $"{newNames[i]}{oldFiles[i].Extension}");
                if (System.IO.File.Exists(newName))
                    return;
                oldFiles[i].MoveTo(newName);
                _ = Interlocked.Increment(ref total);
            });
            MsgB.Ok($"成功重命名{total}个文件，\n"
                    + $"忽略{shitNum}个名称为空或无需修改的文件，\n"
                    + $"跳过{oldFiles.Count - total}个名称被占的文件。", "提示");
        }

        #endregion
    }
}
