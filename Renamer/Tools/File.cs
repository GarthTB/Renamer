using System.IO;
using System.Text.RegularExpressions;

namespace Renamer.Tools
{
    internal static partial class File
    {
        [GeneratedRegex(@"\d+")]
        private static partial Regex Digits();

        public static bool NameValid(string fileName)
            => fileName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;

        public static bool PathValid(string path)
            => Directory.Exists(path)
               && Directory.GetFiles(path).Length > 0;

        /// <summary> 检查编号余量是否足够 </summary>
        public static bool InvSuff(string path, string firstN)
        {
            var amount = Directory.GetFiles(path).Length;
            return int.TryParse(firstN, out int num)
                   && num + amount - 1 <= int.Parse(new string('9', firstN.Length));
        }

        public static IEnumerable<FileInfo> AllWithDigits(string path)
            => Directory.GetFiles(path)
                        .Select(f => new FileInfo(f))
                        .Where(f => Digits().IsMatch(TrimName(f)));

        /// <summary> 参数FileInfo的文件名中必须包含数字 </summary>
        public static (string pattern, string firstN) GetPattern(FileInfo file)
        {
            var name = TrimName(file);
            var matches = Digits().Matches(name);
            var last = matches.Last();
            var holder = new string('?', last.Length);
            var span = name.AsSpan();
            var pattern = string.Concat(
                span[..last.Index], holder, span[(last.Index + last.Length)..]);
            return (pattern, last.Value);
        }

        public static IEnumerable<string> GetExtensions(string path)
        {
            foreach (var f in Directory.GetFiles(path))
                if (!string.IsNullOrEmpty(Path.GetExtension(f)))
                    yield return Path.GetExtension(f).ToLower();
        }

        public static string TrimName(FileInfo file)
            => Path.GetFileNameWithoutExtension(file.Name);

        public static bool NeedRename(FileInfo[] files, string[] names)
        {
            var need = 0;
            _ = Parallel.For(0, files.Length, (i, state) =>
            {
                if (TrimName(files[i]) == names[i])
                    return;
                _ = Interlocked.Exchange(ref need, 1);
                state.Stop();
            });
            return need == 1;
        }
    }
}
