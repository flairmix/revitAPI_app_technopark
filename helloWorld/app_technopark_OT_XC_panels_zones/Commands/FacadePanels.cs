using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection.Emit;
using System.Collections.ObjectModel;

namespace Technopark.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class FacadePanels : IExternalCommand
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
        string outputFileName;

        readonly List<string> archModelTitles = new List<string>() { "164_TPS_Arch_GOR_EXT_A-C_Panel_(MEP)" };

        readonly IDictionary<string, double> levelsHightLimit = new Dictionary<string, double>(14) {
                {"level_1_floor" , 0.0},
                {"level_2_floor" , 30.5},
                {"level_3_floor" , 53.20},
                {"level_4_floor" , 75.86},
                {"level_5_floor" , 98.5},
                {"level_6_floor" , 121.0},
                {"level_7_floor" , 148.0},
                {"level_1_ceiling" , 30.5},
                {"level_2_ceiling" , 53.20},
                {"level_3_ceiling" , 75.86},
                {"level_4_ceiling" , 98.5},
                {"level_5_ceiling" , 121.17},
                {"level_6_ceiling" , 148.0},
                {"level_7_ceiling" , 200.0}
            };

        int level;

        IDictionary<string, int> docFileLevel = new Dictionary<string, int>(4) {
            {"11899_TPS_MEP_OT_ATP_A-C_L1-L2", 1},
            {"11899_TPS_MEP_OT_ATP_A-C_L3-L4", 3},
            {"11899_TPS_MEP_OT_ATP_A-C_L5-L6", 5},
            {"11899_TPS_MEP_OT_ATP_A-C_L7", 7}
        };



public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            Autodesk.Revit.DB.View activeView = doc.ActiveView;

            //phase determine for finding spaces
            int phaseConvectorInt = 3;

            PhaseArray phases = doc.Phases;
            Phase phaseConvector = null;

            foreach (Phase phase in phases)
            {
                if (phase.Id.IntegerValue == phaseConvectorInt)
                {
                    phaseConvector = phase;
                }
            }

            using (StreamWriter log = new StreamWriter(pathLogs)) 
            {
                // finding what Document is opened and determine level of building drom Document.Title
                foreach (string docName in docFileLevel.Keys) {
                    if (doc.Title.Contains(docName)) { 
                        level = docFileLevel[docName];
                    }
                }

                outputFileName = "Panels_level_" + level.ToString() + "_OT.txt";

                using (StreamWriter outputFile = new StreamWriter(pathLevelOutputFolder + outputFileName))
                {
                    try
                    {
                        panelsHeatingZoneFind(doc, 
                            outputFile, 
                            log, 
                            archModelTitles, 
                            levelsHightLimit["level_" + level.ToString() + "_floor"], 
                            levelsHightLimit["level_" + level.ToString() + "_ceiling"], 
                            60,
                            phaseConvector);

                        MessageBox.Show("Уровень - " + level.ToString() + Environment.NewLine
                            + " - Успех" + Environment.NewLine
                            + "сохранено по пути:  " + pathLevelOutputFolder + outputFileName);
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Error", ex.Message);
                    }
                }

                if(level != 7)
                {
                    level += 1;
                    outputFileName = "Panels_level_" + level.ToString() + "_OT.txt";
                    using (StreamWriter outputFile = new StreamWriter(pathLevelOutputFolder + outputFileName))
                    {
                        try
                        {
                            panelsHeatingZoneFind(doc,
                                outputFile,
                                log,
                                archModelTitles,
                                levelsHightLimit["level_" + level.ToString() + "_floor"],
                                levelsHightLimit["level_" + level.ToString() + "_ceiling"],
                                60,
                                phaseConvector);

                            MessageBox.Show("Уровень - " + level.ToString() + Environment.NewLine
                                + " - Успех" + Environment.NewLine
                                + "сохранено по пути:  " + pathLevelOutputFolder + outputFileName);
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Error", ex.Message);
                        }

                    }
                }
            }
            return Result.Succeeded;
        }

        private void panelsHeatingZoneFind(Document _doc,
                                            StreamWriter outputFile,
                                            StreamWriter logFile,
                                            List<string> archModelTitles,
                                           double hightLimitDown,
                                           double hightLimitUp,
                                           int finderRadius, 
                                           Phase phase
                                           )
        {
            FilteredElementCollector revitLinks = new FilteredElementCollector(_doc)
                .OfClass(typeof(RevitLinkInstance))
                .WhereElementIsNotElementType();

            foreach (Element e in revitLinks)
            {
                try
                {
                    RevitLinkInstance Instance = e as RevitLinkInstance;
                    Document docLink = Instance.GetLinkDocument();

                    logFile.WriteLine(docLink.Title);  //log

                    if (archModelTitles.Contains(docLink.Title.ToString()))
                    {
                        logFile.WriteLine("found right Document");  //log
                        try
                        {
                            IList<Element> walls = new FilteredElementCollector(docLink)
                                                        .OfCategory(BuiltInCategory.OST_Walls)
                                                        .WhereElementIsNotElementType()
                                                        .ToList();

                            IList<Element> doors = new FilteredElementCollector(docLink)
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
                                                  + "," + checkSpacesAround(_doc, doorLocation.Point, finderRadius, logFile, phase)
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

                                            if (center.Z > hightLimitDown && center.Z < hightLimitUp && wall.LookupParameter("ADSK_Позиция на схеме").AsString().Length > 0)
                                            {
                                                outputFile.WriteLine(wall.Id.ToString() + "," 
                                                    + wall.Name.ToString().Replace(',', '.')
                                                    + "," + System.Math.Round(center.X, 2).ToString().Replace(',', '.')
                                                    + "," + System.Math.Round(center.Y, 2).ToString().Replace(',', '.')
                                                    + "," + System.Math.Round(center.Z, 2).ToString().Replace(',', '.')
                                                    + "," + checkSpacesAround(_doc, center, finderRadius, logFile, phase)
                                                    + "," + System.Math.Round(UnitUtils.ConvertFromInternalUnits(wall.LookupParameter("Area")
                                                                            .AsDouble(), UnitTypeId.SquareMeters), 3).ToString().Replace(',', '.')
                                                    + "," + R_wall[wall.Name.ToString()].ToString().Replace(",", ".")
                                                    + "," + wall.LookupParameter("ADSK_Позиция на схеме").AsString()
                                                );
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        logFile.WriteLine("cant find walls");  //log
                    }
                }
                catch (Exception )
                {
                }
            }
        }

        
        private string checkSpacesAround(Document _doc, XYZ point, int diff, StreamWriter logs, Phase phase)
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
