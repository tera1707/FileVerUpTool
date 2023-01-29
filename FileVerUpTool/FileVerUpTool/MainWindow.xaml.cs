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
using Windows.System;
using Windows.UI.Popups;
using static FileVerUpTool.MainWindow;


/*
■おかしいところ
✅.netFW 元々データがないものに対して設定しようとしても、設定できない。(置換するものがないからだと思う)

■要検討事項
・,net6なのか、FWなのかの切り替えの判定方法が今てきとう。（複数個所ある）
　見るファイルがcsprojなら.net6、そうでなければ(AssemblyInfo.csなら？)FWにしてるがそれでいいのか。（C++もみたいときにこまらないか？）
・FWのためにAssemblyInfo.csを探すが、そういうファイル名、普通にありそう。
　AssemblyInfo.csをみて、さらにそのなかのなにかをみて、ちゃんとバージョン情報を含んだものなのかを判定したほうがいい？
　(例えば、最低、AssemblyVersionとAssemblyFileVersionはあるはずだからその2つがあれば正しい、とするとか。)

■やりたいこと
・Authorを消したい。いらんっぽい。
・クラス分け。

*/
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

        // Readボタン押下時
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
                        DataList.Add(data);
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

                    if (data != null)
                        DataList.Add(data);
                });
            }

        }


        // 書き込みボタン
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var data in DataList)
            {
                if (System.IO.Path.GetExtension(data.FileFullPath) == ".csproj")
                {
                    var writer = new SdkTypeCsprojHandler();
                    writer.Write(data);
                }
                else
                {
                    var writer = new DotnetFrameworkProjHandler();
                    writer.Write(data);
                }
            }
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

        // csprojを読み書きするクラス
        public interface IProjMetaDataHandler
        {
            ModuleMetaData? Read(string csprojPath);
            void Write(ModuleMetaData data);
        }

        public class DotnetFrameworkProjHandler : IProjMetaDataHandler
        {
            public ModuleMetaData Read(string AssemblyInfoPath)
            {
                //var metadata = new ModuleMetaData();
                string pjName = string.Empty;
                string version = string.Empty;
                string assemblyVersion = string.Empty;
                string authors = string.Empty;
                string company = string.Empty;
                string product = string.Empty;
                string copyright = string.Empty;
                string description = string.Empty;
                string neutralLanguage = string.Empty;

                // まず、csprojを読み込み、中身の全テキストを保存（こいつを置換していく）
                var lines = File.ReadAllLines(AssemblyInfoPath, System.Text.Encoding.UTF8);

                foreach (var line in lines)
                {
                    if (line.Contains("AssemblyTitle")) pjName = line.Split("\"")[1];
                    if (line.Contains("AssemblyVersion")) version = line.Split("\"")[1];
                    //if (line.Contains("AssemblyVersion")) assemblyVersion = line.Split("\"")[1];
                    if (line.Contains("AssemblyAuthors")) authors = line.Split("\"")[1];
                    if (line.Contains("AssemblyCompany")) company = line.Split("\"")[1];
                    if (line.Contains("AssemblyProduct")) product = line.Split("\"")[1];
                    if (line.Contains("AssemblyCopyright")) copyright = line.Split("\"")[1];
                    if (line.Contains("AssemblyDescription")) description = line.Split("\"")[1];
                    if (line.Contains("NeutralResourcesLanguage")) neutralLanguage = line.Split("\"")[1];
                }

                return new ModuleMetaData(AssemblyInfoPath, version, assemblyVersion, authors, company, product, copyright, description, neutralLanguage, pjName);

            }

            public void Write(ModuleMetaData data)
            {
                var expBase = "(^|(?<=\r\n))\\[assembly: ";
                var expBase2 = "\\(\".*\"\\)\\]";

                var all = File.ReadAllText(data.FileFullPath, System.Text.Encoding.UTF8);

                // 書き換えるデータを用意
                var items = new Collection<(string propName, string val)>()
                {
                    ("AssemblyVersion", data.Version),
                    ("AssemblyCompany", data.Company),
                    ("AssemblyProduct", data.Product),
                    ("AssemblyCopyright", data.Copyright),
                    ("AssemblyDescription", data.Description),
                    ("NeutralResourcesLanguage", data.NeutralLanguage),
                };

                foreach (var item in items)
                {
                    if (string.IsNullOrEmpty(item.val))
                        continue;//設定すべきデータがない場合はなにもしない

                    var pattern = expBase + item.propName + expBase2;
                    var val = "[assembly: " + item.propName + "(\"" + item.val + "\")]";

                    // 該当する項目がまだない場合は
                    if (!Regex.IsMatch(all, pattern))
                    {
                        all += val + "\r\n";
                    }

                    all = Regex.Replace(all, pattern, val);
                }
                //all = Regex.Replace(all, expBase + "AssemblyVersion" + expBase2, "[assembly: AssemblyVersion(\"" + data.Version + "\")]");
                //all = Regex.Replace(all, expBase + "AssemblyCompany" + expBase2, "[assembly: AssemblyCompany(\"" + data.Company + "\")]");
                //all = Regex.Replace(all, expBase + "AssemblyProduct" + expBase2, "[assembly: AssemblyProduct(\"" + data.Product + "\")]");
                //all = Regex.Replace(all, expBase + "AssemblyCopyright" + expBase2, "[assembly: AssemblyCopyright(\"" + data.Copyright + "\")]");
                //all = Regex.Replace(all, expBase + "AssemblyDescription" + expBase2, "[assembly: AssemblyDescription(\"" + data.Description + "\")]");
                //all = Regex.Replace(all, expBase + "NeutralResourcesLanguage" + expBase2, "[assembly: NeutralResourcesLanguage(\"" + data.NeutralLanguage + "\")]");

                File.WriteAllText(data.FileFullPath, all);
            }
        }

        // csprojを読み書きするクラス
        public class SdkTypeCsprojHandler : IProjMetaDataHandler
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


    }

    public class ModuleMetaData 
    {
        public string FileFullPath { get; set; }
        public string ProjectName { get; set; }//.NET Framework用
        public string Version { get; set; }
        public string AssemblyVersion { get; set; }
        public string Authors { get; set; }
        public string Company { get; set; }
        public string Product { get; set; }
        public string Copyright { get; set; }
        public string Description { get; set; }
        public string NeutralLanguage { get; set; }
        public ModuleMetaData(string fileFullPath, string version, string assemblyVersion, string authors,
                                string company, string product, string copyRight, string description, string neutralLanguage, string projectName = "")
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
            ProjectName = projectName;
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
            var data = value as ModuleMetaData;

            if (System.IO.Path.GetExtension(data.FileFullPath) == ".csproj")
            {
                return System.IO.Path.GetFileNameWithoutExtension(data.FileFullPath);
            }
            else
            {
                return data.ProjectName;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
