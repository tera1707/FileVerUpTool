using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileVerUpTool.Interface
{
    public interface ISearchSpecifiedExtFile
    {
        List<string> Search(string targetDirectoryFullPath, string targetExt);
    }
}
