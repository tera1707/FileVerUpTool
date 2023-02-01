using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileVerUpTool.Model
{

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
}
