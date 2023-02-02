using FileVerUpTool.Interface;
using FileVerUpTool.Logic;
using FileVerUpTool.Model;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileVerUpTool
{
    public sealed partial class MainWindow : Window
    {
        private IVersionReadWrite _logic;// = new VersionReadWrite();

        //private SearchSpecifiedExtFile _ssef;

        public ObservableCollection<ModuleMetaData> DataList { get; set; } = new ObservableCollection<ModuleMetaData>();

        private MainWindow() { }

        public MainWindow(IVersionReadWrite logic)
        {
            this.InitializeComponent();

            _logic = logic;
        }

        // Readボタン押下時
        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            var targetDir = TargetDirBox.Text;

            // くるくる開始
            LoadingProgressRing.Visibility = Visibility.Visible;
            DataList.Clear();

            // 読み込み
            var tmpList = await _logic.Read(targetDir);
            tmpList.ToList().ForEach(x => DataList.Add(x));

            // くるくる終了
            LoadingProgressRing.Visibility = Visibility.Collapsed;
        }


        // 書き込みボタン
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadingProgressRing.Visibility = Visibility.Visible;

            // 書き込み
            await _logic.Write(DataList);

            LoadingProgressRing.Visibility = Visibility.Collapsed;
        }

        // 一括入力
        private void IkkatsuButton_Click(object sender, RoutedEventArgs e)
        {
            var ikkatsuType = IkkatsuType.SelectedValue as string;
            var val = IkkatsuValue.Text;

            // リスト中の指定の項目を一括設定をする
            CsprojDataGrid.ItemsSource = _logic.BulkSetOne(DataList, ikkatsuType, val);
        }
    }
}
