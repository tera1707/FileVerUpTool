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
        private IProjMetaDataHandler _appxmanifest;

        private VersionReadWrite() { }

        public VersionReadWrite(ISearchSpecifiedExtFile ssef, IProjMetaDataHandler[] handlers)
        {
            _finder = ssef;
            _sdkcsproj = handlers[0];
            _dfwcsproj = handlers[1];
            _cppproj = handlers[2];
            _appxmanifest = handlers[3];
        }

        public async Task<List<(string ProjDataFilePath, ModuleMetaData Module)>> Read(string targetDir)
        {
            // タスク中で使用する仮リスト(直接別スレッド中でDataListに入れると例外になるので)
            var tmp = new List<(string, ModuleMetaData)>();

            (IProjMetaDataHandler Handler, string File, Func<ModuleMetaData, bool> AddJouken)[] tbl =
            {
                (_sdkcsproj, "*.csproj", (x => x != null)),
                (_dfwcsproj, "AssemblyInfo.cs", (x => x != null  && !string.IsNullOrEmpty(x.ProjectName))),
                (_cppproj, "*.rc", (x => x != null)),
                (_appxmanifest, "Package.appxmanifest", (x => x != null))
            };

            await Task.Run(() =>
            {
                foreach (var t in tbl)
                {
                    // 指定フォルダ以下のcsprojファイルを検索
                    var foundList = _finder.Search(targetDir, t.File);

                    // エラー：指定のフォルダの中にcsprojファイルが見つからなかった
                    if (foundList.Count != 0)
                    {
                        // 見つかった奴を表示する
                        foundList.ForEach(x =>
                        {
                            var data = t.Handler.Read(x);

                            if (t.AddJouken(data))
                                tmp.Add((x, data));
                        });
                    }
                }
            });

            return tmp;
        }

        public async Task Write(List<(string ProjDataFilePath, ModuleMetaData Module)> list)
        {
            await Task.Run(() =>
            {
                foreach (var data in list)
                {
                    var ext = System.IO.Path.GetExtension(data.ProjDataFilePath);

                    if (ext == ".csproj")
                    {
                        // .net5-
                        var writer = new SdkTypeCsprojHandler();
                        writer.Write(data.ProjDataFilePath, data.Module);
                    }
                    else if (ext == ".rc")
                    {
                        // CPP
                        var writer = new CppProjHandler();
                        writer.Write(data.ProjDataFilePath, data.Module);
                    }
                    else if (ext == ".cs")
                    {
                        // .netFW
                        var writer = new DotnetFrameworkProjHandler();
                        writer.Write(data.ProjDataFilePath, data.Module);
                    }
                    else // appxmanifest
                    {
                        // .netFW
                        var writer = new AppxManifestHandler();
                        writer.Write(data.ProjDataFilePath, data.Module);
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
        //public List<ModuleMetaData> BulkSetOne(List<ModuleMetaData> currentList, string propName, string val)
        //{
        //    List<ModuleMetaData> tmp = new List<ModuleMetaData>();

        //    // 無理やりコピーをつくる
        //    for (int i = 0; i < currentList.Count; i++)
        //    {
        //        tmp.Add(new ModuleMetaData(currentList[i].FileFullPath, currentList[i].Version, currentList[i].AssemblyVersion, currentList[i].FileVersion, currentList[i].Company,
        //                                    currentList[i].Product, currentList[i].Copyright, currentList[i].Description, currentList[i].NeutralLanguage, currentList[i].ProjectName));
        //    }

        //    // 一括設定するものだけ、無理やり全csproj分入れてる
        //    // propNameは、画面の一括設定コンボボックスで選んだもの(=ModuleMetaDataのプロパティの名前。)
        //    for (int i = 0; i < currentList.Count; i++)
        //    {
        //        typeof(ModuleMetaData).GetProperty(propName).SetValue(tmp[i], val);
        //    }
        //    return tmp;
        //}
    }
}