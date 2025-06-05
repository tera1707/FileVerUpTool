using System.Collections.Generic;
using FileVerUpTool.Model;

namespace FileVerUpTool.Interface
{
    public interface IAdditionalInfoManager
    {
        void Save(string directoryPath, IEnumerable<WholeData> dataList);
        List<(string ProjFilePath, bool Visible, string Remark)> Load(string directoryPath);
    }
}
