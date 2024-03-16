using FileVerUpTool.Interface;
using FileVerUpTool.Model;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileVerUpTool
{
    public sealed partial class MainWindow : Window
    {
        private IVersionReadWrite _logic;// = new VersionReadWrite();

        public ObservableCollection<WholeData> DataList { get; set; } = new ObservableCollection<WholeData>();

        private MainWindow() { }

        public MainWindow(IVersionReadWrite logic)
        {
            this.InitializeComponent();

            _logic = logic;
        }

        // 全表示
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            await ReadButton();
        }

        // 付加情報を保存
        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            ////////////////// この部分をクラス化必要（付加情報の読み書きクラス）→logicに負荷情報保存クラスを設けてそこで負荷情報保存クラスを呼んでやる
            var tmp = string.Empty;
            var additionalInfoFilePath = System.IO.Path.Combine(TargetDirBox.Text, "AdditionalData.txt");

            DataList.ToList().ForEach(x =>
            {
                tmp += x.ProjFilePath + "," + x.Additional.Visible + "," + x.Additional.Remark + "\r\n";
            });

            File.WriteAllText(additionalInfoFilePath, tmp, new System.Text.UTF8Encoding(true));
            /////////////////////////////////////////////////////////////////////
        }

        //---------------------------------------------

        private async Task ReadButton()
        {
            var targetDir = TargetDirBox.Text;

            // くるくる開始
            LoadingProgressRing.Visibility = Visibility.Visible;
            DataList.Clear();


            ////////////////// この部分をクラス化必要（付加情報の読み書きクラス）→logicのReadの中に移動
            var additionalInfoFilePath = System.IO.Path.Combine(TargetDirBox.Text, "AdditionalData.txt");

            var additionalInfoList = new List<(string ProjFilePath, bool Visible, string Remark)>();

            if (File.Exists(additionalInfoFilePath))
            {
                using (var fs = new FileStream(additionalInfoFilePath, FileMode.Open))
                using (var sr = new StreamReader(fs))
                {
                    while (sr.Peek() != -1)
                    {
                        var line = sr.ReadLine();
                        var s = line.Split(',');
                        additionalInfoList.Add((s[0], bool.Parse(s[1]), s[2]));
                    }
                }
            }
            /////////////////////////////////////////////////////////////////////
            
            // バージョン情報読み込み
            var tmpList = await _logic.Read(targetDir);

            Debug.WriteLine(tmpList.Count());

            tmpList.ToList().ForEach(x =>
            {
                var a = additionalInfoList.Where(y => x.ProjDataFilePath == y.ProjFilePath);

                if (a.Any())
                {
                    var adash = a.FirstOrDefault();

                    DataList.Add(new WholeData()
                    {
                        ProjFilePath = x.ProjDataFilePath,
                        Additional = new AdditionalData(adash.Visible, adash.Remark),
                        Module = x.Module,
                    });
                }
                else
                {
                    DataList.Add(new WholeData()
                    {
                        ProjFilePath = x.ProjDataFilePath,
                        Additional = new AdditionalData(true, ""),
                        Module = x.Module,
                    });
                }

            });

            // Visibleにチェックがついているものを前に持ってくる（見やすいように）
            var tmp = DataList.OrderByDescending(x => x.Additional.Visible)
                                .ThenByDescending(x => x.Module.ProjectName)
                                .ToList();
            DataList.Clear();
            tmp.ForEach(x => DataList.Add(x));

            // くるくる終了
            LoadingProgressRing.Visibility = Visibility.Collapsed;
        }

        // バージョン情報 書き込みボタン
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadingProgressRing.Visibility = Visibility.Visible;

            // VisibleチェックがついているもののModuleデータを保存する
            var moduleDataList = DataList
                                    .Where(x => x.Additional.Visible == true)
                                    .Select(x => (x.ProjFilePath, x.Module))
                                    .ToList();

            await _logic.Write(moduleDataList);

            LoadingProgressRing.Visibility = Visibility.Collapsed;
        }
    }
}