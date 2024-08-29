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

namespace app_technopark_collectingPower
{
    public class ViewModel : INotifyPropertyChanged
    {
        private Workset _selectedWorkset;
        private Level _selectedLevel;
        private Phase _selectedPhase;
        private BuiltInCategory _selectedBuildInCategory;
        private Parameter _selectedParameter;
        private Parameter _selectedParameterSpace;

        readonly string pathLogs = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\log.txt";

        Document doc = RevitAPI.Document;

        public ViewModel(Reference reference)
        {
            CollectWorksets(doc);
            CollectLevels(doc);
            CollectPhases(doc);
            CollectParametersEquip(reference);
            CollectParametersSpaces();

            XC_WriteConvectorsPower_to_space_command = new RelayCommand(Write_DoubleParameter_to_space_cumulatively, TypeCheckingInputs);
        }

        public RelayCommand XC_WriteConvectorsPower_to_space_command { get; set; }

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
        public IList<Parameter> Parameters { get; set; } = new List<Parameter>();
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
        private void CollectParametersEquip(Reference reference)
        {
            var element = RevitAPI.Document.GetElement(reference);
            ParameterSet parameterSet = element.Parameters;
            foreach (Parameter paramObj in parameterSet)
            {
                var parameter = (Parameter)paramObj;
                if (!parameter.IsReadOnly && parameter.StorageType != StorageType.ElementId)
                {
                    Parameters.Add(parameter);
                }
            }
        }
        private void CollectParametersSpaces()
        {
            ParameterSet parameterSet = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_MEPSpaces).FirstElement().Parameters;

            foreach (Parameter paramObj in parameterSet)
            {
                var parameter = (Parameter)paramObj;
                if (!parameter.IsReadOnly && parameter.StorageType != StorageType.ElementId)
                {
                    ParametersSpace.Add(parameter);
                }
            }
        }


        private bool TypeCheckingInputs(object obj)
        {
            return SelectedWorkset != null
                && SelectedLevel != null
                && SelectedPhase != null
                && SelectedParameter != null
                && SelectedParameterSpace != null
                && SelectedBuildInCategory != 0
                ;
        }


        public void Write_DoubleParameter_to_space_cumulatively(object obj)
        {
            using (StreamWriter log = new StreamWriter(pathLogs))
            {
                using (Transaction tr = new Transaction(doc, "CopyParameter"))
                {
                    tr.Start();

                    IList<Element> convectors = new FilteredElementCollector(doc)
                        .OfCategory(SelectedBuildInCategory)
                        .WhereElementIsNotElementType().ToList()
                        .Where(x => x.WorksetId.IntegerValue == SelectedWorkset.Id.IntegerValue && x.LevelId.IntegerValue == SelectedLevel.Id.IntegerValue)
                        .ToList();

                    IList<Element> spaces = new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_MEPSpaces)
                            .WhereElementIsNotElementType()
                            .ToList();

                    log.WriteLine("Конвекторов найдено - " + convectors.Count.ToString());
                    log.WriteLine("Spaces найдено - " + spaces.Count.ToString());

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

                        if (SelectedPhase != null && doc.GetSpaceAtPoint(convectorLocationPointZ, SelectedPhase) != null)
                        {
                            try
                            {
                                Space spaceWhereConvector = doc.GetSpaceAtPoint(convectorLocationPointZ, SelectedPhase) as Space;
                                double cool_power_was_conv = spaceWhereConvector.LookupParameter(SelectedParameterSpace.Definition.Name).AsDouble();

                                spaceWhereConvector.LookupParameter(SelectedParameterSpace.Definition.Name)
                                    .Set(cool_power_was_conv + UnitUtils.ConvertFromInternalUnits(convectors[i]
                                    .LookupParameter(SelectedParameter.Definition.Name)
                                    .AsDouble(), UnitTypeId.Watts));
                            }
                            catch (Exception e)
                            {
                                log.WriteLine(e);
                            }
                        }
                    }

                    tr.Commit();
                }
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
