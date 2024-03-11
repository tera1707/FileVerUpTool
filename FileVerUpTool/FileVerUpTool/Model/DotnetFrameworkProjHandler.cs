using FileVerUpTool.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileVerUpTool.Model
{
    public class DotnetFrameworkProjHandler : IProjMetaDataHandler
    {
        public ModuleMetaData Read(string AssemblyInfoPath)
        {
            //var metadata = new ModuleMetaData();
            string pjName = string.Empty;
            string fileversion = string.Empty;
            string assemblyVersion = string.Empty;
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
                if (line.Contains("AssemblyVersion")) assemblyVersion = line.Split("\"")[1];
                if (line.Contains("AssemblyFileVersion")) fileversion = line.Split("\"")[1];// .netFWでは、製品VerとファイルVerがおなじ値になる
                if (line.Contains("AssemblyCompany")) company = line.Split("\"")[1];
                if (line.Contains("AssemblyProduct")) product = line.Split("\"")[1];
                if (line.Contains("AssemblyCopyright")) copyright = line.Split("\"")[1];
                if (line.Contains("AssemblyDescription")) description = line.Split("\"")[1];
                if (line.Contains("NeutralResourcesLanguage")) neutralLanguage = line.Split("\"")[1];
            }

            return new ModuleMetaData(AssemblyInfoPath, fileversion, assemblyVersion, fileversion, company, product, copyright, description, neutralLanguage, pjName);

        }

        public void Write(ModuleMetaData data)
        {
            var expBase = "(^|(?<=\n))\\[assembly: ";
            var expBase2 = "\\(\".*\"\\)\\]";

            var all = File.ReadAllText(data.FileFullPath, System.Text.Encoding.UTF8);
            data.FileVersion = data.Version;// .NETFWは、FileVersionとVersionが同じ値。

            // 書き換えるデータを用意
            var items = new Collection<(string propName, string val)>()
                {
                    ("AssemblyFileVersion", data.FileVersion),
                    ("AssemblyVersion", data.AssemblyVersion),
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

            File.WriteAllText(data.FileFullPath, all, new System.Text.UTF8Encoding(true));
        }
    }
}
