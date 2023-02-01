using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileVerUpTool.Model
{
    public class ModuleMetaData
    {
        public string FileFullPath { get; set; }
        public string ProjectName { get; set; }//.NET Framework用
        public string Version { get; set; }
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
        public string Company { get; set; }
        public string Product { get; set; }
        public string Copyright { get; set; }
        public string Description { get; set; }
        public string NeutralLanguage { get; set; }
        public ModuleMetaData(string fileFullPath, string version, string assemblyVersion, string fileVersion,
                                string company, string product, string copyRight, string description, string neutralLanguage, string projectName = "")
        {
            FileFullPath = fileFullPath;
            Version = version;
            AssemblyVersion = assemblyVersion;
            FileVersion = fileVersion;
            Company = company;
            Product = product;
            Copyright = copyRight;
            Description = description;
            NeutralLanguage = neutralLanguage;
            ProjectName = projectName;
        }
    }
}
