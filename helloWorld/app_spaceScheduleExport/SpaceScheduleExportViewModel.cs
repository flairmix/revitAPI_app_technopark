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
        private Workset _selectedWorkset;
        private Level _selectedLevel;
        readonly string _version = "v2024.0.40_MID";
        readonly string _folderPath;


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

        //private string _busyText;
        ////Busy Text Content
        //public string BusyText
        //{
        //    get { return _busyText; }
        //    set
        //    {
        //        _busyText = value;
        //        OnPropertyChanged();
        //    }
        //}

        readonly string folderPath = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\";

        Document doc = RevitAPI.Document;

        public SpaceScheduleExportViewModel(string pathFolderForSave)
        {
            CollectWorksets(doc);
            CollectLevels(doc);
            IsBusy = false;

            _folderPath = pathFolderForSave;

            ExportSpaceWithInfo_relay = new RelayCommand(ExportSpaceWithInfo, ExportSpaceWithInfo_CanExecute);
        }

        public RelayCommand ExportSpaceWithInfo_relay { get; set; }

        public string Version
        {
            get => _version;
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

            string datelog = DateTime.Now.ToLocalTime().ToString("yyyyMMdd_HHmmss");
            string pathlog = folderPath + datelog + "log.txt";
            string pathOutputFile = _folderPath + @"\" + SelectedLevel.Name + "_MID_SpaceLoadsScheduleHeating.csv";

            IList<string> columnsNamesSpace = new List<string>() {
            "Level", "ADSK_Зона", "Name", "Number", "Area", "Room: Number",
            "Room: Name", "ADSK_Температура в помещении",
            "ADSK_Теплопотери", "ADSK_Тепловая мощность"};

            using (StreamWriter log = new StreamWriter(pathlog))
            {
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
                            log.Write(space.Name + e);
                        }

                        outputFile.WriteLine(lineResult);


                    }
                }
            }
        }

        public bool ExportSpaceWithInfo_CanExecute(object obj)
        {
            return SelectedWorkset != null && SelectedLevel != null;
             
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
