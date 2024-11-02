using ApplicationNamespace;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace app_MostLoadedBranch
{
    public class MostLoadedBranchViewModel : INotifyPropertyChanged
    {
        Document doc = RevitAPI.Document;
       
        readonly string _pathFolderLogs = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\app_MostLoadedBranch\";
        
        private string _pathToFolderForReading;
        private string _pathToFileForReading;
 
        private Level _selectedlevel;
        private Parameter _selectedParameter;
        private bool _isBusy;

        private string _status;
        private string _version;

        public MostLoadedBranchViewModel()
        {
            _isBusy = false;
            _version = "ver_241102_0.67_MID";
            _pathToFolderForReading = @"C:\User\Desktop";

            CollectLevels(doc);
            CollectElementParameters(doc);
            WriteParameter = new RelayCommand(FillMostLoadedParameter, TypeCheckingInputs);
            DialogShow = new RelayCommand(Dialog, x=> true);
        }

        public RelayCommand WriteParameter { get; set; }
        public RelayCommand DialogShow { get; set; }

        public string PathToFileForReading { 
            get => _pathToFileForReading;
            set
            {
                _pathToFileForReading = value;
                OnPropertyChanged();
            }
        }         
     
        public string Version { get => _version; }
        public string Status { 
            get => _status;
            set { 
                _status = value;
                OnPropertyChanged();
            }
        }
        public bool IsBusy { 
            get => _isBusy;
            set { 
                _isBusy=value;
                OnPropertyChanged();  
            } 
        }
        public Level SelectedLevel
        {
            get => _selectedlevel;
            set { 
                _selectedlevel = value;
                CollectElementParameters(doc);
                OnPropertyChanged();
            }
        }              
        public Parameter SelectedParameter
        {
            get => _selectedParameter;
            set {
                _selectedParameter = value;
                OnPropertyChanged();
            }
        }        

        public IList<Level> Levels { get; set; } = new List<Level>();
        public ObservableCollection<Parameter> SpaceParameters { get; set; } = new ObservableCollection<Parameter>();


        private void CollectLevels(Document doc)
        {
            Levels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels)
                .ToElements().Select(x => x as Level).ToList();
        }
        private void CollectElementParameters(Document doc)
        {
            Element element = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .Where(x => x != null
                    //&& x.LevelId.IntegerValue == SelectedLevel.Id.IntegerValue
                    && x.LookupParameter("MC Running Index Format") != null
                    && x.LookupParameter("MC Running Index 1") != null
                    && !x.Parameters.IsEmpty).First();

            SpaceParameters.Clear();

            ParameterSet parameterSet = element.Parameters;

            foreach (Parameter paramObj in parameterSet)
                    {
                        var parameter = (Parameter)paramObj;
                        if (!parameter.IsReadOnly && parameter.StorageType != StorageType.ElementId)
                        {
                            SpaceParameters.Add(parameter);
                        }
                    }          
            SpaceParameters.OrderBy(x => x.Definition.Name).ToList();
        }

        private bool TypeCheckingInputs(object obj)
        {
            return 
                PathToFileForReading != null
                && SelectedParameter != null
                ;
        }


        private void FillMostLoadedParameter(object obj)
        {
            string datelog_status = DateTime.Now.ToLocalTime().ToString("yyMMdd_ddd_HHmmss");
            string user_log = doc.Application.Username;
            string pathLog = _pathFolderLogs + $"app_MostLoadedBranch_log_{datelog_status}_{user_log}.txt";

            string[] lines = File.ReadAllLines(PathToFileForReading);
            List<string> list_fromTXT = new List<string>(); ;

            foreach (string line in lines)
            {
                // Разделяем строку по запятым и добавляем значения в список
                string[] parts = line.Split(',');
                list_fromTXT.AddRange(parts);
            }

            IList<Element> elements = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType().ToList()
                .Where(x => x != null
                    && x.LookupParameter("MC Running Index Format") != null
                    && x.LookupParameter("MC Running Index 1") != null).ToList();

            short ElementsChange = 0;
            short ElementsNOChange = 0;
            short ElementsWithProblems = 0;

            using (StreamWriter log = new StreamWriter(pathLog))
            {
                log.WriteLine("app used - " + "app_MostLoadedBranch" + Environment.NewLine +
                    "document used - " + doc.PathName + Environment.NewLine +
                    "user - " + doc.Application.Username + Environment.NewLine +
                    "date - " + DateTime.Now.ToLocalTime().ToString("yyMMdd_ddd_HHmmss"));

                using (Transaction tr = new Transaction(doc, "CopyParameter"))
                {
                    tr.Start();

                    foreach (var element in elements)
                    {
                        try
                        {
                            string elementParameterFormat = element.LookupParameter("MC Running Index Format").AsValueString();
                            string elementParameterIndex = element.LookupParameter("MC Running Index 1").AsValueString();

                            if (elementParameterFormat != null && elementParameterIndex != null)
                            {
                                element.LookupParameter(_selectedParameter.Definition.Name).Set(0);

                                if (list_fromTXT.Contains(elementParameterFormat + "-" + elementParameterIndex))
                                {
                                    element.LookupParameter(_selectedParameter.Definition.Name).Set(1);

                                    ElementsChange++;
                                }
                                else
                                {
                                    ElementsNOChange++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.WriteLine(element.Name+ " - " + ex.Message);
                            ElementsWithProblems++;
                        }
                    }
                    tr.Commit();
                }
            }
            Status = "Успех - " + datelog_status
                + Environment.NewLine + $"Элементов изменено - {ElementsChange} шт"
                + Environment.NewLine + $"Элементов не изменено - {ElementsNOChange} шт"
                + Environment.NewLine + $"Элементов с проблемами - {ElementsWithProblems} шт";
        }
        

        private void Dialog(object obj)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Выберите файл:";
            dlg.IsFolderPicker = false;
            dlg.InitialDirectory = _pathToFolderForReading;

            dlg.AddToMostRecentlyUsedList = true;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = _pathToFolderForReading;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;


            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                PathToFileForReading = dlg.FileName;
            }
        }



        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler HideRequest;
        private void RaiseHideRequest()
        {
            HideRequest?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ShowRequest;
        private void RaiseShowRequest()
        {
            ShowRequest?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
