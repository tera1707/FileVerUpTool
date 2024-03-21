using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Standard20
{
    public class Std20
    {
        public void func()
        {
            Debug.WriteLine("abc");

            var dict1 = new Dictionary<string, string>();
            dict1.Add("fruits-1", "いちご");
            dict1.Add("fruits-2", "りんご");
            Debug.WriteLine(JsonUtilSample.ToJson(dict1));

            var dict2 = new Dictionary<string, int>();
            dict2.Add("国語", 80);
            dict2.Add("英語", 90);
            Debug.WriteLine(JsonUtilSample.ToJson(dict2));
        }
    }


    public class JsonUtilSample
    {
        /// <summary>
        /// 入力をJSON文字列に変換します。
        /// </summary>
        /// <param name="dict">Dictionary<string, string>型の入力</param>
        /// <returns>JSON文字列</returns>
        public static string ToJson(Dictionary<string, string> dict)
        {
            var json = JsonSerializer.Serialize(dict, JsonUtilSample.GetOption());
            return json;
        }

        /// <summary>
        /// 入力をJSON文字列に変換します。
        /// </summary>
        /// <param name="dict">Dictionary<string, int>型の入力</param>
        /// <returns>JSON文字列</returns>
        public static string ToJson(Dictionary<string, int> dict)
        {
            var json = JsonSerializer.Serialize(dict, JsonUtilSample.GetOption());
            return json;
        }

        /// <summary>
        /// オプションを設定します。内部メソッドです。
        /// </summary>
        /// <returns>JsonSerializerOptions型のオプション</returns>
        private static JsonSerializerOptions GetOption()
        {
            // ユニコードのレンジ指定で日本語も正しく表示、インデントされるように指定
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true,
            };
            return options;
        }
    }
}
