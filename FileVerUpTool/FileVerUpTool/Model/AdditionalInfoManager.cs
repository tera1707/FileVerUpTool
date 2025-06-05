using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FileVerUpTool.Interface;

namespace FileVerUpTool.Model
{
    public class AdditionalInfoManager : IAdditionalInfoManager
    {
        public void Save(string directoryPath, IEnumerable<WholeData> dataList)
        {
            var additionalInfoFilePath = Path.Combine(directoryPath, "AdditionalData.txt");
            var lines = dataList.Select(x =>
                $"{x.ProjFilePath},{x.Additional.Visible},{x.Additional.Remark}");
            File.WriteAllText(additionalInfoFilePath, string.Join("\r\n", lines), new UTF8Encoding(true));
        }

        public List<(string ProjFilePath, bool Visible, string Remark)> Load(string directoryPath)
        {
            var additionalInfoFilePath = Path.Combine(directoryPath, "AdditionalData.txt");
            var result = new List<(string, bool, string)>();

            if (File.Exists(additionalInfoFilePath))
            {
                foreach (var line in File.ReadAllLines(additionalInfoFilePath))
                {
                    var s = line.Split(',');
                    if (s.Length >= 3)
                    {
                        result.Add((s[0], bool.Parse(s[1]), s[2]));
                    }
                }
            }
            return result;
        }
    }
}
