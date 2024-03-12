using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileVerUpTool.Model
{
    internal class HideProjectList
    {
        // 非表示プロジェクトのリストファイルの名前
        private static readonly string HideListFileName = "FileVerUpToolHideList.txt";

        // 非表示プロジェクトのリストを保存するファイルのパス
        // ※このクラスを使う側に保存ファイルパスは任せるが、
        // 　一応、検索対象フォルダパスとして入力したパスが来ることを想定（お勧め）
        private readonly string HideListFilePath = string.Empty;

        private List<string> HideList = new List<string>();


        public HideProjectList(string hideListFilePath)
        {
            HideListFilePath = Path.Combine(hideListFilePath, HideListFileName);
        }

        public void LoadFromFile()
        {
            if (string.IsNullOrEmpty(HideListFilePath))
                throw new InvalidOperationException("対象ファイルが指定されていません");

            HideList = LoadFromFile(HideListFilePath);
        }

        public void SaveToFile()
        {
            SaveToFile(HideList);
        }

        public void Add(string path)
        {
            HideList.Add(path);
        }

        public void Clear()
        {
            HideList.Clear();
        }

        // 現在の隠すプロジェクトリストが、
        // 引数のファイルパス(プロジェクトのファイルパス)を含んでいるかどうか
        public bool Contains(string path)
        {
            return HideList.Contains(path);
        }

        private List<string> LoadFromFile(string path)
        {
            var hideList = new List<string>();

            if (File.Exists(path))
            {
                using (var fs = new FileStream(path, FileMode.Open))
                using (var sr = new StreamReader(fs))
                {
                    // こうすれば、1行ごとにも取り出せる
                    while (sr.Peek() != -1)
                    {
                        hideList.Add(sr.ReadLine());
                    }
                }
            }

            return hideList;
        }

        private void SaveToFile(List<string> paths)
        {
            var tmp = string.Empty;

            paths.ForEach(x =>
            {
                tmp += x + "\r\n";
            });

            File.WriteAllText(HideListFilePath, tmp, new System.Text.UTF8Encoding(true));
        }

    }


}
