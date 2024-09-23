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

namespace app_SpaceHeatLossesFill
{
    public class SpaceHeatLossesFillViewModel : INotifyPropertyChanged
    {
        Document doc = RevitAPI.Document;
       
        readonly string _pathFolderLogs = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\app_SpaceHeatLossesFill\";
        
        private string _pathToFolderForReading;
        private string _pathToFileForReading;
        private char _csvSeparator; 
        private Level _selectedlevel;
        private Parameter _selectedParameterSpace;
        private bool _isBusy;

        private string _status;
        private string _version;

        public SpaceHeatLossesFillViewModel()
        {
            _isBusy = false;
            _version = "ver_240923_0.60_MID";
            _pathToFolderForReading = @"C:\User\Desktop";
            _csvSeparator = ','; 

            CollectLevels(doc);

            WriteParameterToSpace = new RelayCommand(FillSpaceHeatLooses, TypeCheckingInputs);
            DialogShow = new RelayCommand(Dialog, x=> true);
        }

        public RelayCommand WriteParameterToSpace { get; set; }
        public RelayCommand DialogShow { get; set; }

        public string PathToFileForReading { 
            get => _pathToFileForReading;
            set
            {
                _pathToFileForReading = value;
                OnPropertyChanged();
            }
        }         
        public char CsvSeparator
        { 
            get => _csvSeparator;
            set
            {
                _csvSeparator = value;
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
                CollectSpaceParameters(doc);
                OnPropertyChanged();
            }
        }              
        public Parameter SelectedParameterSpace
        {
            get => _selectedParameterSpace;
            set {
                _selectedParameterSpace = value;
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
        private void CollectSpaceParameters(Document doc)
        {
            Element element = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MEPSpaces)
                .WhereElementIsNotElementType()
                .Where(x => x != null 
                    && x.LevelId.IntegerValue == SelectedLevel.LevelId.IntegerValue
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
            return SelectedLevel != null
                && PathToFileForReading != null
                && SelectedParameterSpace != null
                ;
        }


        private void FillSpaceHeatLooses(object obj)
        {
            string datelog_status = DateTime.Now.ToLocalTime().ToString("yyMMdd_ddd_HHmmss");
            string user_log = doc.Application.Username;
            string pathLog = _pathFolderLogs + $"app_SpaceHeatLossesFill_log_{datelog_status}_{user_log}.txt";

            Dictionary<string, string> spaceHeatLosses = File.ReadLines(PathToFileForReading)
                .Select(line => line.Split(CsvSeparator))
                .ToDictionary(parts => parts[1].Trim(), parts => parts[2].Trim());

            IList<Element> spaces = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_MEPSpaces)
                .WhereElementIsNotElementType().ToList()
                .Where(x => x.LevelId.IntegerValue == SelectedLevel.Id.IntegerValue)
                .ToList();

            short SpacesChange = 0;
            short SpacesNOChange = 0;
            short SpacesWithProblems = 0;

            using (StreamWriter log = new StreamWriter(pathLog))
            {
                log.WriteLine("app used - " + "app_SpaceHeatLossesFill" + Environment.NewLine +
                    "document used - " + doc.PathName + Environment.NewLine +
                    "user - " + doc.Application.Username + Environment.NewLine +
                    "date - " + DateTime.Now.ToLocalTime().ToString("yyMMdd_ddd_HHmmss"));

                using (Transaction tr = new Transaction(doc, "CopyParameter"))
                {
                    tr.Start();

                    // TODO - fill all spaces to zeros 0 before writing new values
                    foreach (var space in spaces)
                    {
                        try
                        {
                            double  heatLoosesWas = space.LookupParameter(SelectedParameterSpace.Definition.Name).AsDouble();
                            double heatLooses = UnitUtils.ConvertToInternalUnits(Int32.Parse(spaceHeatLosses[space.LookupParameter("Number").AsString()]), UnitTypeId.Watts);

                            if  (heatLoosesWas != heatLooses)
                            {
                                space.LookupParameter(SelectedParameterSpace.Definition.Name)
                                    .Set(UnitUtils.ConvertToInternalUnits(heatLooses, UnitTypeId.Watts));
                                
                                SpacesChange++;
                                log.WriteLine(space.Name + "_" + heatLoosesWas + " - " +heatLooses);
                            } else
                            {
                                SpacesNOChange++;
                            }

                        }
                        catch (Exception ex)
                        {
                            log.WriteLine(space.Name+ " - " + ex.Message);
                            SpacesWithProblems++;
                        }
                    }
                    tr.Commit();
                }
            }
            Status = "Успех - " + datelog_status
                + Environment.NewLine + $"Пространств изменено - {SpacesChange} шт"
                + Environment.NewLine + $"Пространств не изменено - {SpacesNOChange} шт"
                + Environment.NewLine + $"Пространств с проблемами - {SpacesWithProblems} шт";
        }
        

        private void Dialog(object obj)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Выберите файл csv:";
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
