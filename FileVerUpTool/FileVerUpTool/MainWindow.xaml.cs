using FileVerUpTool.Interface;
using FileVerUpTool.Logic;
using FileVerUpTool.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileVerUpTool
{
    public sealed partial class MainWindow : Window
    {
        private IVersionReadWrite _logic;// = new VersionReadWrite();

        public ObservableCollection<WholeMetaData> DataList { get; set; } = new ObservableCollection<WholeMetaData>();

        private HideProjectList? _hidePJList;

        private MainWindow() { }

        public MainWindow(IVersionReadWrite logic)
        {
            this.InitializeComponent();

            _logic = logic;
        }

        // Readボタン押下時(非表示設定のPJは表示しない)
        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataList is null || DataList.Count() == 0 || _hidePJList is null)
                return;

            // チェックがOFFになっている項目(=非表示にしたいプロジェクト)をファイルに保存
            _hidePJList.Clear();
            DataList.Where(x => x.Visible == false).ToList().ForEach(x => _hidePJList.Add(x.Module.FileFullPath));
            _hidePJList.SaveToFile();

            await ReadButton(false);
        }

        // Readボタン押下時(非表示設定のPJも表示する)
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            await ReadButton(true);
        }

        //---------------------------------------------

        private async Task ReadButton(bool showAll)
        {
            var targetDir = TargetDirBox.Text;

            // くるくる開始
            LoadingProgressRing.Visibility = Visibility.Visible;
            DataList.Clear();

            // 非表示プロジェクト管理クラスを初期化
            _hidePJList = new HideProjectList(TargetDirBox.Text);
            _hidePJList.LoadFromFile();

            // バージョン情報読み込み
            var tmpList = await _logic.Read(targetDir);

            tmpList.ToList().ForEach(x =>
            {
                if (showAll)
                {
                    // 全表示時：
                    // →非表示リストにあるものは、Visubleチェックを外して表示する
                    DataList.Add(new WholeMetaData() { Visible = _hidePJList.Contains(x.FileFullPath) ? false : true, Module = x });
                }
                else if (!_hidePJList.Contains(x.FileFullPath))
                {
                    // 非表示プロジェクトリストにあるPJは非表示時：
                    // →非表示リストにあるものは表示しない
                    DataList.Add(new WholeMetaData() { Visible = true, Module = x });
                }
            });

            // くるくる終了
            LoadingProgressRing.Visibility = Visibility.Collapsed;
        }

        // バージョン情報 書き込みボタン
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadingProgressRing.Visibility = Visibility.Visible;

            // 書き込み
            await _logic.Write(DataList.Where(x => x.Visible == true).Select(x => x.Module).ToList());

            LoadingProgressRing.Visibility = Visibility.Collapsed;
        }
    }
}
