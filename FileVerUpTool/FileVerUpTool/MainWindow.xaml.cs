using FileVerUpTool.Converter;
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
        private IAdditionalInfoManager _additionalInfoManager = new AdditionalInfoManager();

        public ObservableCollection<WholeData> DataList { get; set; } = new ObservableCollection<WholeData>();

        private MainWindow() { }

        public MainWindow(IVersionReadWrite logic)
        {
            this.InitializeComponent();

            Title = "File Ver Up Tool ver.20240317";

            _logic = logic;
        }

        // 全表示
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();

            await ReadButton();

            sw.Stop();
            Debug.WriteLine(" 経過時間：" + sw.ElapsedMilliseconds + " ms");
        }

        // 付加情報を保存
        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            _additionalInfoManager.Save(TargetDirBox.Text, DataList);
        }

        //---------------------------------------------

        private async Task ReadButton()
        {
            var targetDir = TargetDirBox.Text;

            // くるくる開始
            LoadingProgressRing.Visibility = Visibility.Visible;
            DataList.Clear();

            // 付加情報の読み込み
            var additionalInfoList = _additionalInfoManager.Load(targetDir);
            
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
                                .ThenBy(x => WholeDataToProjectName(x))
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

        private string WholeDataToProjectName(WholeData x)
        {
            var ext = System.IO.Path.GetExtension(x.ProjFilePath);

            if (ext == ".csproj" || ext == ".rc")
            {
                return System.IO.Path.GetFileNameWithoutExtension(x.ProjFilePath);
            }
            else
            {
                return x.Module.ProjectName;
            }
        }

    }
}