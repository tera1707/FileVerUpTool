using FileVerUpTool.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FileVerUpTool.Model
{
    // csprojを読み書きするクラス
    public class SdkTypeCsprojHandler : IProjMetaDataHandler
    {
        public SdkTypeCsprojHandler() { }

        public ModuleMetaData? Read(string csprojPath)
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
            var fver = ReturnNullIfThrowException(() => pg.Elements("FileVersion"));
            var company = ReturnNullIfThrowException(() => pg.Elements("Company"));
            var product = ReturnNullIfThrowException(() => pg.Elements("Product"));
            var description = ReturnNullIfThrowException(() => pg.Elements("Description"));
            var copyright = ReturnNullIfThrowException(() => pg.Elements("Copyright"));
            var newtralLang = ReturnNullIfThrowException(() => pg.Elements("NeutralLanguage"));

            Debug.WriteLine($"{csprojPath}, {ver}, {aver}, {fver}, {company}, {product}, {description}, {copyright}, {newtralLang}");

            return new ModuleMetaData(/*csprojPath,*/ ver?.ToString(), aver?.ToString(), fver?.ToString(), company?.ToString(),
                                        product?.ToString(), copyright?.ToString(), description?.ToString(), newtralLang?.ToString());
        }

        public void Write(string projFilePath, ModuleMetaData data)
        {
            // まず、csprojを読み込み、中身の全テキストを保存（こいつを置換していく）
            var all = File.ReadAllText(projFilePath, System.Text.Encoding.UTF8);

            // 書き換えるデータを用意（仮）
            var input = new Collection<(string, string)>()
                {
                    ("Version", data.Version),
                    ("AssemblyVersion", data.AssemblyVersion),
                    ("FileVersion", data.FileVersion),
                    ("Company", data.Company),
                    ("Product", data.Product),
                    ("Description", data.Description),
                    ("Copyright", data.Copyright),
                    ("NeutralLanguage", data.NeutralLanguage),
                };

            // 置換実施
            all = SetValueForSpecifiedKey(all, input);

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