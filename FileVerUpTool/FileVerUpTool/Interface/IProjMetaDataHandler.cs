using FileVerUpTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileVerUpTool.Interface
{
    public interface IProjMetaDataHandler
    {
        ModuleMetaData? Read(string csprojPath);
        void Write(string projFilePath, ModuleMetaData data);
    }
}