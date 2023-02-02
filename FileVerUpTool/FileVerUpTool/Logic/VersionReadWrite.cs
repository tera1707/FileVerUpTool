using FileVerUpTool.Interface;
using FileVerUpTool.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileVerUpTool.Logic
{
    public class VersionReadWrite : IVersionReadWrite
    {
        private ISearchSpecifiedExtFile _finder;
        private IProjMetaDataHandler _sdkcsproj;
        private IProjMetaDataHandler _dfwcsproj;
        private IProjMetaDataHandler _cppproj;

        private VersionReadWrite() { }

        public VersionReadWrite(ISearchSpecifiedExtFile ssef, IProjMetaDataHandler[] handlers)
        {
            _finder = ssef;
            _sdkcsproj = handlers[0];
            _dfwcsproj = handlers[1];
            _cppproj = handlers[2];
        }

        public async Task<ObservableCollection<ModuleMetaData>> Read(string targetDir)
        {
            // タスク中で使用する仮リスト(直接別スレッド中でDataListに入れると例外になるので)
            var tmp = new ObservableCollection<ModuleMetaData>();

            //var targetDir = TargetDirBox.Text;

            await Task.Run(() =>
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
                var foundList = _finder.Search(targetDir, "*.csproj");

                // エラー：指定のフォルダの中にcsprojファイルが見つからなかった
                if (foundList.Count != 0)
                {
                    // 見つかった奴を表示する
                    foundList.ForEach(x =>
                    {
                        // 本ちゃん
                        //var reader = new SdkTypeCsprojHandler();
                        var data = _sdkcsproj.Read(x);

                        if (data != null)
                            tmp.Add(data);
                    });
                }

                //////////////////////////////
                // .netFrameworkのAssemblyInfo.csを検索
                //////////////////////////////

                // 指定フォルダ以下のcsprojファイルを検索
                var foundList2 = _finder.Search(targetDir, "AssemblyInfo.cs");

                if (foundList2.Count != 0)
                {
                    foundList2.ForEach(x =>
                    {
                        //var reader = new DotnetFrameworkProjHandler();
                        var data = _dfwcsproj.Read(x);

                        if (data != null && !string.IsNullOrEmpty(data.ProjectName))
                            tmp.Add(data);
                    });
                }

                //////////////////////////////
                // C++のリソース.rcを検索
                //////////////////////////////

                // 指定フォルダ以下のcsprojファイルを検索
                var foundList3 = _finder.Search(targetDir, "*.rc");

                if (foundList3.Count != 0)
                {
                    foundList3.ForEach(x =>
                    {
                        var data = _cppproj.Read(x);

                        if (data != null)
                            tmp.Add(data);
                    });
                }
            });

            return tmp;
        }

        public async Task Write(ObservableCollection<ModuleMetaData> list)
        {
            await Task.Run(() =>
            {
                foreach (var data in list)
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
        }

        /// <summary>
        /// リスト中の指定の項目を一括設定をする
        /// ItemsSourceに入れたリストが更新されたときに、画面のリストも更新してくれるDataGrid.Refresh()みたいなメソッドが
        /// 無かったので、無理やりコピーをつくって入れ替えてる。
        /// </summary>
        /// <param name="currentList">現在のバージョン等情報のリスト</param>
        /// <param name="propName">一括設定したいModuleMetaDataのプロパティ名</param>
        /// <param name="val">一括設定したい値</param>
        /// <returns></returns>
        public ObservableCollection<ModuleMetaData> BulkSetOne(ObservableCollection<ModuleMetaData> currentList, string propName, string val)
        {
            ObservableCollection<ModuleMetaData> tmp = new ObservableCollection<ModuleMetaData>();

            // 無理やりコピーをつくる
            for (int i = 0; i < currentList.Count; i++)
            {
                tmp.Add(new ModuleMetaData(currentList[i].FileFullPath, currentList[i].Version, currentList[i].AssemblyVersion, currentList[i].FileVersion, currentList[i].Company,
                                            currentList[i].Product, currentList[i].Copyright, currentList[i].Description, currentList[i].NeutralLanguage, currentList[i].ProjectName));
            }

            // 一括設定するものだけ、無理やり全csproj分入れてる
            // propNameは、画面の一括設定コンボボックスで選んだもの(=ModuleMetaDataのプロパティの名前。)
            for (int i = 0; i < currentList.Count; i++)
            {
                typeof(ModuleMetaData).GetProperty(propName).SetValue(tmp[i], val);
            }
            return tmp;
        }
    }
}
