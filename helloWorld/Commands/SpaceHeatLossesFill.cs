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
            string folderPath = @"\\atptlp.local\dfs\MOS-TLP\PROJEKTE\11899\05_HAUSTECHNIK\01_Planung\05_Ausfuehrungsplanung\01_HVaC\03_Calculations\08_Отопление и теплоснабжение\MID_расчет теплопотерь\Нагрузки отопления в ревит\";
            string fileName = "lvl_MID_HeatLosses_to_REVIT.csv";
            
            // TO DO  -- add all IDs
            Dictionary <string, string> levelsIDS_to_levelNumber = new Dictionary<string, string>()
            {
                {"725700", "5"},
                {"725701", "6"},
            };

            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            Autodesk.Revit.DB.View activeView = doc.ActiveView;

            foreach (string lvlId in levelsIDS_to_levelNumber.Keys)
            {
                try
                {
                    spaceHeatLosses_fill(doc,
                        folderPath + levelsIDS_to_levelNumber[lvlId] + fileName,
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
