using System.Windows;

namespace Renamer.Tools
{
    /// <summary> 为简化代码，将MessageBox封装成一个类 </summary>
    internal static class MsgB
    {
        public static void Ok(string info, string title)
            => _ = MessageBox.Show(info, title, MessageBoxButton.OK);

        public static void OkErr(string info, string title)
            => _ = MessageBox.Show(info, title, MessageBoxButton.OK, MessageBoxImage.Error);

        public static bool YesNo(string info, string title)
            => MessageBox.Show(info, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
    }
}
