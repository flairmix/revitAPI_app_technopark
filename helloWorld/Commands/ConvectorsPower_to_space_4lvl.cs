using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System.IO;


namespace Technopark.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class ConvectorsPower_to_space_4lvl : IExternalCommand
    {
        int levelID = 725699;
        int worksetIdHeating = 440;
        int phaseConvectorInt = 3;

        // for report message
        Dictionary<string, int> report = new Dictionary<string, int>();
        string reportMessage = "";
        int catchedConvectors = 0;
        string catchedConvectorsID = "";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            string pathLogs = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\log.txt";
            string p_ADSK_Zone = "ADSK_Зона";
            string p_ADSK_HeatingPower_in_zone = "ADSK_Тепловая мощность";

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
                using (Transaction tr = new Transaction(doc, "CopyParameter"))
                {
                    tr.Start();

                    IList<Element> convectors = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
                        .WhereElementIsNotElementType().ToList()
                        .Where(x => x.LookupParameter("ADSK_Обозначение") != null && x.WorksetId.IntegerValue == worksetIdHeating && x.LevelId.IntegerValue == levelID)
                        .ToList();

                    IList<Element> spaces = new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_MEPSpaces)
                            .WhereElementIsNotElementType()
                            .ToList()
                            .Where(x => x.LevelId.IntegerValue == levelID)
                            .ToList();

                    foreach (var space in spaces)
                    {
                        space.LookupParameter(p_ADSK_HeatingPower_in_zone).Set(0.0);
                    }

                    log.WriteLine(convectors.Count.ToString());
                    log.WriteLine(doc.Title.ToString());

                    for (int i = 0; i < convectors.Count(); i++)
                    {
                        //report dict logging
                        string convectorZone = (convectors[i].LookupParameter(p_ADSK_Zone).AsString());
                        if (!report.ContainsKey(convectorZone))
                        {
                            report[convectorZone] = 0;
                        }

                        XYZ convectorLocationPoint = (convectors[i].Location as LocationPoint).Point;
                        XYZ convectorLocationPointZ = new XYZ(convectorLocationPoint.X, convectorLocationPoint.Y, convectorLocationPoint.Z + 3.0);
                        XYZ convectorLocationPointZ_plusY = new XYZ(convectorLocationPoint.X, convectorLocationPoint.Y + 3.0, convectorLocationPoint.Z + 3.0);
                        XYZ convectorLocationPointZ_minusY = new XYZ(convectorLocationPoint.X, convectorLocationPoint.Y - 3.0, convectorLocationPoint.Z + 3.0);

                        if (doc.GetSpaceAtPoint(convectorLocationPointZ, phaseConvector) != null)
                        {
                            try
                            {
                                Space spaceWhereConvector = doc.GetSpaceAtPoint(convectorLocationPointZ, phaseConvector) as Space;
                                string spaceZone = spaceWhereConvector.LookupParameter(p_ADSK_Zone).AsString();
                                double heat_power = convectors[i].LookupParameter("MC Piping Power").AsDouble();
                                double heat_power_was = spaceWhereConvector.LookupParameter(p_ADSK_HeatingPower_in_zone).AsDouble();
                                spaceWhereConvector.LookupParameter(p_ADSK_HeatingPower_in_zone).Set(heat_power_was + heat_power);
                                
                                // counting for report dialog message
                                report[convectorZone]++;

                            }
                            catch (Exception e)
                            {
                                log.WriteLine(convectors[i].Name.ToString());
                                log.WriteLine(convectors[i].LookupParameter(p_ADSK_Zone).AsString());
                                log.WriteLine(convectors[i].Id.ToString() + " - " + e.Message);
                                // counting for report dialog message
                                catchedConvectors++;
                                catchedConvectorsID += (convectors[i].Id.ToString() + " ");
                            }
                        }
                        else if (doc.GetSpaceAtPoint(convectorLocationPointZ_plusY) != null)
                        {
                            try
                            {
                                Space spaceWhereConvector = doc.GetSpaceAtPoint(convectorLocationPointZ_plusY, phaseConvector) as Space;
                                string spaceZone = spaceWhereConvector.LookupParameter(p_ADSK_Zone).AsString();
                                double heat_power = convectors[i].LookupParameter("MC Piping Power").AsDouble();
                                double heat_power_was = spaceWhereConvector.LookupParameter(p_ADSK_HeatingPower_in_zone).AsDouble();
                                spaceWhereConvector.LookupParameter(p_ADSK_HeatingPower_in_zone).Set(heat_power_was + heat_power);
                                
                                // counting for report dialog message
                                report[convectorZone]++;
                                catchedConvectorsID += (convectors[i].Id.ToString() + " ");
                            }
                            catch (Exception e)
                            {
                                log.WriteLine(convectors[i].Name.ToString());
                                log.WriteLine(convectors[i].LookupParameter(p_ADSK_Zone).AsString());
                                log.WriteLine(convectors[i].Id.ToString() + " - " + e.Message);                                
                                // counting for report dialog message
                                catchedConvectors++;
                                catchedConvectorsID += (convectors[i].Id.ToString() + " ");
                            }
                        }

                        else
                        {
                            try
                            {
                                Space spaceWhereConvector = doc.GetSpaceAtPoint(convectorLocationPointZ_minusY, phaseConvector) as Space;
                                string spaceZone = spaceWhereConvector.LookupParameter(p_ADSK_Zone).AsString();
                                double heat_power = convectors[i].LookupParameter("MC Piping Power").AsDouble();
                                double heat_power_was = spaceWhereConvector.LookupParameter(p_ADSK_HeatingPower_in_zone).AsDouble();
                                spaceWhereConvector.LookupParameter(p_ADSK_HeatingPower_in_zone).Set(heat_power_was + heat_power);

                                // counting for report dialog message
                                report[convectorZone]++;
                                catchedConvectorsID += (convectors[i].Id.ToString() + " ");

                            }
                            catch (Exception e)
                            {
                                log.WriteLine(convectors[i].Name.ToString());
                                log.WriteLine(convectors[i].LookupParameter(p_ADSK_Zone).AsString());
                                log.WriteLine(convectors[i].Id.ToString() + " - " + e.Message);
                                // counting for report dialog message
                                catchedConvectors++;
                                catchedConvectorsID += (convectors[i].Id.ToString() + " ");
                            }
                        }
                    }

                    tr.Commit();

                } // end transaction 
            } //end StreamWriter

            //report window show 
            foreach (string zoneString in report.Keys)
            {
                reportMessage += (zoneString + " - " + report[zoneString].ToString() + " шт.");
                reportMessage += Environment.NewLine;
            }
            reportMessage += "не нашлись - " + catchedConvectors.ToString() + "шт."
                + Environment.NewLine
                + catchedConvectorsID;

            TaskDialog reportDialog = new TaskDialog("Инфо");
            reportDialog.MainInstruction = "result - конвекторы нашли свои зоны";
            reportDialog.MainContent = reportMessage;
            reportDialog.Show();

            return Result.Succeeded;
        }
    }
}
