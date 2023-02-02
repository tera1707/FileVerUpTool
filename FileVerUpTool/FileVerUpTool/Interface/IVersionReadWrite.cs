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
        Task<ObservableCollection<ModuleMetaData>> Read(string targetDir);
        Task Write(ObservableCollection<ModuleMetaData> list);
        ObservableCollection<ModuleMetaData> BulkSetOne(ObservableCollection<ModuleMetaData> currentList, string propName, string val);
    }
}
