using FileVerUpTool.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileVerUpTool.Interface
{
    public interface IVersionReadWrite
    {
        Task<List<(string ProjDataFilePath, ModuleMetaData Module)>> Read(string targetDir);
        Task Write(List<(string ProjDataFilePath, ModuleMetaData Module)> list);
        //List<ModuleMetaData> BulkSetOne(List<ModuleMetaData> currentList, string propName, string val);
    }
}
