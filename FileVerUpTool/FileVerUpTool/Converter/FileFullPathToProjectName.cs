using FileVerUpTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileVerUpTool.Converter
{
    public sealed class FileFullPathToProjectName : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var data = value as ModuleMetaData;

            var ext = System.IO.Path.GetExtension(data.FileFullPath);

            if (ext == ".csproj" || ext == ".rc")
            {
                return System.IO.Path.GetFileNameWithoutExtension(data.FileFullPath);
            }
            else
            {
                return data.ProjectName;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
