using FileVerUpTool.Interface;
using FileVerUpTool.Model;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FileVerUpTool
{
    public sealed partial class MainWindow : Window
    {
        string targetExt = "*.csproj";

        public MainWindow()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<ModuleMetaData> DataList { get; set; } = new ObservableCollection<ModuleMetaData>();

        // Readボタン押下時
        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            LoadingProgressRing.Visibility = Visibility.Visible;

            // タスク中で使用する仮リスト(直接別スレッド中でDataListに入れると例外になるので)
            var tmp = new ObservableCollection<ModuleMetaData>();

            // 画面表示を一旦クリア
            DataList.Clear();

            var targetDir = TargetDirBox.Text;

            await Task.Run(()=>
            {
                // エラー：フォルダが指定されていない
                if (string.IsNullOrEmpty(targetDir))
                    return;

                // エラー：指定のフォルダが存在しない
                if (!Directory.Exists(targetDir))
                    return;

                //////////////////////////////
                // sdkタイプのcsprojを検索
                //////////////////////////////

                // 指定フォルダ以下のcsprojファイルを検索
                var ssef = new SearchSpecifiedExtFile(targetDir, targetExt);
                var foundList = ssef.Search();

                // エラー：指定のフォルダの中にcsprojファイルが見つからなかった
                if (foundList.Count != 0)
                {
                    // 見つかった奴を表示する
                    foundList.ForEach(x =>
                    {
                        // 本ちゃん
                        var reader = new SdkTypeCsprojHandler();
                        var data = reader.Read(x);

                        if (data != null)
                            tmp.Add(data);
                    });
                }

                //////////////////////////////
                // .netFrameworkのAssemblyInfo.csを検索
                //////////////////////////////

                // 指定フォルダ以下のcsprojファイルを検索
                var ssef2 = new SearchSpecifiedExtFile(targetDir, "AssemblyInfo.cs");
                var foundList2 = ssef2.Search();

                if (foundList2.Count != 0)
                {
                    foundList2.ForEach(x =>
                    {
                        var reader = new DotnetFrameworkProjHandler();
                        var data = reader.Read(x);

                        if (data != null && !string.IsNullOrEmpty(data.ProjectName))
                            tmp.Add(data);
                    });
                }

                //////////////////////////////
                // C++のリソース.rcを検索
                //////////////////////////////

                // 指定フォルダ以下のcsprojファイルを検索
                var ssef3 = new SearchSpecifiedExtFile(targetDir, "*.rc");
                var foundList3 = ssef3.Search();

                if (foundList3.Count != 0)
                {
                    foundList3.ForEach(x =>
                    {
                        var reader = new CppProjHandler();
                        var data = reader.Read(x);

                        if (data != null)
                            tmp.Add(data);
                    });
                }
            });

            // 画面表示のリストに入れる
            tmp.ToList().ForEach(x => DataList.Add(x));

            LoadingProgressRing.Visibility = Visibility.Collapsed;
        }


        // 書き込みボタン
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadingProgressRing.Visibility = Visibility.Visible;

            await Task.Run(()=>
            {
                foreach (var data in DataList)
                {
                    var ext = System.IO.Path.GetExtension(data.FileFullPath);

                    if (ext == ".csproj")
                    {
                        // .net5-
                        var writer = new SdkTypeCsprojHandler();
                        writer.Write(data);
                    }
                    else if (ext == ".rc")
                    {
                        // CPP
                        var writer = new CppProjHandler();
                        writer.Write(data);
                    }
                    else
                    {
                        // .netFW
                        var writer = new DotnetFrameworkProjHandler();
                        writer.Write(data);
                    }
                }
            });
            LoadingProgressRing.Visibility = Visibility.Collapsed;
        }

        // 一括入力
        private void IkkatsuButton_Click(object sender, RoutedEventArgs e)
        {
            var ikkatsuType = IkkatsuType.SelectedValue as string;
            var val = IkkatsuValue.Text;

            setvalue(ikkatsuType, val);

            // 指定の項目を一括設定をする
            // ItemsSourceに入れたリストが更新されたときに、画面のリストも更新してくれるDataGrid.Refresh()みたいなメソッドが
            // 無かったので、無理やりコピーをつくって入れ替えてる。
            void setvalue(string propName, string val)
            {
                ObservableCollection<ModuleMetaData> tmp = new ObservableCollection<ModuleMetaData>();

                // 無理やりコピーをつくる
                for (int i = 0; i < DataList.Count; i++)
                {
                    tmp.Add(new ModuleMetaData(DataList[i].FileFullPath, DataList[i].Version, DataList[i].AssemblyVersion, DataList[i].FileVersion, DataList[i].Company,
                                                DataList[i].Product, DataList[i].Copyright, DataList[i].Description, DataList[i].NeutralLanguage, DataList[i].ProjectName));
                }

                // 一括設定するものだけ、無理やり全csproj分入れてる
                for (int i = 0; i < DataList.Count; i++)
                {
                    typeof(ModuleMetaData).GetProperty(propName).SetValue(tmp[i], val);
                }

                // 入れ替え
                CsprojDataGrid.ItemsSource = tmp;
            }
        }
    }
}
