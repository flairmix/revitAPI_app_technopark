using ApplicationNamespace;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Policy;
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
        private string _pathLevelOutputFolder;

        private string _selectedSpaceOrRooms;

        private Document _selectedLinkWithWalls;
        private Document _selectedDocWhereWeFind;
        private Level _selectedLevelFloor;
        private Workset _selectedWorksetWithSpaces;
        private Level _selectedLevelCeiling;
        private double _selectedLevelFloorIndent;
        private double _selectedLevelCeilingIndent;
        private double _selectedRadiusOfFind;
        private Phase _selectedPhase;
        private Parameter _selectedParameterWall;
        private bool _isBusy;

        string _status;
        private string _version;

        Document doc = RevitAPI.Document;

        public FacadePanelsInfoViewModel()
        {
            _isBusy = false;
            _version = "ver_241004_0.65_MID";
            _pathLevelOutputFolder = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs";
            CollectLinkDocuments(doc);
            CollectWorksetsWithSpaces(doc);
            CollectLevels(doc);
            SelectedLevelFloorIndent = 0.0;
            SelectedLevelCeilingIndent = 0.0;
            _selectedRadiusOfFind = 5.0;
            CollectPhases(doc);

            DocumentLinks.Add(doc);
            _selectedDocWhereWeFind = doc;

            WallsOrRooms.Add("Spaces");
            WallsOrRooms.Add("Rooms");
            _selectedSpaceOrRooms = WallsOrRooms[0];

            CollectExternalWallInfo_Command = new RelayCommand(CollectExternalWallInfo, TypeCheckingInputs);
            Dialog_Command = new RelayCommand(Dialog, TypeCheckingInputs);
            AddDocsToFindList = new RelayCommand(DocsToFindList, x => true);
        }


        public RelayCommand CollectExternalWallInfo_Command { get; set; }
        public RelayCommand Dialog_Command { get; set; }
        public RelayCommand AddDocsToFindList { get; set; }

        public Document SelectedLinkWithWalls
        {
            get => _selectedLinkWithWalls;
            set
            {
                _selectedLinkWithWalls = value;
                CollectParametersWallsObservable(value);  
                OnPropertyChanged();
            }
        }        
        


        public IList<Workset> WorksetsWithSpaces { get; set; } = new List<Workset>();

        public ObservableCollection<string> WallsOrRooms { get; set; } = new ObservableCollection<string>();

        public IList<Level> Levels { get; set; } = new List<Level>();
        public IList<Phase> Phases { get; set; } = new List<Phase>();
        public ObservableCollection<Document> DocumentLinks { get; set; } = new ObservableCollection<Document>();
        public ObservableCollection<Parameter> ParametersWallsObservable { get; set; } = new ObservableCollection<Parameter>();
        public ObservableCollection<Document> DocsWhereWeFind { get; set; } = new ObservableCollection<Document>();



        public Document SelectedDocWhereWeFind
        {
            get => _selectedLinkWithWalls;
            set
            {
                _selectedDocWhereWeFind = value;
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

        public string SelectedSpaceOrRooms
        {
            get => _selectedSpaceOrRooms;
            set
            {
                _selectedSpaceOrRooms = value;
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
        public double SelectedRadiusOfFind
        {
            get => _selectedRadiusOfFind;
            set
            {
                _selectedRadiusOfFind = value;
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
        public Parameter SelectedParameterWall
        {
            get => _selectedParameterWall;
            set
            {
                _selectedParameterWall = value;
                OnPropertyChanged();
            }
        }
        public bool isBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
        public string PathLevelOutputFolder
        {
            get => _pathLevelOutputFolder;
            set
            {
                _pathLevelOutputFolder = value;
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

        public void DocsToFindList(object obj)
        {
            DocsWhereWeFind.Add(SelectedDocWhereWeFind);
            Status = $"Документ \"{_selectedDocWhereWeFind.Title }\" добавлен в список документов, " +
                $"в которых мы ищем Помещения или Пространства" ;
        }




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
        private void CollectParametersWallsObservable(Document doc)
        {
            Element element = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType()
                 .FirstOrDefault();
            ParameterSet parameterSet = element.Parameters;
            ParametersWallsObservable.Clear();

            foreach (Parameter paramObj in parameterSet)
            {
                var parameter = (Parameter)paramObj;
                if (!parameter.IsReadOnly && parameter.StorageType != StorageType.ElementId)
                {
                    ParametersWallsObservable.Add(parameter);
                }
            }
            ParametersWallsObservable.OrderBy(x => x.Definition.Name).ToList();
        }     

        private bool TypeCheckingInputs(object obj)
        {
            return SelectedLinkWithWalls != null
                && SelectedPhase != null
                && SelectedLevelFloor != null
                && SelectedLevelCeiling != null
                ;
        }



        public void CollectExternalWallInfo(object obj)
        {
            string datelog_status = DateTime.Now.ToLocalTime().ToString("HH:mm:ss");
            string outputFileName = "\\Walls_level_" + SelectedLevelFloor.Name + ".txt";

            using (StreamWriter log = new StreamWriter(pathLogs))
            {
                using (StreamWriter outputFile = new StreamWriter(PathLevelOutputFolder + outputFileName))
                {
                    try
                    {
                        PanelsHeatingZoneFind(doc,
                            SelectedLinkWithWalls,
                            outputFile,
                            log,
                            SelectedLevelFloor.Elevation + UnitUtils.ConvertToInternalUnits(SelectedLevelFloorIndent, UnitTypeId.Meters),
                            SelectedLevelCeiling.Elevation + UnitUtils.ConvertToInternalUnits(SelectedLevelCeilingIndent, UnitTypeId.Meters),
                            UnitUtils.ConvertToInternalUnits(SelectedRadiusOfFind, UnitTypeId.Meters),
                            SelectedPhase);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error", ex.Message);
                    }
                }
            }
            Status = "Успех - " + datelog_status + Environment.NewLine
                + " Для уровня: от " + SelectedLevelFloor.Name + " + " + SelectedLevelFloorIndent
                + " до " + SelectedLevelCeiling.Name + " + " + SelectedLevelCeilingIndent + Environment.NewLine
                + "Cохранено по пути:  " + PathLevelOutputFolder + outputFileName;
        }

        private void PanelsHeatingZoneFind(Document _doc,
                                    Document docLinkWithWalls,
                                    StreamWriter outputFile,
                                    StreamWriter logFile,
                                   double hightLimitDown,
                                   double hightLimitUp,
                                   double finderRadius,
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
                    LocationPoint doorLocationReal = door.Location as LocationPoint;

                    XYZ doorLocation = new XYZ(doorLocationReal.Point.X, doorLocationReal.Point.Y, doorLocationReal.Point.Z + 1.0);

                    string checkAround = null;

                    if (SelectedSpaceOrRooms == "Spaces")
                    {
                        checkAround = CheckSpacesAroundOLD(_doc, doorLocation, finderRadius, logFile, phase);
                    } else if (SelectedSpaceOrRooms == "Rooms")
                    {
                        checkAround = CheckRoomsAroundOLD(_doc, doorLocation, finderRadius, logFile, phase);
                    }


                    if ((doorLocation.Z + 7.0) > hightLimitDown && (doorLocation.Z + 7.0) < hightLimitUp)
                    {
                        outputFile.WriteLine(door.Id.ToString()
                                        + "," + door.Name.ToString().Replace(',', '.')
                                        + "," + Math.Round(doorLocationReal.Point.X, 2).ToString().Replace(',', '.')
                                        + "," + Math.Round(doorLocationReal.Point.Y, 2).ToString().Replace(',', '.')
                                        + "," + Math.Round(doorLocationReal.Point.Z, 2).ToString().Replace(',', '.')
                                        + "," + checkAround
                                        + "," + Math.Round(UnitUtils.ConvertFromInternalUnits(door.LookupParameter("Площадь проема").AsDouble(), UnitTypeId.SquareMeters), 3)
                                        .ToString().Replace(',', '.')
                                        + "," + R_wall["door"].ToString().Replace(",", ".")
                                        + "," + door.LookupParameter(SelectedParameterWall.Definition.Name).AsString()
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

                                string checkAround = null;
                                if (SelectedSpaceOrRooms == "Spaces")
                                {
                                    checkAround = CheckSpacesAround(_doc, center, finderRadius, logFile, phase);
                                }
                                else if (SelectedSpaceOrRooms == "Rooms")
                                {
                                    checkAround = CheckSpacesAround(_doc, center, finderRadius, logFile, phase);
                                }


                                if (center.Z > hightLimitDown 
                                    && center.Z < hightLimitUp 
                                    && wall.LookupParameter(SelectedParameterWall.Definition.Name).AsString().Length > 0
                                    && SelectedParameterWall.AsString() != ""
                                    && SelectedParameterWall.AsString() != null)
                                {
                                    outputFile.WriteLine(wall.Id.ToString() + ","
                                        + wall.Name.ToString().Replace(',', '.')
                                        + "," + Math.Round(center.X, 2).ToString().Replace(',', '.')
                                        + "," + Math.Round(center.Y, 2).ToString().Replace(',', '.')
                                        + "," + Math.Round(center.Z, 2).ToString().Replace(',', '.')
                                        + "," + checkAround
                                        + "," + Math.Round(UnitUtils.ConvertFromInternalUnits(wall.LookupParameter("Area")
                                                                .AsDouble(), UnitTypeId.SquareMeters), 3).ToString().Replace(',', '.')
                                        + "," + R_wall[wall.Name.ToString()].ToString().Replace(",", ".")
                                        + "," + wall.LookupParameter(SelectedParameterWall.Definition.Name).AsString()
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

        private string CheckSpacesAround(Document _doc, 
                                        XYZ point, 
                                        double radius_ft, 
                                        StreamWriter logs, 
                                        Phase phase, 
                                        int radiusFindingStep_ft = 3,
                                        double angleOfRotate = Math.PI / 2)
        {
            try
            {
                if (_doc.GetSpaceAtPoint(point, phase) != null)
                {
                    string zone = _doc.GetSpaceAtPoint(point, phase).LookupParameter("ADSK_Зона").AsString();
                    string spaceName = _doc.GetSpaceAtPoint(point, phase).Name.ToString();
                    return zone + "," + spaceName;
                }

                for (int radiusDelta = 1; radiusDelta < (int)radius_ft; radiusDelta += radiusFindingStep_ft)
                {
                    XYZ localVector = new XYZ(0.0, 1.0, 0.0).Multiply(radiusDelta);

                    for (double angleDelta = 0; angleDelta < Math.PI*2; angleDelta+= angleOfRotate)
                    {
                        // counter clockwise rotation of vector
                        XYZ localVectorRotated = new XYZ((localVector.X * Math.Cos(angleDelta) - localVector.Y * Math.Sin(angleDelta)),
                                                        (localVector.X * Math.Sin(angleDelta) + localVector.Y * Math.Cos(angleDelta)), 
                                                        localVector.Z
                                                        );

                        if (_doc.GetSpaceAtPoint(point.Add(localVectorRotated), phase) != null)
                        {
                            string zone = _doc.GetSpaceAtPoint(point.Add(localVectorRotated), phase).LookupParameter("ADSK_Зона").AsString();
                            string spaceName = _doc.GetSpaceAtPoint(point.Add(localVectorRotated), phase).Name.ToString();
                            return zone + "," + spaceName;
                        }
                    } 
                }
            }
            catch (Exception)
            {
            }
            return ("not found, not found");

        }

        private string CheckSpacesAroundOLD(Document _doc, XYZ point, double diff, StreamWriter logs, Phase phase)
        {
            try
            {
                if (_doc.GetSpaceAtPoint(point, phase) != null)
                {
                    string zone = _doc.GetSpaceAtPoint(point, phase).LookupParameter("ADSK_Зона").AsString();
                    string spaceName = _doc.GetSpaceAtPoint(point, phase).Name.ToString();
                    return zone + "," + spaceName;
                }

                for (double i = 1.0; i < diff; i ++)
                {

                    XYZ checkPlaceUp = new XYZ(point.X + i, point.Y, point.Z);
                    XYZ checkPlaceDown = new XYZ(point.X - i, point.Y, point.Z);
                    XYZ checkPlaceRight = new XYZ(point.X, point.Y + i, point.Z);
                    XYZ checkPlaceLeft = new XYZ(point.X, point.Y - i, point.Z);

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
        
        private string CheckRoomsAroundOLD(Document _doc, XYZ point, double diff, StreamWriter logs, Phase phase)
        {
            try
            {
                if (_doc.GetRoomAtPoint(point, phase) != null)
                {
                    return _doc.GetRoomAtPoint(point, phase).Name.ToString();
                }

                for (int i = 1; i < diff; i += 5)
                {
                    XYZ checkPlaceUp = new XYZ(point.X + i, point.Y, point.Z - 4.0);
                    XYZ checkPlaceDown = new XYZ(point.X - i, point.Y, point.Z - 4.0);
                    XYZ checkPlaceRight = new XYZ(point.X, point.Y + i, point.Z - 4.0);
                    XYZ checkPlaceLeft = new XYZ(point.X, point.Y - i, point.Z - 4.0);

                    if (_doc.GetRoomAtPoint(checkPlaceUp, phase) != null)
                    {
                        return _doc.GetRoomAtPoint(checkPlaceUp, phase).Name.ToString();
                    }
                    else if (_doc.GetRoomAtPoint(checkPlaceDown, phase) != null)
                    {
                        return _doc.GetRoomAtPoint(checkPlaceDown, phase).Name.ToString();
                    }
                    else if (_doc.GetRoomAtPoint(checkPlaceRight, phase) != null)
                    {
                        return _doc.GetRoomAtPoint(checkPlaceRight, phase).Name.ToString();
                    }
                    else if (_doc.GetRoomAtPoint(checkPlaceLeft, phase) != null)
                    {
                        return _doc.GetRoomAtPoint(checkPlaceLeft, phase).Name.ToString();
                    }
                }
            }
            catch (Exception)
            {
            }
            return ("not found, not found");
        }


        private void Dialog(object obj)
        {

            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Выберите место сохранения файла:";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = _pathLevelOutputFolder;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = _pathLevelOutputFolder;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;


            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                PathLevelOutputFolder = dlg.FileName;
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