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
    }
}
