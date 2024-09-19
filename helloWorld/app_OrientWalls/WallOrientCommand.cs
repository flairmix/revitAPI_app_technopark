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

                        //if (orientedWall.IsValidObject)
                        //{

                        //    logWalls.WriteLine(wall.Id + ","
                        //          + wall.Name + ","
                        //          + wall.LookupParameter("ADSK_Позиция на схеме").AsString() + ","
                        //          + Math.Round(UnitUtils.ConvertFromInternalUnits(orientedWall.FaceArea, UnitTypeId.SquareMeters), 2) + ","
                        //          + Math.Round(orientedWall.FaceNormal.X, 2) + ","
                        //          + Math.Round(orientedWall.FaceNormal.Y, 2) + ","
                        //          + Math.Round(orientedWall.FaceNormal.Z, 2) + ","
                        //          + orientedWall.AngleToTrueNorth + ","
                        //          + orientedWall.AnglesToCardinal + ","
                        //          + orientedWall.IsClerestory + ","
                        //          + orientedWall.FaceOrientationCardial

                        //         );
                        //}                        
                        
                        if (orientedWall.IsValidObject && (orientedWall.CurrentPlaneType == OrientedWall.PlaneType.Revolved))
                        {

                            logWalls.WriteLine(wall.Id + ","
                                  + wall.Name + ","
                                  + wall.LookupParameter("ADSK_Позиция на схеме").AsString() + ","
                                  + Math.Round(UnitUtils.ConvertFromInternalUnits(orientedWall.FaceArea, UnitTypeId.SquareMeters), 2) + ","
                                  + Math.Round(orientedWall.FaceNormal.X, 2) + ","
                                  + Math.Round(orientedWall.FaceNormal.Y, 2) + ","
                                  + Math.Round(orientedWall.FaceNormal.Z, 2) 

                                 );
                        }
                    }
                }
                return Result.Succeeded;
            }
        }





    }
}
