using System.IO;

namespace Renamer.Tools
{
    internal static class Dele
    {
        /// <summary> 将抽象文件名转换为实际文件名 </summary>
        public static Func<FileInfo, string, string> ConvName(string pattern, DateTime t)
            => (file, num)
            => pattern.Replace(new string('?', num.Length), num)
                      .Replace("*", File.TrimName(file))
                      .Replace(@"\Y", t.Year.ToString("D4"))
                      .Replace(@"\M", t.Month.ToString("D2"))
                      .Replace(@"\D", t.Day.ToString("D2"))
                      .Replace(@"\h", t.Hour.ToString("D2"))
                      .Replace(@"\m", t.Minute.ToString("D2"))
                      .Replace(@"\s", t.Second.ToString("D2"));

        /// <summary> 按指定规则和方向对文件排序 </summary>
        public static Func<IEnumerable<FileInfo>, IOrderedEnumerable<FileInfo>> SortBy(int ruleID, bool reverse)
            => reverse
            ? (files => files.OrderByDescending(IDToRule(ruleID)))
            : (files => files.OrderBy(IDToRule(ruleID)));

        public static Func<FileInfo, object> IDToRule(int ruleID)
        {
            return ruleID switch
            {
                0 => f => f.CreationTime,
                1 => f => f.LastWriteTime,
                _ => f => f.Length,
            };
        }
    }
}
