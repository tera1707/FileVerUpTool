using FileVerUpTool.Interface;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace FileVerUpTool.Model
{
    public class CppProjHandler : IProjMetaDataHandler
    {
        public ModuleMetaData Read(string cppRcPath)
        {
            // まず、rc、中身の全テキストを保存（こいつを置換していく）
            var lines = File.ReadAllLines(cppRcPath, System.Text.Encoding.UTF8);

            string pjName = string.Empty;
            string productVersion = string.Empty;
            string assemblyVersion = string.Empty;
            string fileVersion = string.Empty;
            string company = string.Empty;
            string product = string.Empty;
            string copyright = string.Empty;
            string description = string.Empty;

            foreach (var line in lines)
            {
                if (line.Contains("FileVersion")) fileVersion = line.Split("\"")[3];
                if (line.Contains("ProductVersion")) productVersion = line.Split("\"")[3];
                if (line.Contains("ProductName")) product = line.Split("\"")[3];
                if (line.Contains("CompanyName")) company = line.Split("\"")[3];
                if (line.Contains("FileDescription")) description = line.Split("\"")[3];
                if (line.Contains("LegalCopyright")) copyright = line.Split("\"")[3];
            }

            return new ModuleMetaData(/*cppRcPath,*/ productVersion, assemblyVersion, fileVersion, company, product, copyright, description, "");
        }

        public void Write(string projFilePath, ModuleMetaData data)
        {
            var expBase = "VALUE \"";
            var expBase2 = "\", \".*\"";
            var valBase = "VALUE \"";
            var valBase2 = "\", \"";
            var valBase3 = "\"";

            var all = File.ReadAllText(projFilePath, System.Text.Encoding.UTF8);

            // 書き換えるデータを用意
            var items = new Collection<(string propName, string val)>()
                {
                    ("FileVersion", data.FileVersion),
                    ("ProductVersion", data.Version),
                    ("CompanyName", data.Company),
                    ("ProductName", data.Product),
                    ("LegalCopyright", data.Copyright),
                    ("FileDescription", data.Description),
                };

            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.val))
                    continue;//設定すべきデータがない場合はなにもしない

                var pattern = expBase + item.propName + expBase2;
                var val = valBase + item.propName + valBase2 + item.val + valBase3;

                all = Regex.Replace(all, pattern, val);

                // バージョンだけ特別
                if (item.propName == "FileVersion")
                    all = Regex.Replace(all, " FILEVERSION .*", " FILEVERSION " + item.val.Replace('.', ','));
                if (item.propName == "ProductVersion")
                    all = Regex.Replace(all, " PRODUCTVERSION .*", " PRODUCTVERSION " + item.val.Replace('.', ','));
            }

            File.WriteAllText(projFilePath, all, new System.Text.UTF8Encoding(true));
        }
    }
}