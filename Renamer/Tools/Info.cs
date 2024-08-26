using System.Text;
using System.Windows;

namespace Renamer.Tools
{
    internal static class Info
    {
        /// <summary> 版本号 </summary>
        private const string version = "0.1.2";

        /// <summary> 帮助信息 </summary>
        public static void Help()
        {
            StringBuilder sb = new();
            string msg = sb.AppendLine("快捷键：")
                           .AppendLine("    F1：帮助")
                           .AppendLine("    F5：清空\n")
                           .AppendLine($"重命名工具 v{version}")
                           .AppendLine("Copyright (c) 2024 Garth\n")
                           .AppendLine("是否复制仓库地址？")
                           .ToString();
            if (MsgB.YesNo(msg, "帮助"))
            {
                Clipboard.SetText("https://github.com/GarthTB/Renamer");
                MsgB.Ok("仓库地址已复制到剪贴板。", "提示");
            }
        }

        /// <summary> 首次启动的欢迎信息 </summary>
        public static void Welc()
        {
            StringBuilder sb = new();
            string msg = sb.AppendLine("欢迎使用重命名工具！")
                           .AppendLine("快捷键：")
                           .AppendLine("    F1：帮助")
                           .AppendLine("    F5：清空\n")
                           .AppendLine($"重命名工具 v{version}")
                           .AppendLine("Copyright (c) 2024 Garth")
                           .ToString();
            MsgB.Ok(msg, "欢迎");
        }
    }
}
