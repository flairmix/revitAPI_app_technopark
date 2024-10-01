using ApplicationNamespace;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace app_spaceScheduleExport
{
    public class SpaceScheduleExportViewModel : INotifyPropertyChanged
    {
        readonly string _pathLogsFolder = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\app_spaceScheduleExport\";
        private Workset _selectedWorkset;
        private Level _selectedLevel;
        private string _version ;
        string _folderPath;
        string _status;


        private bool _isBusy;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }


        Document doc = RevitAPI.Document;

        public SpaceScheduleExportViewModel ( )
        {
            CollectWorksets(doc);
            CollectLevels(doc);
            IsBusy = false;
            _version = "ver_240918_0.60_MID";

            _folderPath = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs";

            ExportSpaceWithInfo_relay = new RelayCommand(ExportSpaceWithInfo, ExportSpaceWithInfo_CanExecute);
            Dialog_Command = new RelayCommand(Dialog, x=>true);

        }

        public RelayCommand ExportSpaceWithInfo_relay { get; set; }
        public RelayCommand Dialog_Command { get; set; }

        public string Folder
        {
            get => _folderPath;
            set
            {
                _folderPath = value;
                OnPropertyChanged();
            }
        }   
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }        
        public Workset SelectedWorkset
        {
            get => _selectedWorkset;
            set
            {
                _selectedWorkset = value;
                OnPropertyChanged();
            }
        }
        public Level SelectedLevel
        {
            get => _selectedLevel;
            set
            {
                _selectedLevel = value;
                OnPropertyChanged();
            }
        }
        public string Version
        {
            get => _version;
        }


        public IList<Workset> Worksets { get; set; } = new List<Workset>();
        public IList<Level> Levels { get; set; } = new List<Level>();


        private void CollectWorksets(Document doc) {
            Worksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).Where(x => x.IsOpen).ToList();
        }
        private void CollectLevels(Document doc)
        {
            Levels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).ToElements().Select(x => x as Level).ToList();
        }

        public void ExportSpaceWithInfo(object obj)
        {
            string datelog = DateTime.Now.ToLocalTime().ToString("yyMMdd_HHmmss");
            string user_log = doc.Application.Username;
            string pathlog = _pathLogsFolder + $"app_spaceScheduleExport{datelog}_{user_log}_log.txt";

            string pathOutputFile = Folder + @"\" + SelectedLevel.Name + "_MID_SpaceLoadsScheduleHeating.csv";

            IList<string> columnsNamesSpace = new List<string>() {
                "Level", "ADSK_Зона", "Name", "Number", "Area", "Room: Number",
                "Room: Name", "ADSK_Температура в помещении",
                "ADSK_Теплопотери", "ADSK_Тепловая мощность"};

            using (StreamWriter log = new StreamWriter(pathlog))
            {
                log.WriteLine("app used - " + "app_spaceScheduleExport" + Environment.NewLine +
                    "document used - " + doc.PathName + Environment.NewLine +
                    "user - " + doc.Application.Username + Environment.NewLine +
                    "date - " + DateTime.Now.ToLocalTime().ToString("yyMMdd_ddd_HHmmss"));
                
                using (StreamWriter outputFile = new StreamWriter(pathOutputFile))
                {
                    IList<Element> spaces = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_MEPSpaces)
                        .WhereElementIsNotElementType()
                        .Where(x => (x.LevelId.IntegerValue == SelectedLevel.Id.IntegerValue) &&
                                    x.WorksetId.IntegerValue == SelectedWorkset.Id.IntegerValue &&
                                    x.LookupParameter("ADSK_Зона").ToString().Length > 2 &&
                                    UnitUtils.ConvertFromInternalUnits(x.LookupParameter("ADSK_Температура в помещении").AsDouble(), UnitTypeId.Celsius) > -100 &&
                                    UnitUtils.ConvertFromInternalUnits(x.LookupParameter("Area").AsDouble(), UnitTypeId.SquareMeters) > 0
                                    )
                                    .ToList();

                    outputFile.WriteLine("Level,ADSK_Зона,Name,Number,Area,Room: Number,Room: Name,ADSK_Температура в помещении");

                    foreach (Element space in spaces)
                    {
                        Space tempSpace = space as Space;

                        string lineResult = "";

                        try
                        {
                            lineResult += (tempSpace.Level.Name) + ",";
                            lineResult += (tempSpace.LookupParameter("ADSK_Зона").AsValueString()) + ",";
                            lineResult += (tempSpace.LookupParameter("Name").AsValueString()) + ",";
                            lineResult += (tempSpace.LookupParameter("Number").AsValueString()) + ",";
                            lineResult += (tempSpace.LookupParameter("Area").AsValueString().Replace(",", ".")) + ",";
                            lineResult += (tempSpace.get_Parameter(BuiltInParameter.SPACE_ASSOC_ROOM_NUMBER).AsString()) + ",";
                            lineResult += (tempSpace.get_Parameter(BuiltInParameter.SPACE_ASSOC_ROOM_NAME).AsString()) + ",";
                            lineResult += (tempSpace.LookupParameter("ADSK_Температура в помещении").AsValueString());

                        }
                        catch (Exception e)
                        {
                            log.Write(space.Name + e.Message);
                        }

                        outputFile.WriteLine(lineResult);
                    }
                }
            }
            Status = "Успех - " + datelog + Environment.NewLine + pathOutputFile;
        }

        public bool ExportSpaceWithInfo_CanExecute(object obj)
        {
            return SelectedWorkset != null 
                && SelectedLevel != null
                && Folder != null;
        }


        private void Dialog(object obj)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Выберите место сохранения файла:";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = _folderPath;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = _folderPath;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Folder = dlg.FileName;
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
