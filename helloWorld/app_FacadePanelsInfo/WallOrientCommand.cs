using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection.Emit;
using System.Collections.ObjectModel;
using ApplicationNamespace;

namespace app_FacadePanelsInfo
{
    [Transaction(TransactionMode.Manual)]
    public class WallOrientCommand : IExternalCommand
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

            if (RevitAPI.UIApplication == null)
            {
                RevitAPI.Initialize(commandData);
            }

            Document doc = RevitAPI.Document;


            string pathLogs = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\log.txt";
            string pathLogsWalls = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\log_walls.txt";

            using (StreamWriter log = new StreamWriter(pathLogs))
            {

                BasePoint basePoint = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_ProjectBasePoint)
                    .WhereElementIsNotElementType().ToList().First() as BasePoint;

                log.WriteLine("BASEPOINT_ANGLETON_PARAM - " + basePoint.get_Parameter(BuiltInParameter.BASEPOINT_ANGLETON_PARAM).AsValueString());
                
                string angleTrueNorth_str = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_ANGLETON_PARAM).AsValueString();
                angleTrueNorth_str = angleTrueNorth_str.Substring(0, angleTrueNorth_str.Length-1).Replace(',', '.');
                double angleTrueNorth = Math.Round( Double.Parse(angleTrueNorth_str), 2);
                log.WriteLine(angleTrueNorth);


                using (StreamWriter logWalls = new StreamWriter(pathLogsWalls))
                {

                    //write header for output txt
                    List<string> columns = new List<string>() {
                        "wall.Id",
                       "wall.Name",
                       "ADSK_Позиция на схеме",
                       "area_m2",
                       "Normalize_X",
                       "Normalize_Y",
                       "Normalize_Z",
                       "AngleToTrueNorth",
                       "AnglesToCardinal",
                       "IsClerestory",
                       "FaceOrientationCardial"
                    };

                    foreach (string column in columns) {
                        logWalls.Write(column + ",");    
                    }
                    logWalls.Write("\n");

                    // finding angle to true north
                    IList<Element> wall_elements = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().ToList();

                    foreach (Element wall in wall_elements)
                    {


                        OrientedWall orientedWall = new OrientedWall( wall, angleTrueNorth, 0.45, true );

                        if (orientedWall.IsValidObject)
                        {

                            logWalls.WriteLine(wall.Id + ","
                                  + wall.Name + ","
                                  + wall.LookupParameter("ADSK_Позиция на схеме").AsString() + ","
                                  + Math.Round(UnitUtils.ConvertFromInternalUnits(orientedWall.FaceArea, UnitTypeId.SquareMeters), 2) + ","
                                  + Math.Round(orientedWall.FaceNormal.X, 2) + ","
                                  + Math.Round(orientedWall.FaceNormal.Y, 2) + ","
                                  + Math.Round(orientedWall.FaceNormal.Z, 2) + ","
                                  + orientedWall.AngleToTrueNorth + ","
                                  + orientedWall.AnglesToCardinal + ","
                                  + orientedWall.IsClerestory + ","
                                  + orientedWall.FaceOrientationCardial

                                 );
                        }
                    }
                }
                return Result.Succeeded;
            }
        }





    }
}
