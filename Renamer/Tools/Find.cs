namespace Renamer.Tools
{
    internal static class Find
    {
        public static (string pattern, string firstN) Pattern(string path, int ruleID, bool reverse)
        {
            var files = File.AllWithDigits(path);
            if (!files.Any())
                return (string.Empty, string.Empty);

            var Sort = Dele.SortBy(ruleID, reverse);
            var firstFile = Sort(files).First();

            return File.GetPattern(firstFile);
        }

        /// <summary> 获取文件夹中的所有扩展名 </summary>
        /// <returns> 有序扩展名列表 </returns>
        public static List<string> Extensions(string path)
        {
            var extensions = File.GetExtensions(path).Distinct().ToList();
            extensions.Sort();
            return extensions;
        }
    }
}
