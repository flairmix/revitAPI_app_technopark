using Autodesk.Revit.UI;
using Autodesk.Revit;
using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.IO;

namespace Technopark.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class SpaceHeatLossesFill : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string pathLogs = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\log.txt";
            string folderPath = @"\\atptlp.local\dfs\MOS-TLP\PROJEKTE\11899\05_HAUSTECHNIK\01_Planung\05_Ausfuehrungsplanung\01_HVaC\03_Calculations" +
                                    @"\08_Отопление и теплоснабжение" +
                                    @"\MID_расчет теплопотерь\";

            string fileName = "_MID_HeatLosses_to_REVIT.csv";

            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            Autodesk.Revit.DB.View activeView = doc.ActiveView;

            Dictionary<string, string> levelsIDS_to_levelNumber = new Dictionary<string, string>();

            IDictionary<string, int> docFileLevel = new Dictionary<string, int>(4) {
                {"11899_TPS_MEP_OT_ATP_A-C_L1-L2", 1},
                {"11899_TPS_MEP_OT_ATP_A-C_L3-L4", 3},
                {"11899_TPS_MEP_OT_ATP_A-C_L5-L6", 5},
                {"11899_TPS_MEP_OT_ATP_A-C_L7", 7}
            };

            foreach (string docName in docFileLevel.Keys)
            {
                if (doc.Title.Contains(docName))
                {
                    if (docName == "11899_TPS_MEP_OT_ATP_A-C_L1-L2")
                    {
                        levelsIDS_to_levelNumber["725695"] = "1lvl";
                        levelsIDS_to_levelNumber["725705"] = "2lvl";
                    }
                    else if (docName == "11899_TPS_MEP_OT_ATP_A-C_L3-L4")
                    {
                        levelsIDS_to_levelNumber["725698"] = "3lvl";
                        levelsIDS_to_levelNumber["725699"] = "4lvl";

                    }
                    else if (docName == "11899_TPS_MEP_OT_ATP_A-C_L5-L6")
                    {
                        levelsIDS_to_levelNumber["725700"] = "5lvl";
                        levelsIDS_to_levelNumber["725701"] = "6lvl";
                    }
                }
            }

            foreach (string lvlId in levelsIDS_to_levelNumber.Keys)
            {
                try
                {
                    spaceHeatLosses_fill(doc,
                        folderPath + levelsIDS_to_levelNumber[lvlId] + @"\"+ levelsIDS_to_levelNumber[lvlId] + fileName,
                        pathLogs,
                        Int32.Parse(lvlId));

                    TaskDialog.Show("technopark_heatLosses", "Успех" + Environment.NewLine +
                        "для уровня #" + levelsIDS_to_levelNumber[lvlId]);
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error",
                        "some problems with runtime" + Environment.NewLine +
                        "logs in " + pathLogs + Environment.NewLine +
                         "fileName " + folderPath + levelsIDS_to_levelNumber[lvlId] + fileName + Environment.NewLine +
                         ex.Message);
                }
            }



            return Result.Succeeded;
        }

        private void spaceHeatLosses_fill(Document _doc, string path, string log_path, int lvl_ID)
        {
            using (StreamWriter log = new StreamWriter(log_path))
            {
                Dictionary<string, string> spaceHeatLosses = File.ReadLines(path)
                        .Select(line => line.Split(','))
                       .ToDictionary(parts => parts[1].Trim(), parts => parts[2].Trim());

                using (Transaction tr = new Transaction(_doc, "CopyParameter"))
                {
                    tr.Start();

                    IList<Element> spaces = new FilteredElementCollector(_doc)
                        .OfCategory(BuiltInCategory.OST_MEPSpaces)
                        .WhereElementIsNotElementType().ToList()
                        .Where(x => x.LevelId.IntegerValue == lvl_ID)
                        .ToList();

                    // TODO - fill all spaces to zeros 0 before writing new values

                    foreach (var space in spaces)
                    {
                        try
                        {
                            int heatLooses = Int32.Parse(spaceHeatLosses[space.LookupParameter("Number").AsString()]);
                            space.LookupParameter("ADSK_Теплопотери").Set(UnitUtils.ConvertToInternalUnits(heatLooses, UnitTypeId.Watts));
                        }
                        catch (Exception)
                        {
                        }
                    }
                    tr.Commit();
                }
            }
        }


    }
}
