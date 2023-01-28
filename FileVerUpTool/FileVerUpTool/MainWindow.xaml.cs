using CommunityToolkit.WinUI.UI.Controls;
using CommunityToolkit.WinUI.UI.Controls.Primitives;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Windows.UI.Popups;

namespace FileVerUpTool
{
    public sealed partial class MainWindow : Window
    {
        string targetExt = "*.csproj";

        public MainWindow()
        {
            this.InitializeComponent();

            // DEBUG
            TargetDirBox.Text = @"C:\Users\masa\source\repos\FlyoutableToggleButtonJikken";
        }

        public ObservableCollection<ModuleMetaData> DataList { get; set; } = new ObservableCollection<ModuleMetaData>();


        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            // 画面表示を一旦クリア
            DataList.Clear();

            var targetDir = TargetDirBox.Text;

            // エラー：フォルダが指定されていない
            if (string.IsNullOrEmpty(targetDir))
                return;

            // エラー：指定のフォルダが存在しない
            if (!Directory.Exists(targetDir))
                return;

            // 指定フォルダ以下のcsprojファイルを検索
            var ssef = new SearchSpecifiedExtFile(targetDir, targetExt);
            var foundList = ssef.Search();

            // エラー：指定のフォルダの中にcsprojファイルが見つからなかった
            if (foundList.Count == 0)
                return;

            // 見つかった奴を表示する
            foundList.ForEach(x =>
            {
                var reader = new SdkTypeCsprojHandler();
                var data = reader.Read(x);

                DataList.Add(data);
            });
        }

        // csprojを読み書きするクラス
        public class SdkTypeCsprojHandler
        {
            public SdkTypeCsprojHandler() { }

            public ModuleMetaData? Read(string  csprojPath)
            {
                XElement xml = XElement.Load(csprojPath);

                // 欲しい奴だけ取る
                //#region 欲しい奴だけ取る
                var infos = xml.Elements("PropertyGroup");

                if (infos is null)
                    return null;

                var pg = infos.FirstOrDefault();

                if (pg is null)
                    return null;

                var ver = ReturnNullIfThrowException(() => pg.Elements("Version"));
                var aver = ReturnNullIfThrowException(() => pg.Elements("AssemblyVersion"));
                var author = ReturnNullIfThrowException(() => pg.Elements("Authors"));
                var company = ReturnNullIfThrowException(() => pg.Elements("Company"));
                var product = ReturnNullIfThrowException(() => pg.Elements("Product"));
                var description = ReturnNullIfThrowException(() => pg.Elements("Description"));
                var copyright = ReturnNullIfThrowException(() => pg.Elements("Copyright"));
                var newtralLang = ReturnNullIfThrowException(() => pg.Elements("NeutralLanguage"));

                Debug.WriteLine($"{csprojPath}, {ver}, {aver}, {author}, {company}, {product}, {description}, {copyright}, {newtralLang}");

                return new ModuleMetaData(csprojPath, ver?.ToString(), aver?.ToString(), author?.ToString(), company?.ToString(),
                                            product?.ToString(), copyright?.ToString(), description?.ToString(), newtralLang?.ToString());
            }

            public void Write(ModuleMetaData data)
            {
                // まず、csprojを読み込み、中身の全テキストを保存（こいつを置換していく）
                var all = File.ReadAllText(data.FileFullPath, System.Text.Encoding.UTF8);

                // 書き換えるデータを用意（仮）
                var input = new Collection<(string, string)>()
                {
                    ("Version", data.Version),
                    ("AssemblyVersion", data.AssemblyVersion),
                    ("Authors", data.Authors),
                    ("Company", data.Company),
                    ("Product", data.Product),
                    ("Description", data.Description),
                    ("Copyright", data.Copyright),
                    ("NeutralLanguage", data.NeutralLanguage),
                };

                // 置換実施
                all = SetValueForSpecifiedKey(all, input);

                File.WriteAllText(data.FileFullPath, all);
            }

            private string ReturnNullIfThrowException(Func<IEnumerable<XElement>> getElementFunc)
            {
                string ret = null;

                try
                {
                    ret = getElementFunc.Invoke().FirstOrDefault().Value;
                }
                catch (NullReferenceException)
                {
                    ret = null;
                }

                return ret;
            }

            private string SetValueForSpecifiedKey(string all, IReadOnlyCollection<(string ElementName, string Value)> input)
            {
                //var all = File.ReadAllText(x, System.Text.Encoding.UTF8);

                foreach (var element in input)
                {
                    if (string.IsNullOrEmpty(element.Value))
                        continue;

                    // 指定の値で置き換える
                    //var pattern = "\\<Version\\>.*\\<\\/Version\\>";
                    var pattern = "\\<" + element.ElementName + "\\>.*\\<\\/" + element.ElementName + "\\>";

                    if (!Regex.IsMatch(all, pattern))
                    {
                        // 指定のelementが無ければ
                        // とりあえず、最初に見つけたPropertyGroupの最後のElementとして、無理やり指定のElementを入れてやる
                        var temp = new Regex("</PropertyGroup>");
                        var rep = "  <" + element.ElementName + "></" + element.ElementName + ">\r\n  </PropertyGroup>";
                        all = temp.Replace(all, rep, 1);
                    }

                    // 指定の値で置換する
                    var replace = "<" + element.ElementName + ">" + element.Value + "</" + element.ElementName + ">";//仮
                    all = Regex.Replace(all, pattern, replace);
                }
                return all;
            }
        }



        // 書き込みボタン
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach(var data in DataList)
            {
                var writer = new SdkTypeCsprojHandler();
                writer.Write(data);
            }
        }

        private string ReturnNullIfThrowException(Func<IEnumerable<XElement>> getElementFunc)
        {
            string ret = null;

            try
            {
                ret = getElementFunc.Invoke().FirstOrDefault().Value;
            }
            catch (NullReferenceException)
            {
                ret = null;
            }

            return ret;
        }


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
                    tmp.Add(new ModuleMetaData(DataList[i].FileFullPath, DataList[i].Version, DataList[i].AssemblyVersion, DataList[i].Authors, DataList[i].Company,
                                                DataList[i].Product, DataList[i].Copyright, DataList[i].Description, DataList[i].NeutralLanguage));
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

    public class ModuleMetaData 
    {
        public string FileFullPath { get; set; }
        public string Version { get; set; }
        public string AssemblyVersion { get; set; }
        public string Authors { get; set; }
        public string Company { get; set; }
        public string Product { get; set; }
        public string Copyright { get; set; }
        public string Description { get; set; }
        public string NeutralLanguage { get; set; }
        public ModuleMetaData(string fileFullPath, string version, string assemblyVersion, string authors,
                                string company, string product, string copyRight, string description, string neutralLanguage)
        {
            FileFullPath = fileFullPath;
            Version = version;
            AssemblyVersion = assemblyVersion;
            Authors = authors;
            Company = company;
            Product = product;
            Copyright = copyRight;
            Description = description;
            NeutralLanguage = neutralLanguage;
        }
    }

    // 指定フォルダから指定拡張子のファイルを探してListにするクラス
    public class SearchSpecifiedExtFile
    {
        private string _targetDirectoryFullPath = string.Empty;
        private string _targetExt = "*.csproj";
        private List<string> foundList = new();

        private SearchSpecifiedExtFile() { }

        public SearchSpecifiedExtFile(string targetDirectoryFullPath, string targetExt)
        {
            _targetDirectoryFullPath = targetDirectoryFullPath;
            _targetExt = targetExt;
        }

        public List<string> Search()
        {
            Search(_targetDirectoryFullPath);
            return foundList;
        }

        private void Search(string target)
        {
            var parentDir = new DirectoryInfo(target);
            var files = parentDir.GetFiles(_targetExt);

            // その階層にある対象ファイルをリストに入れる
            files.ToList().ForEach(x => foundList.Add(x.FullName));

            // その階層にあるフォルダを探し、
            var childDirs = parentDir.GetDirectories();
            // フォルダの中を探しに行く(再帰的に)
            foreach (var dir in childDirs)
            {
                Search(dir.FullName);
            }
        }
    }
    //private static string FileFullPathToProjectNam(string fullPath)
    //{
    //    return System.IO.Path.GetFileNameWithoutExtension(fullPath);
    //}
    public sealed class FileFullPathToProjectName : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var fullPath = value as string;
            return System.IO.Path.GetFileNameWithoutExtension(fullPath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
