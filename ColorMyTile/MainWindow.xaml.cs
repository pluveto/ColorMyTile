using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ColorMyTile
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            
            if(!Functions.IsAdministrator())
            {
                MessageBox.Show("由于创建文件的目录可能需要权限，被本程序需要提权运行。\n代码开源，无任何危险代码，请放心给予权限。");
                RunElevated(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            
        }
        private void RunElevated(string fileName)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = fileName;
            try
            {
                Process.Start(processInfo);
            }
            finally
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
        public class TileProgram
        {
            public string Title { get; set; }
            public string Path { get; set; }
            public ImageSource Icon { get; set; }
        }
        private List<TileProgram> tilePrograms = new List<TileProgram>();
        private void ProgramListBox_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            // 过滤非exe的文件
            foreach (var ele in files)
            {
                AddFile(ele);
            }
            ProgramListBox.ItemsSource = null;
            ProgramListBox.ItemsSource = tilePrograms;


        }
        private void AddFile(string ele)
        {
            string path = "";
            if (ele.ToLower().EndsWith(".lnk"))
            {
                path = Functions.GetShortcutTarget(ele);
            }
            if (path == string.Empty) path = ele;
            if (!path.ToLower().EndsWith(".exe")) return;
            tilePrograms.Add(new TileProgram()
            {
                Icon = Functions.IconToImageSource(System.Drawing.Icon.ExtractAssociatedIcon(path)),
                Path = path,
                Title = System.IO.Path.GetFileNameWithoutExtension(path)
            });
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var BackgroundColor = BGColorPicker.SelectedColor;
            if (BackgroundColor == null) return;
            if (LightTextCheckBox.IsChecked == null) return;


            

            var ForegroundText = (bool)LightTextCheckBox.IsChecked ? "light" : "dark";

            foreach (var app in ProgramListBox.SelectedItems)
            {
                var tileProgram = (TileProgram)app;
                var dir = System.IO.Path.GetDirectoryName(tileProgram.Path);
                var outputFileName = string.Format("{0}/{1}.visualelementsmanifest.xml", dir, tileProgram.Title);
                var outputXml = string.Format(
                    "<Application xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\n" +
                    " <VisualElements\n" +
                    "BackgroundColor=\"{0}\"\n" +
                    "ShowNameOnSquare150x150Logo=\"on\"\n" +
                    "ForegroundText=\"{1}\"/>\n" +
                    "</Application>\n",
                    Functions.HexConverter(BackgroundColor.Value),
                    ForegroundText
                    ) ;
               
                System.IO.File.WriteAllText(outputFileName, outputXml, Encoding.UTF8);                

            }
            MessageBox.Show("All done.");

            // rf: https://docs.microsoft.com/en-us/uwp/schemas/appxpackage/appxmanifestschema/element-visualelements
            /*
             * 


             */

        }

        private void AddProgramButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "exe|*.exe;*.lnk";
            ofd.ShowDialog();
            foreach(var file in ofd.FileNames)
            {
                if (!System.IO.File.Exists(file)) continue;
                AddFile(file);
            }
            ProgramListBox.ItemsSource = null;
            ProgramListBox.ItemsSource = tilePrograms;
        }

        private void ProgramListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sb = new StringBuilder();
            if (ProgramListBox.SelectedItems.Count == 0)
            {
                ApplyButton.IsEnabled = false;
                sb.Append("Nothing selected");
            }
            else
            {
                ApplyButton.IsEnabled = true;
                
                sb.Append("Apply for:");
                foreach (var app in ProgramListBox.SelectedItems)
                {
                    var tileProgram = (TileProgram)app;
                    sb.Append(" "+tileProgram.Title);

                }
            }

            ApplyForLabel.Content = sb.ToString();
        }
    }
}
