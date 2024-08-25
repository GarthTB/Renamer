namespace Renamer.Tools
{
    internal static class Text
    {
        public static bool IsNatNum(string input)
            => int.TryParse(input, out int num)
               && num >= 0;

        public static bool PMatchesN(string pattern, string number)
        {
            var holder = new string('?', number.Length);
            int index1 = pattern.IndexOf(holder);
            if (index1 == -1)
                return false;
            int index2 = pattern.IndexOf(holder, index1 + 1);
            return index2 == -1;
        }

        public static string TestName(string pattern)
            => pattern.Replace('?', '0')
                      .Replace('*', 'A')
                      .Replace(@"\Y", "2024")
                      .Replace(@"\M", "08")
                      .Replace(@"\D", "26")
                      .Replace(@"\h", "00")
                      .Replace(@"\m", "28")
                      .Replace(@"\s", "45");
    }
}
