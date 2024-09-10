using ApplicationNamespace;
using Autodesk.Revit.DB;
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
using System.Windows.Controls;

namespace app_EquipmentFindingZone
{
    public class EquipmentFindingZoneViewModel : INotifyPropertyChanged
    {
        private Workset _selectedWorkset;
        private Level _selectedLevel;
        private Phase _selectedPhase;
        private BuiltInCategory _selectedBuildInCategory;

        readonly string pathLogs = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\log.txt";

        Document doc = RevitAPI.Document;

        public EquipmentFindingZoneViewModel()
        {
            CollectWorksets(doc);
            CollectLevels(doc);
            CollectPhases(doc);

            OT_XC_write_zones_to_equipment = new RelayCommand(ADSK_Zone_by_Space, TypeCheckingInputs);
        }

        public RelayCommand OT_XC_write_zones_to_equipment { get; set; }

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
        public Phase SelectedPhase
        {
            get => _selectedPhase;
            set
            {
                _selectedPhase = value;
                OnPropertyChanged();
            }
        }        
        public BuiltInCategory SelectedBuildInCategory
        {
            get => _selectedBuildInCategory;
            set
            {
                _selectedBuildInCategory = value;
                OnPropertyChanged();
            }
        }

        public IList<Workset> Worksets { get; set; } = new List<Workset>();
        public IList<Level> Levels { get; set; } = new List<Level>();
        public IList<Phase> Phases { get; set; } = new List<Phase>();
        public IList<BuiltInCategory> BuiltInCategory_Categories { get; set; } = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_MechanicalEquipment
        };

        private void CollectWorksets(Document doc) {
            Worksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).Where(x => x.IsOpen && !x.Name.Contains("000")).ToList();
        }
        private void CollectLevels(Document doc)
        {
            Levels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).ToElements().Select(x => x as Level).ToList();
        }
        private void CollectPhases(Document doc) {
            PhaseArray phases = doc.Phases;
            foreach (Phase phase in phases) {
                Phases.Add(phase);
            }
        }

        private bool TypeCheckingInputs(object obj)
        {
            return SelectedWorkset != null
                && SelectedLevel != null
                && SelectedPhase != null
                && SelectedBuildInCategory != 0
                && (SelectedWorkset.Owner == null || SelectedWorkset.Owner == "");
                ;
        }
        
        public void ADSK_Zone_by_Space(object obj)
        {
            string p_ADSK_Zone = "ADSK_Зона";
            int countConvectorZoneChanges = 0;
            int countConvectorZoneNoChanges = 0;
            string convectorsNotWithZone = "";

            using (Transaction tr = new Transaction(doc, "CopyParameter"))
            {

                tr.Start();

                IList<Element> convectors = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
                        .WhereElementIsNotElementType().ToList()
                        .Where(x => ((x.WorksetId.IntegerValue == SelectedWorkset.Id.IntegerValue) 
                        && x.LevelId.IntegerValue == SelectedLevel.LevelId.IntegerValue)).ToList();

                IList<Element> spaces = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_MEPSpaces)
                        .WhereElementIsNotElementType()
                        .ToList();

                for (int i = 0; i < convectors.Count(); i++)
                {

                    XYZ convectorLocationPoint = (convectors[i].Location as LocationPoint).Point;
                    XYZ convectorLocationPointZ = new XYZ(convectorLocationPoint.X, convectorLocationPoint.Y, convectorLocationPoint.Z + 3.0);

                    try
                    {
                        Space spaceWhereConvector = doc.GetSpaceAtPoint(convectorLocationPointZ, SelectedPhase) as Space;
                        string spaceZone = spaceWhereConvector.LookupParameter(p_ADSK_Zone).AsString();

                        if (convectors[i].LookupParameter(p_ADSK_Zone).AsString() == spaceZone)
                        {
                            countConvectorZoneNoChanges++;
                        }
                        else
                        {
                            convectors[i].LookupParameter(p_ADSK_Zone).Set(spaceZone);
                            countConvectorZoneChanges++;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                tr.Commit();
                TaskDialog.Show("Result",
                    "Конвекторов без изменения параметра ADKS_Зона: " + countConvectorZoneNoChanges + Environment.NewLine +
                    "Конвекторов c изменением параметра ADKS_Зона: " + countConvectorZoneChanges + Environment.NewLine + convectorsNotWithZone) ;
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
