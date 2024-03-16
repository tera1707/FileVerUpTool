using FileVerUpTool.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Power;

namespace FileVerUpTool.Model
{
    // csprojを読み書きするクラス
    public class AppxManifestHandler : IProjMetaDataHandler
    {
        private XNamespace ns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";

        public AppxManifestHandler() { }

        public ModuleMetaData? Read(string manifestPath)
        {
            var xmlDoc = XDocument.Load(manifestPath);

            // DisplayNameの値を取得
            string DisplayName = xmlDoc.Root!
                                        .Element(ns + "Properties")!
                                        .Element(ns + "DisplayName")!
                                        .Value;

            // Versionの値を取得
            string ver = xmlDoc.Root!
                                        .Element(ns + "Identity")!
                                        .Attribute("Version")!
                                        .Value;

            // Versionの値を取得
            string company = xmlDoc.Root!
                                        .Element(ns + "Properties")!
                                        .Element(ns + "PublisherDisplayName")!
                                        .Value;

            Debug.WriteLine($"{manifestPath}, {ver}, {DisplayName}, {company}");

            return new ModuleMetaData(/*csprojPath,*/ ver?.ToString(), "-", "-", company?.ToString(), "-", "-", "-", "-", DisplayName);
        }

        public void Write(string projFilePath, ModuleMetaData data)
        {
            // まず、csprojを読み込み、中身の全テキストを保存（こいつを置換していく）
            var all = File.ReadAllText(projFilePath, System.Text.Encoding.UTF8);

            //// 書き換えるデータを用意（仮）
            //var input = new Collection<(string, string)>()
            //    {
            //        ("Version", data.Version),
            //        ("AssemblyVersion", data.AssemblyVersion),
            //        ("FileVersion", data.FileVersion),
            //        ("Company", data.Company),
            //        ("Product", data.Product),
            //        ("Description", data.Description),
            //        ("Copyright", data.Copyright),
            //        ("NeutralLanguage", data.NeutralLanguage),
            //    };

            // 置換実施
            // Version
            {
                var pattern = "\\bVersion\\b=\".*\\..*\\..*\\..*\"";
                var replace = "Version=\"" + data.Version + "\"";
                all = Regex.Replace(all, pattern, replace);
            }

            // DisplayName
            {
                var pattern = "<DisplayName>.*</DisplayName>";
                var replace = "<DisplayName>" + data.ProjectName + "</DisplayName>";
                all = Regex.Replace(all, pattern, replace);
            }

            // Company
            {
                var pattern = "<PublisherDisplayName>.*</PublisherDisplayName>";
                var replace = "<PublisherDisplayName>" + data.Company + "</PublisherDisplayName>";
                all = Regex.Replace(all, pattern, replace);
            }

            File.WriteAllText(projFilePath, all, new System.Text.UTF8Encoding(true));
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
