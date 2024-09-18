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
using System.Windows;
using System.Windows.Controls;

namespace app_FacadePanelsInfo
{
    public class FacadePanelsInfoViewModel : INotifyPropertyChanged
    {

        readonly IDictionary<string, double> R_wall = new Dictionary<string, double>(){
            {"EWS11", 0.710},
            {"EWS12", 0.710},
            {"EWS13", 0.710},
            {"EWS15", 0.710},
            {"EWS16", 3.0},
            {"EWS17", 3.0},
            {"EWS21", 0.710},
            {"EWS22", 0.710},
            {"EWS23", 0.710},
            {"EWS23.ут", 3.700},
            {"EWS24", 0.710},
            {"EWS25", 0.710},
            {"EWS25.1", 0.710},
            {"EWS31", 0.710},
            {"EWS41", 0.710},
            {"RSF41", 0.710},
            {"door", 0.63}
        };

        readonly string pathLogs = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\log_panels_from_level.txt";
        readonly string pathLevelOutputFolder = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\";

        private Document _selectedLinkWithWalls;
        private Level _selectedLevelFloor;
        private Workset _selectedWorksetWithSpaces;
        private Level _selectedLevelCeiling;
        private double _selectedLevelFloorIndent;
        private double _selectedLevelCeilingIndent;
        private Phase _selectedPhase;
        string _status;
        private string _version;

        Document doc = RevitAPI.Document;

        public FacadePanelsInfoViewModel()
        {
            _version = "ver_240918_0.60_MID";
            CollectLinkDocuments(doc);
            CollectWorksetsWithSpaces(doc);
            CollectLevels(doc);
            SelectedLevelFloorIndent = 0.0;
            SelectedLevelCeilingIndent = 0.0;
            CollectPhases(doc);

            CollectExternalWallInfo_Command = new RelayCommand(CollectExternalWallInfo, TypeCheckingInputs);
        }

        public RelayCommand CollectExternalWallInfo_Command { get; set; }

        public Document SelectedLinkWithWalls
        {
            get => _selectedLinkWithWalls;
            set
            {
                _selectedLinkWithWalls = value;
                OnPropertyChanged();
            }
        }
        public Level SelectedLevelFloor
        {
            get => _selectedLevelFloor;
            set
            {
                _selectedLevelFloor = value;
                OnPropertyChanged();
            }
        }        
        public Level SelectedLevelCeiling
        {
            get => _selectedLevelCeiling;
            set
            {
                _selectedLevelCeiling = value;
                OnPropertyChanged();
            }
        }        
        public double SelectedLevelFloorIndent
        {
            get => _selectedLevelFloorIndent;
            set
            {
                _selectedLevelFloorIndent = value;
                OnPropertyChanged();
            }
        }        
        public double SelectedLevelCeilingIndent
        {
            get => _selectedLevelCeilingIndent;
            set
            {
                _selectedLevelCeilingIndent = value;
                OnPropertyChanged();
            }
        }       
        public Workset SelectedWorksetWithSpaces
        {
            get => _selectedWorksetWithSpaces;
            set
            {
                _selectedWorksetWithSpaces = value;
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
        }


        public IList<Workset> WorksetsWithSpaces { get; set; } = new List<Workset>();
        public IList<Level> Levels { get; set; } = new List<Level>();
        public IList<Phase> Phases { get; set; } = new List<Phase>();
        public IList<Document> DocumentLinks { get; set; } = new List<Document>();

        private void CollectWorksetsWithSpaces(Document doc)
        {
            WorksetsWithSpaces = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).Where(x => x.IsOpen).ToList();
        }        
        private void CollectLinkDocuments(Document doc)
        {
            FilteredElementCollector revitLinks = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .WhereElementIsNotElementType();

            foreach (Element e in revitLinks)
            {
                try
                {
                    RevitLinkInstance Instance = e as RevitLinkInstance;
                    
                    if (Instance != null && Instance.GetLinkDocument().Title != null) {
                        DocumentLinks.Add(Instance.GetLinkDocument());
                    }
                    
                }
                catch (Exception) { }
            }
        }
        private void CollectLevels(Document doc)
        {
            Levels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).ToElements().Select(x => x as Level).ToList();
        }
        private void CollectPhases(Document doc)
        {
            PhaseArray phases = doc.Phases;
            foreach (Phase phase in phases)
            {
                Phases.Add(phase);
            }
        }

        private bool TypeCheckingInputs(object obj)
        {
            return SelectedLinkWithWalls != null
                && SelectedPhase != null
                && SelectedLevelFloor != null
                && SelectedLevelCeiling != null
                ;
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


        public void CollectExternalWallInfo(object obj)
        {
            string datelog_status = DateTime.Now.ToLocalTime().ToString("HH:mm:ss");
            string outputFileName = "Walls_level_" + SelectedLevelFloor.Name + ".txt";

            using (StreamWriter log = new StreamWriter(pathLogs))
            {
                using (StreamWriter outputFile = new StreamWriter(pathLevelOutputFolder + outputFileName))
                {
                    try
                    {
                        PanelsHeatingZoneFind(doc,
                            SelectedLinkWithWalls,
                            outputFile,
                            log,
                            SelectedLevelFloor.Elevation + UnitUtils.ConvertToInternalUnits(SelectedLevelFloorIndent, UnitTypeId.Meters),
                            SelectedLevelCeiling.Elevation + UnitUtils.ConvertToInternalUnits(SelectedLevelCeilingIndent, UnitTypeId.Meters),
                            60,
                            SelectedPhase);
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Error", ex.Message);
                    }
                }
            }
            Status = "Успех - " + datelog_status + Environment.NewLine  
                + " Для уровня: от " + SelectedLevelFloor.Name + " + " + SelectedLevelFloorIndent 
                + " до " + SelectedLevelCeiling.Name + " + " + SelectedLevelCeilingIndent + Environment.NewLine
                + "Cохранено по пути:  " + pathLevelOutputFolder + outputFileName;
        }

        private void PanelsHeatingZoneFind(Document _doc,
                                    Document docLinkWithWalls,
                                    StreamWriter outputFile,
                                    StreamWriter logFile,
                                   double hightLimitDown,
                                   double hightLimitUp,
                                   int finderRadius,
                                   Phase phase
                                   )
        {
            try
            {
                IList<Element> walls = new FilteredElementCollector(docLinkWithWalls)
                                            .OfCategory(BuiltInCategory.OST_Walls)
                                            .WhereElementIsNotElementType()
                                            .ToList();

                IList<Element> doors = new FilteredElementCollector(docLinkWithWalls)
                                            .OfCategory(BuiltInCategory.OST_Doors)
                                            .WhereElementIsNotElementType()
                                            .ToList();

                logFile.WriteLine("walls found - " + walls.Count.ToString());  //log
                logFile.WriteLine("doors found - " + doors.Count.ToString());  //log

                foreach (var door in doors)
                {
                    LocationPoint doorLocation = door.Location as LocationPoint;

                    if ((doorLocation.Point.Z + 7.0) > hightLimitDown && (doorLocation.Point.Z + 7.0) < hightLimitUp)
                    {
                        outputFile.WriteLine(door.Id.ToString()
                                        + "," + door.Name.ToString().Replace(',', '.')
                                        + "," + Math.Round(doorLocation.Point.X, 2).ToString().Replace(',', '.')
                                        + "," + Math.Round(doorLocation.Point.Y, 2).ToString().Replace(',', '.')
                                        + "," + Math.Round(doorLocation.Point.Z, 2).ToString().Replace(',', '.')
                                        + "," + CheckSpacesAround(_doc, doorLocation.Point, finderRadius, logFile, phase)
                                        + "," + Math.Round(UnitUtils.ConvertFromInternalUnits(door.LookupParameter("Площадь проема").AsDouble(), UnitTypeId.SquareMeters), 3)
                                        .ToString().Replace(',', '.')
                                        + "," + R_wall["door"].ToString().Replace(",", ".")
                                        + "," + door.LookupParameter("ADSK_Позиция на схеме").AsString()
                            );
                    }
                }

                if (walls.Count != 0)
                {
                    foreach (var wall in walls)
                    {
                        Options opt = new Options();
                        opt.DetailLevel = ViewDetailLevel.Coarse;
                        Solid wallSolid = null;
                        try
                        {
                            foreach (GeometryObject obj in wall.get_Geometry(opt))
                            {
                                Solid s = obj as Solid;

                                if (s != null)
                                {
                                    wallSolid = s;
                                    break;
                                }
                            }
                            if (wallSolid != null)
                            {
                                XYZ center = wallSolid.ComputeCentroid();

                                if (center.Z > hightLimitDown 
                                    && center.Z < hightLimitUp 
                                    && wall.LookupParameter("ADSK_Позиция на схеме").AsString().Length > 0)
                                {
                                    outputFile.WriteLine(wall.Id.ToString() + ","
                                        + wall.Name.ToString().Replace(',', '.')
                                        + "," + Math.Round(center.X, 2).ToString().Replace(',', '.')
                                        + "," + Math.Round(center.Y, 2).ToString().Replace(',', '.')
                                        + "," + Math.Round(center.Z, 2).ToString().Replace(',', '.')
                                        + "," + CheckSpacesAround(_doc, center, finderRadius, logFile, phase)
                                        + "," + Math.Round(UnitUtils.ConvertFromInternalUnits(wall.LookupParameter("Area")
                                                                .AsDouble(), UnitTypeId.SquareMeters), 3).ToString().Replace(',', '.')
                                        + "," + R_wall[wall.Name.ToString()].ToString().Replace(",", ".")
                                        + "," + wall.LookupParameter("ADSK_Позиция на схеме").AsString()
                                    );
                                }
                            }
                        }
                        catch (Exception)   { }
                    }
                }
            }
            catch (Exception) { }
        }


        private string CheckSpacesAround(Document _doc, XYZ point, int diff, StreamWriter logs, Phase phase)
        {

            try
            {
                if (_doc.GetSpaceAtPoint(point, phase) != null)
                {
                    string zone = _doc.GetSpaceAtPoint(point, phase).LookupParameter("ADSK_Зона").AsString();
                    string spaceName = _doc.GetSpaceAtPoint(point, phase).Name.ToString();
                    return zone + "," + spaceName;
                }

                for (double i = 1.0; i < diff; i += 5.0)
                {

                    XYZ checkPlaceUp = new XYZ(point.X + i, point.Y, point.Z - 4.0);
                    XYZ checkPlaceDown = new XYZ(point.X - i, point.Y, point.Z - 4.0);
                    XYZ checkPlaceRight = new XYZ(point.X, point.Y + i, point.Z - 4.0);
                    XYZ checkPlaceLeft = new XYZ(point.X, point.Y - i, point.Z - 4.0);

                    if (_doc.GetSpaceAtPoint(checkPlaceUp, phase) != null)
                    {
                        string zone = _doc.GetSpaceAtPoint(checkPlaceUp, phase).LookupParameter("ADSK_Зона").AsString();
                        string spaceName = _doc.GetSpaceAtPoint(checkPlaceUp, phase).Name.ToString();
                        return zone + "," + spaceName;
                    }
                    else if (_doc.GetSpaceAtPoint(checkPlaceDown, phase) != null)
                    {
                        string zone = _doc.GetSpaceAtPoint(checkPlaceDown, phase).LookupParameter("ADSK_Зона").AsString();
                        string spaceName = _doc.GetSpaceAtPoint(checkPlaceDown, phase).Name.ToString();
                        return zone + "," + spaceName;
                    }
                    else if (_doc.GetSpaceAtPoint(checkPlaceRight, phase) != null)
                    {
                        string zone = _doc.GetSpaceAtPoint(checkPlaceRight, phase).LookupParameter("ADSK_Зона").AsString();
                        string spaceName = _doc.GetSpaceAtPoint(checkPlaceRight, phase).Name.ToString();
                        return zone + "," + spaceName;
                    }
                    else if (_doc.GetSpaceAtPoint(checkPlaceLeft, phase) != null)
                    {
                        string zone = _doc.GetSpaceAtPoint(checkPlaceLeft, phase).LookupParameter("ADSK_Зона").AsString();
                        string spaceName = _doc.GetSpaceAtPoint(checkPlaceLeft, phase).Name.ToString();
                        return zone + "," + spaceName;
                    }
                }
            }
            catch (Exception)
            {
            }
            return ("not found, not found");
        }


    }
}