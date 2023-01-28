using CommunityToolkit.WinUI.UI.Controls;
using CommunityToolkit.WinUI.UI.Controls.Primitives;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Windows.System;
using Windows.UI.Popups;


/*
�����������Ƃ���
�E.netFW ���X�f�[�^���Ȃ����̂ɑ΂��Đݒ肵�悤�Ƃ��Ă��A�ݒ�ł��Ȃ��B(�u��������̂��Ȃ����炾�Ǝv��)

���v��������
�E,net6�Ȃ̂��AFW�Ȃ̂��̐؂�ւ��̔�����@�����Ă��Ƃ��B
�@����t�@�C����csproj�Ȃ�.net6�A�����łȂ����(AssemblyInfo.cs�Ȃ�H)FW�ɂ��Ă邪����ł����̂��B�iC++���݂����Ƃ��ɂ��܂�Ȃ����H�j
�EFW�̂��߂�AssemblyInfo.cs��T�����A���������t�@�C�����A���ʂɂ��肻���B
�@AssemblyInfo.cs���݂āA����ɂ��̂Ȃ��̂Ȃɂ����݂āA�����ƃo�[�W���������܂񂾂��̂Ȃ̂��𔻒肵���ق��������H
�@(�Ⴆ�΁A�Œ�AAssemblyVersion��AssemblyFileVersion�͂���͂������炻��2������ΐ������A�Ƃ���Ƃ��B)

����肽������
�EAuthor�����������B�������ۂ��B

*/
namespace FileVerUpTool
{
    public sealed partial class MainWindow : Window
    {
        string targetExt = "*.csproj";

        public MainWindow()
        {
            this.InitializeComponent();

            // DEBUG
            TargetDirBox.Text = @"C:\Users\masa\source\repos\FlyoutableToggleButtonJikken";
        }

        public ObservableCollection<ModuleMetaData> DataList { get; set; } = new ObservableCollection<ModuleMetaData>();

        // Read�{�^��������
        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            // ��ʕ\������U�N���A
            DataList.Clear();

            var targetDir = TargetDirBox.Text;

            // �G���[�F�t�H���_���w�肳��Ă��Ȃ�
            if (string.IsNullOrEmpty(targetDir))
                return;

            // �G���[�F�w��̃t�H���_�����݂��Ȃ�
            if (!Directory.Exists(targetDir))
                return;

            //////////////////////////////
            // sdk�^�C�v��csproj������
            //////////////////////////////

            // �w��t�H���_�ȉ���csproj�t�@�C��������
            var ssef = new SearchSpecifiedExtFile(targetDir, targetExt);
            var foundList = ssef.Search();

            // �G���[�F�w��̃t�H���_�̒���csproj�t�@�C����������Ȃ�����
            if (foundList.Count != 0)
            {
                // ���������z��\������
                foundList.ForEach(x =>
                {
                    // �{�����
                    var reader = new SdkTypeCsprojHandler();
                    var data = reader.Read(x);

                    if (data != null)
                        DataList.Add(data);
                });
            }

            //////////////////////////////
            // .netFramework��AssemblyInfo.cs������
            //////////////////////////////

            // �w��t�H���_�ȉ���csproj�t�@�C��������
            var ssef2 = new SearchSpecifiedExtFile(targetDir, "AssemblyInfo.cs");
            var foundList2 = ssef2.Search();

            if (foundList2.Count != 0)
            {
                foundList2.ForEach(x =>
                {
                    var reader = new DotnetFrameworkProjHandler();
                    var data = reader.Read(x);

                    if (data != null)
                        DataList.Add(data);
                });
            }

        }


        // �������݃{�^��
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var data in DataList)
            {
                if (System.IO.Path.GetExtension(data.FileFullPath) == ".csproj")
                {
                    var writer = new SdkTypeCsprojHandler();
                    writer.Write(data);
                }
                else
                {
                    var writer = new DotnetFrameworkProjHandler();
                    writer.Write(data);
                }
            }
        }

        // �ꊇ����
        private void IkkatsuButton_Click(object sender, RoutedEventArgs e)
        {
            var ikkatsuType = IkkatsuType.SelectedValue as string;
            var val = IkkatsuValue.Text;

            setvalue(ikkatsuType, val);

            // �w��̍��ڂ��ꊇ�ݒ������
            // ItemsSource�ɓ��ꂽ���X�g���X�V���ꂽ�Ƃ��ɁA��ʂ̃��X�g���X�V���Ă����DataGrid.Refresh()�݂����ȃ��\�b�h��
            // ���������̂ŁA�������R�s�[�������ē���ւ��Ă�B
            void setvalue(string propName, string val)
            {
                ObservableCollection<ModuleMetaData> tmp = new ObservableCollection<ModuleMetaData>();

                // �������R�s�[������
                for (int i = 0; i < DataList.Count; i++)
                {
                    tmp.Add(new ModuleMetaData(DataList[i].FileFullPath, DataList[i].Version, DataList[i].AssemblyVersion, DataList[i].Authors, DataList[i].Company,
                                                DataList[i].Product, DataList[i].Copyright, DataList[i].Description, DataList[i].NeutralLanguage));
                }

                // �ꊇ�ݒ肷����̂����A�������Scsproj������Ă�
                for (int i = 0; i < DataList.Count; i++)
                {
                    typeof(ModuleMetaData).GetProperty(propName).SetValue(tmp[i], val);
                }

                // ����ւ�
                CsprojDataGrid.ItemsSource = tmp;
            }

        }

        // csproj��ǂݏ�������N���X
        public interface IProjMetaDataHandler
        {
            ModuleMetaData? Read(string csprojPath);
            void Write(ModuleMetaData data);
        }

        public class DotnetFrameworkProjHandler : IProjMetaDataHandler
        {
            public ModuleMetaData Read(string AssemblyInfoPath)
            {
                //var metadata = new ModuleMetaData();
                string pjName = string.Empty;
                string version = string.Empty;
                string assemblyVersion = string.Empty;
                string authors = string.Empty;
                string company = string.Empty;
                string product = string.Empty;
                string copyright = string.Empty;
                string description = string.Empty;
                string neutralLanguage = string.Empty;

                // �܂��Acsproj��ǂݍ��݁A���g�̑S�e�L�X�g��ۑ��i������u�����Ă����j
                var lines = File.ReadAllLines(AssemblyInfoPath, System.Text.Encoding.UTF8);

                foreach (var line in lines)
                {
                    if (line.Contains("AssemblyTitle")) pjName = line.Split("\"")[1];
                    if (line.Contains("AssemblyVersion")) version = line.Split("\"")[1];
                    //if (line.Contains("AssemblyVersion")) assemblyVersion = line.Split("\"")[1];
                    if (line.Contains("AssemblyAuthors")) authors = line.Split("\"")[1];
                    if (line.Contains("AssemblyCompany")) company = line.Split("\"")[1];
                    if (line.Contains("AssemblyProduct")) product = line.Split("\"")[1];
                    if (line.Contains("AssemblyCopyright")) copyright = line.Split("\"")[1];
                    if (line.Contains("AssemblyDescription")) description = line.Split("\"")[1];
                    if (line.Contains("NeutralResourcesLanguage")) neutralLanguage = line.Split("\"")[1];
                }

                return new ModuleMetaData(AssemblyInfoPath, version, assemblyVersion, authors, company, product, copyright, description, neutralLanguage);

            }

            public void Write(ModuleMetaData data)
            {
                var expBase = "(^|(?<=\r\n))\\[assembly: ";
                var expBase2 = "\\(\".*\"\\)\\]";

                var all = File.ReadAllText(data.FileFullPath, System.Text.Encoding.UTF8);

                all = Regex.Replace(all, expBase + "AssemblyVersion" + expBase2, "[assembly: AssemblyVersion(\"" + data.Version + "\")]");
                all = Regex.Replace(all, expBase + "AssemblyCompany" + expBase2, "[assembly: AssemblyCompany(\"" + data.Company + "\")]");
                all = Regex.Replace(all, expBase + "AssemblyProduct" + expBase2, "[assembly: AssemblyProduct(\"" + data.Product + "\")]");
                all = Regex.Replace(all, expBase + "AssemblyCopyright" + expBase2, "[assembly: AssemblyCopyright(\"" + data.Copyright + "\")]");
                all = Regex.Replace(all, expBase + "AssemblyDescription" + expBase2, "[assembly: AssemblyDescription(\"" + data.Description + "\")]");
                all = Regex.Replace(all, expBase + "NeutralResourcesLanguage" + expBase2, "[assembly: NeutralResourcesLanguage(\"" + data.NeutralLanguage + "\")]");

                File.WriteAllText(data.FileFullPath, all);
            }
        }

        // csproj��ǂݏ�������N���X
        public class SdkTypeCsprojHandler : IProjMetaDataHandler
        {
            public SdkTypeCsprojHandler() { }

            public ModuleMetaData? Read(string  csprojPath)
            {
                XElement xml = XElement.Load(csprojPath);

                // �~�����z�������
                //#region �~�����z�������
                var infos = xml.Elements("PropertyGroup");

                if (infos is null)
                    return null;

                var pg = infos.FirstOrDefault();

                if (pg is null)
                    return null;

                var ver = ReturnNullIfThrowException(() => pg.Elements("Version"));
                var aver = ReturnNullIfThrowException(() => pg.Elements("AssemblyVersion"));
                var author = ReturnNullIfThrowException(() => pg.Elements("Authors"));
                var company = ReturnNullIfThrowException(() => pg.Elements("Company"));
                var product = ReturnNullIfThrowException(() => pg.Elements("Product"));
                var description = ReturnNullIfThrowException(() => pg.Elements("Description"));
                var copyright = ReturnNullIfThrowException(() => pg.Elements("Copyright"));
                var newtralLang = ReturnNullIfThrowException(() => pg.Elements("NeutralLanguage"));

                Debug.WriteLine($"{csprojPath}, {ver}, {aver}, {author}, {company}, {product}, {description}, {copyright}, {newtralLang}");

                return new ModuleMetaData(csprojPath, ver?.ToString(), aver?.ToString(), author?.ToString(), company?.ToString(),
                                            product?.ToString(), copyright?.ToString(), description?.ToString(), newtralLang?.ToString());
            }

            public void Write(ModuleMetaData data)
            {
                // �܂��Acsproj��ǂݍ��݁A���g�̑S�e�L�X�g��ۑ��i������u�����Ă����j
                var all = File.ReadAllText(data.FileFullPath, System.Text.Encoding.UTF8);

                // ����������f�[�^��p�Ӂi���j
                var input = new Collection<(string, string)>()
                {
                    ("Version", data.Version),
                    ("AssemblyVersion", data.AssemblyVersion),
                    ("Authors", data.Authors),
                    ("Company", data.Company),
                    ("Product", data.Product),
                    ("Description", data.Description),
                    ("Copyright", data.Copyright),
                    ("NeutralLanguage", data.NeutralLanguage),
                };

                // �u�����{
                all = SetValueForSpecifiedKey(all, input);

                File.WriteAllText(data.FileFullPath, all);
            }

            private string ReturnNullIfThrowException(Func<IEnumerable<XElement>> getElementFunc)
            {
                string ret = null;

                try
                {
                    ret = getElementFunc.Invoke().FirstOrDefault().Value;
                }
                catch (NullReferenceException)
                {
                    ret = null;
                }

                return ret;
            }

            private string SetValueForSpecifiedKey(string all, IReadOnlyCollection<(string ElementName, string Value)> input)
            {
                //var all = File.ReadAllText(x, System.Text.Encoding.UTF8);

                foreach (var element in input)
                {
                    if (string.IsNullOrEmpty(element.Value))
                        continue;

                    // �w��̒l�Œu��������
                    //var pattern = "\\<Version\\>.*\\<\\/Version\\>";
                    var pattern = "\\<" + element.ElementName + "\\>.*\\<\\/" + element.ElementName + "\\>";

                    if (!Regex.IsMatch(all, pattern))
                    {
                        // �w���element���������
                        // �Ƃ肠�����A�ŏ��Ɍ�����PropertyGroup�̍Ō��Element�Ƃ��āA�������w���Element�����Ă��
                        var temp = new Regex("</PropertyGroup>");
                        var rep = "  <" + element.ElementName + "></" + element.ElementName + ">\r\n  </PropertyGroup>";
                        all = temp.Replace(all, rep, 1);
                    }

                    // �w��̒l�Œu������
                    var replace = "<" + element.ElementName + ">" + element.Value + "</" + element.ElementName + ">";//��
                    all = Regex.Replace(all, pattern, replace);
                }
                return all;
            }
        }


    }

    public class ModuleMetaData 
    {
        public string FileFullPath { get; set; }
        public string Version { get; set; }
        public string AssemblyVersion { get; set; }
        public string Authors { get; set; }
        public string Company { get; set; }
        public string Product { get; set; }
        public string Copyright { get; set; }
        public string Description { get; set; }
        public string NeutralLanguage { get; set; }
        public ModuleMetaData(string fileFullPath, string version, string assemblyVersion, string authors,
                                string company, string product, string copyRight, string description, string neutralLanguage)
        {
            FileFullPath = fileFullPath;
            Version = version;
            AssemblyVersion = assemblyVersion;
            Authors = authors;
            Company = company;
            Product = product;
            Copyright = copyRight;
            Description = description;
            NeutralLanguage = neutralLanguage;
        }
    }

    // �w��t�H���_����w��g���q�̃t�@�C����T����List�ɂ���N���X
    public class SearchSpecifiedExtFile
    {
        private string _targetDirectoryFullPath = string.Empty;
        private string _targetExt = "*.csproj";
        private List<string> foundList = new();

        private SearchSpecifiedExtFile() { }

        public SearchSpecifiedExtFile(string targetDirectoryFullPath, string targetExt)
        {
            _targetDirectoryFullPath = targetDirectoryFullPath;
            _targetExt = targetExt;
        }

        public List<string> Search()
        {
            Search(_targetDirectoryFullPath);
            return foundList;
        }

        private void Search(string target)
        {
            var parentDir = new DirectoryInfo(target);
            var files = parentDir.GetFiles(_targetExt);

            // ���̊K�w�ɂ���Ώۃt�@�C�������X�g�ɓ����
            files.ToList().ForEach(x => foundList.Add(x.FullName));

            // ���̊K�w�ɂ���t�H���_��T���A
            var childDirs = parentDir.GetDirectories();
            // �t�H���_�̒���T���ɍs��(�ċA�I��)
            foreach (var dir in childDirs)
            {
                Search(dir.FullName);
            }
        }
    }
    //private static string FileFullPathToProjectNam(string fullPath)
    //{
    //    return System.IO.Path.GetFileNameWithoutExtension(fullPath);
    //}
    public sealed class FileFullPathToProjectName : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var fullPath = value as string;
            return System.IO.Path.GetFileNameWithoutExtension(fullPath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
