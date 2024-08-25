using Renamer.Properties;
using Renamer.Tools;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Renamer
{
    /// <summary> Interaction logic for MainWindow.xaml </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();

        #region 基础逻辑

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Reset();
            if (Settings.Default.FirstStart)
            {
                Info.Welc();
                Settings.Default.FirstStart = false;
                Settings.Default.Save();
            }
        }

        /// <summary> 快捷键 </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                    Info.Help();
                    break;
                case Key.F5:
                    Reset();
                    break;
            }
        }

        /// <summary> 重置所有控件以及核心 </summary>
        private void Reset()
        {
            Core.Reset();

            Tx_Path_A.Clear();
            Tx_Pattern.Clear();
            Tx_FirstN.Clear();
            Cb_Rule.SelectedIndex = 0;
            Ch_Reverse.IsChecked = false;
            Ch_Filter_A.IsChecked = false;
            Bt_Run_A.IsEnabled = false;

            // Tx_Path_B.Clear();
            Tx_Find.Clear();
            Tx_Replace.Clear();
            Ch_Regex.IsChecked = false;
            // Ch_Filter_B.IsChecked = false;
            // Bt_Run_B.IsEnabled = false;

            TI_Filter.IsEnabled = false;
            Cb_Extensions.ItemsSource = null;
            Cb_Extensions.SelectedItem = null;
            // Cb_Extensions.SelectedIndex = -1;
        }

        #endregion

        #region 拖放文件

        private void Window_DragEnter(object sender, DragEventArgs e)
            => e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                string filePath = files[0];
                string folderPath = Path.GetDirectoryName(filePath) ?? string.Empty;
                Tx_Path_A.Text = folderPath;
            }
        }

        #endregion

        #region 文件夹路径处理

        private void Tx_Path_A_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tx_Path_B.Text = Tx_Path_A.Text;
            PathChanged(Tx_Path_A.Text);
        }

        private void Tx_Path_B_TextChanged(object sender, TextChangedEventArgs e)
            => Tx_Path_A.Text = Tx_Path_B.Text;

        private void PathChanged(string path)
        {
            if (!string.IsNullOrWhiteSpace(path)
                && Tools.File.PathValid(path))
            {
                Core.path = path;
                Cb_Extensions.ItemsSource = Find.Extensions(path);
                Cb_Extensions.SelectedIndex = 0;
                if (string.IsNullOrWhiteSpace(Tx_Pattern.Text)
                    && string.IsNullOrWhiteSpace(Tx_FirstN.Text))
                    (Tx_Pattern.Text, Tx_FirstN.Text) = Find.Pattern(
                        path, Core.ruleID, Core.reverse);
                else Bt_Run_A.IsEnabled = Core.CheckA();
            }
            else
            {
                Core.path = string.Empty;
                Cb_Extensions.ItemsSource = null;
                Cb_Extensions.SelectedItem = null;
                Bt_Run_A.IsEnabled = false;
            }
        }

        #endregion

        #region 模式和起始编号

        private void Tx_Pattern_TextChanged(object sender, TextChangedEventArgs e)
            => PatternChanged(Tx_Pattern.Text, Tx_FirstN.Text);

        private void Tx_FirstN_TextChanged(object sender, TextChangedEventArgs e)
            => PatternChanged(Tx_Pattern.Text, Tx_FirstN.Text);

        private void PatternChanged(string pattern, string firstN)
        {
            if (!string.IsNullOrWhiteSpace(pattern)
                && !string.IsNullOrWhiteSpace(firstN)
                && Tools.File.NameValid(Text.TestName(pattern))
                && Text.IsNatNum(firstN)
                && Text.PMatchesN(pattern, firstN))
            {
                Core.pattern = pattern;
                Core.firstN = firstN;
                Bt_Run_A.IsEnabled = Core.CheckA();
            }
            else
            {
                Core.pattern = Core.firstN = string.Empty;
                Bt_Run_A.IsEnabled = false;
            }
        }

        #endregion

        #region 模式递推其余配置

        private void Cb_Rule_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => Core.ruleID = Cb_Rule.SelectedIndex;

        private void Ch_Reverse_Checked(object sender, RoutedEventArgs e)
            => Core.reverse = true;

        private void Ch_Reverse_Unchecked(object sender, RoutedEventArgs e)
            => Core.reverse = false;

        #endregion

        #region 过滤器

        private void Ch_Filter_A_Checked(object sender, RoutedEventArgs e)
        {
            Ch_Filter_B.IsChecked = true;
            TI_Filter.IsEnabled = true;
            Core.filter = true;
        }

        private void Ch_Filter_B_Checked(object sender, RoutedEventArgs e)
            => Ch_Filter_A.IsChecked = true;

        private void Ch_Filter_A_Unchecked(object sender, RoutedEventArgs e)
        {
            Ch_Filter_B.IsChecked = false;
            TI_Filter.IsEnabled = false;
            Core.filter = false;
        }

        private void Ch_Filter_B_Unchecked(object sender, RoutedEventArgs e)
            => Ch_Filter_A.IsChecked = false;

        private void Cb_Extensions_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => Core.filt_ext = Cb_Extensions.SelectedItem as string ?? string.Empty;

        #endregion

        #region 执行模式递推的重命名

        private void Bt_Run_A_Click(object sender, RoutedEventArgs e)
        {
            if (Tools.File.InvSuff(Core.path, Core.firstN))
                Core.RunA();
            else MsgB.Ok("文件编号余量不足，无法继续！", "提示");
        }

        #endregion
    }
}
