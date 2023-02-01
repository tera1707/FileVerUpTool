using FileVerUpTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileVerUpTool.Interface
{
    internal interface IProjMetaDataHandler
    {
        ModuleMetaData? Read(string csprojPath);
        void Write(ModuleMetaData data);
    }
}
