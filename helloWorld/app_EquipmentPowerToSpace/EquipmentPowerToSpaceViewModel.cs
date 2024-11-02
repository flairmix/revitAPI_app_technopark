using ApplicationNamespace;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;


namespace app_EquipmentPowerToSpace
{
    public class EquipmentPowerToSpaceViewModel : INotifyPropertyChanged
    {
        private Workset _selectedWorkset;
        private Level _selectedLevel;
        private Phase _selectedPhase;
        private BuiltInCategory _selectedBuildInCategory;
        private Parameter _selectedParameter;
        private Parameter _selectedParameterSpace;
        string _status;
        private string _version = "ver_240926_0.62_MID";
        private Reference _reference;

        readonly string _pathFolderLogs = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\log_EquipmentPowerToSpace\";

        Document doc = RevitAPI.Document;

        public EquipmentPowerToSpaceViewModel()
        {
            CollectWorksets(doc);
            CollectLevels(doc);
            CollectPhases(doc);
            CollectParametersSpaces();

            WriteConvectorsPowerToSpace = new RelayCommand(Write_DoubleParameter_to_space_cumulatively, TypeCheckingInputs);
            GetElement = new RelayCommand(GetReference, x=>true);
        }


        public RelayCommand WriteConvectorsPowerToSpace { get; set; }
        public RelayCommand GetElement { get; set; }

        public Workset SelectedWorkset
        {
            get => _selectedWorkset;
            set
            {
                _selectedWorkset = value;
                OnPropertyChanged();
            }
        }        
        public Reference ReferenceElement
        {
            get => _reference;
            set
            {
                _reference = value;
                CollectParametersEquip(value);
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
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                OnPropertyChanged();
            }
        }

        public IList<Workset> Worksets { get; set; } = new List<Workset>();
        public IList<Level> Levels { get; set; } = new List<Level>();
        public IList<Phase> Phases { get; set; } = new List<Phase>();
        public ObservableCollection<Parameter> Parameters { get; set; } = new ObservableCollection<Parameter>();

        public IList<Parameter> ParametersSpace { get; set; } = new List<Parameter>();
        public IList<BuiltInCategory> BuiltInCategory_Categories { get; set; } = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_MechanicalEquipment
        };
        public Parameter SelectedParameter
        {
            get => _selectedParameter;
            set
            {
                _selectedParameter = value;
                OnPropertyChanged();
            }
        }        
        public Parameter SelectedParameterSpace
        {
            get => _selectedParameterSpace;
            set
            {
                _selectedParameterSpace = value;

                OnPropertyChanged();
            }
        }

        private void CollectWorksets(Document doc) {
            Worksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset)
                .Where(x => x.IsOpen && !x.Name.Contains("000")).ToList();
        }

        private void CollectLevels(Document doc)
        {
            Levels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels)
                .ToElements().Select(x => x as Level).ToList();
            Levels.OrderBy(x => x.Name);
        }
        private void CollectPhases(Document doc) {
            PhaseArray phases = doc.Phases;
            foreach (Phase phase in phases) {
                Phases.Add(phase);
            }
        }
        private void CollectParametersEquip(Reference reference)
        {
            var element = RevitAPI.Document.GetElement(reference);
            ParameterSet parameterSet = element.Parameters;
            Parameters.Clear(); 

            foreach (Parameter paramObj in parameterSet)
            {
                var parameter = (Parameter)paramObj;
                if (!parameter.IsReadOnly 
                    && parameter.StorageType != StorageType.ElementId
                    && (parameter.Definition.Name.Contains("ADSK") || parameter.Definition.Name.Contains("atp-tlp"))
                    )
                {
                    Parameters.Add(parameter);
                }
            }
            Parameters.OrderBy(x => x.Definition.Name).ToList();

        }
        private void CollectParametersSpaces()
        {
            ParameterSet parameterSet = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_MEPSpaces).FirstElement().Parameters;

            foreach (Parameter paramObj in parameterSet)
            {
                var parameter = (Parameter)paramObj;
                if (!parameter.IsReadOnly 
                    && parameter.StorageType != StorageType.ElementId
                    && (parameter.Definition.Name.Contains("ADSK") || parameter.Definition.Name.Contains("atp-tlp"))
                    )
                    
                {
                    ParametersSpace.Add(parameter);
                }
            }
            ParametersSpace.OrderBy(x => x.Definition.Name);
        }


        private bool TypeCheckingInputs(object obj)
        {
            return ReferenceElement != null 
                && SelectedWorkset != null
                && SelectedLevel != null
                && SelectedPhase != null
                && (SelectedParameter != null && Parameters.Contains(SelectedParameter))
                && (SelectedParameterSpace != null && ParametersSpace.Contains(SelectedParameterSpace))
                && SelectedBuildInCategory != 0;
        }

        public void GetReference(object obj)
        {
            RaiseHideRequest();
            ReferenceElement = RevitAPI.UIDocument.Selection
                .PickObject(ObjectType.Element, "Выберете элемент для сбора параметров");
            RaiseShowRequest();
        }

        public void Write_DoubleParameter_to_space_cumulatively(object obj)
        {

            string datelog_status = DateTime.Now.ToLocalTime().ToString("yyMMdd_ddd_HHmmss");
            string user_log = doc.Application.Username;
            string pathLog = _pathFolderLogs + $"log_EquipmentPowerToSpace_{datelog_status}_{user_log}_log.txt";

            int convectorsCount = 0;
            int convectorsLostCount = 0;

            using (StreamWriter log = new StreamWriter(pathLog))
            {
                using (Transaction tr = new Transaction(doc, "CopyParameter"))
                {
                    tr.Start();

                    IList<Element> convectors = new FilteredElementCollector(doc)
                        .OfCategory(SelectedBuildInCategory)
                        .WhereElementIsNotElementType().ToList()
                        .Where(x => x.WorksetId.IntegerValue == SelectedWorkset.Id.IntegerValue 
                            && x.LevelId.IntegerValue == SelectedLevel.Id.IntegerValue)
                        .ToList();
                    
                    IList<Element> spaces = new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_MEPSpaces)
                            .WhereElementIsNotElementType()
                            .Where(x => x.LevelId.IntegerValue == SelectedLevel.Id.IntegerValue)
                            .ToList();

                    log.WriteLine("app used - " + "app_EquipmentPowerToSpace" + Environment.NewLine +
                                    "document used - " + doc.PathName + Environment.NewLine +
                                    "user - " + doc.Application.Username + Environment.NewLine +
                                    "date - " + DateTime.Now.ToLocalTime().ToString("yyMMdd_ddd_HHmmss"));

                    log.WriteLine("Конвекторов найдено - " + convectors.Count.ToString());
                    log.WriteLine("Spaces найдено - " + spaces.Count.ToString());

                    // null old values of parameter
                    foreach (var space in spaces)
                    {
                        try
                        {
                            space.LookupParameter(SelectedParameterSpace.Definition.Name).Set(0.0);
                        }
                        catch (Exception e1)
                        {
                            log.WriteLine(space.Name.ToString() + " - " + e1.Message);
                        }
                    }

                    for (int i = 0; i < convectors.Count(); i++)
                    {
                        XYZ convectorLocationPoint = (convectors[i].Location as LocationPoint).Point;
                        XYZ convectorLocationPointZ = new XYZ(convectorLocationPoint.X, convectorLocationPoint.Y, convectorLocationPoint.Z + 3.0);

                        XYZ convectorLocationPoint_PlusZMinusY = new XYZ(convectorLocationPoint.X, convectorLocationPoint.Y - 3.0, convectorLocationPoint.Z + 3.0);
                        XYZ convectorLocationPoint_PlusZPlusY = new XYZ(convectorLocationPoint.X, convectorLocationPoint.Y + 3.0, convectorLocationPoint.Z + 3.0);
                        XYZ convectorLocationPoint_PlusZPlusX = new XYZ(convectorLocationPoint.X + 3.0, convectorLocationPoint.Y, convectorLocationPoint.Z + 3.0);
                        XYZ convectorLocationPoint_PlusZPMinusX = new XYZ(convectorLocationPoint.X -3.0, convectorLocationPoint.Y, convectorLocationPoint.Z + 3.0);

                        // check if space not just above convector, but closely 
                        if (doc.GetSpaceAtPoint(convectorLocationPointZ, SelectedPhase) == null)
                        {
                            if (doc.GetSpaceAtPoint(convectorLocationPoint_PlusZMinusY, SelectedPhase) != null) {
                                convectorLocationPointZ = convectorLocationPoint_PlusZMinusY;
                            } else if (doc.GetSpaceAtPoint(convectorLocationPoint_PlusZPlusY, SelectedPhase) != null) {
                                convectorLocationPointZ = convectorLocationPoint_PlusZPlusY;
                            } else if (doc.GetSpaceAtPoint(convectorLocationPoint_PlusZPlusX, SelectedPhase) != null) {
                                convectorLocationPointZ = convectorLocationPoint_PlusZPlusX;
                            } else if (doc.GetSpaceAtPoint(convectorLocationPoint_PlusZPMinusX, SelectedPhase) != null) {
                                convectorLocationPointZ = convectorLocationPoint_PlusZPMinusX;
                            }
                        }

                        if (SelectedPhase != null && doc.GetSpaceAtPoint(convectorLocationPointZ, SelectedPhase) != null)
                        {
                            try
                            {
                                Space spaceWhereConvector = doc.GetSpaceAtPoint(convectorLocationPointZ, SelectedPhase) as Space;
                                double cool_power_was_conv = spaceWhereConvector.LookupParameter(SelectedParameterSpace.Definition.Name).AsDouble();

                                spaceWhereConvector.LookupParameter(SelectedParameterSpace.Definition.Name)
                                    .Set(cool_power_was_conv + UnitUtils.ConvertFromInternalUnits(convectors[i].LookupParameter(SelectedParameter.Definition.Name).AsDouble(), UnitTypeId.Watts));
                                convectorsCount ++;

                            }
                            catch (Exception e)
                            {
                                log.WriteLine(e.Message + convectors[i].Id.IntegerValue);
                            }
                        }
                        else
                        {
                            log.WriteLine(convectors[i].Id + " - " + convectors[i].LookupParameter("ADSK_Зона").AsString() + " - did't found Space");
                            convectorsLostCount++;
                        }
                    }

                    tr.Commit();
                }
            }
            Status = "Успех - " + datelog_status
                + Environment.NewLine + "конвекторов найдено: " + convectorsCount 
                + Environment.NewLine + "конвекторов не нашедших space: " + convectorsLostCount;
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
