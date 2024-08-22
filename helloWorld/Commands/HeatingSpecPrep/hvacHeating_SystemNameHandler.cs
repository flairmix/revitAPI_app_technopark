using Autodesk.Revit;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Autodesk.Revit.DB.Visual;

namespace HeatingSpecPrep.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class hvacHeating_SystemNameHandler : IExternalCommand
    {
        readonly List<string> worksets_user_black_list = new List<string>(2) { "310_Ventilation", "315_Smoke removal" };

        readonly List<BuiltInCategory> categories = new List<BuiltInCategory>(6) 
        {
            BuiltInCategory.OST_MechanicalEquipment,
            BuiltInCategory.OST_PipeAccessory,
            BuiltInCategory.OST_PipeCurves,
            BuiltInCategory.OST_PipeFitting,
            BuiltInCategory.OST_FlexPipeCurves,
            BuiltInCategory.OST_PipeInsulations
        };

        readonly Dictionary<string, string> groupNew = new Dictionary<string, string>()
        {
            {"OST_MechanicalEquipment", "1"},
            {"OST_PipeAccessory", "2"},
            {"OST_PipeCurves", "3"},
            {"OST_PipeFitting", "4"},
            {"OST_FlexPipeCurves", "5"},
            {"OST_PipeInsulations", "6"}
        };

        Dictionary<string, string> systemsNew = new Dictionary<string, string>();

        Dictionary<string, int> systemsCount = new Dictionary<string, int>()
        {
            {"OST_MechanicalEquipment", 0},
            {"OST_PipeAccessory", 0},
            {"OST_PipeCurves", 0},
            {"OST_PipeFitting", 0},
            {"OST_FlexPipeCurves", 0},
            {"OST_PipeInsulations", 0}
        };
            
        readonly string logFolder = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\logs_systemName\";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document; 
            Autodesk.Revit.DB.View activeView = doc.ActiveView;
            //Parameter param = activeView.get_Parameter(BuiltInParameter.MODEL_GRAPHICS_STYLE);

            string username = doc.Title.Split('_').ToList().Last();
            string projectNumber = doc.Title.Split('_').ToList().First();
            string datelog = DateTime.Now.ToLocalTime().ToString("yyyyMMdd_HHmmss");


            using (StreamWriter log = new StreamWriter(logFolder + datelog + "_" + username + "_" + "log.txt"))
            {
                log.WriteLine("hvacHeating_SystemNameHandler");
                log.WriteLine("username " + username);
                log.WriteLine("date " + datelog);
                log.WriteLine("projectNumber " + projectNumber);
                log.WriteLine("doc.Title " + doc.Title);

                // reading csv with prepared Names of heating piping systems 
                string path = @"P:\MOS-TLP\PROJEKTE\11692\05_HAUSTECHNIK\01_Planung\04_Genehmigungsplanung\01_HVaC\01_Documents\99_Others\";
                string file = "11692_Dictionary_SysName_" + "A_heating";

                List<string> listA = new List<string>();
                List<string> listB = new List<string>();

                Dictionary <string, string> keyValuePairs = new Dictionary<string, string>();
                
                //TO DO - read source file with systems 

                /*            using (var reader = new System.IO.StreamReader(path + file + ".csv"))
                            {

                                while (!reader.EndOfStream)
                                {
                                    var values = reader.ReadLine().Split(',');

                                    if (values[0] != "" && !systemsNew.Keys.Contains(values[0]))
                                    {
                                        systemsNew.Add(values[0], values[1]);

                                        listA.Add(values[0]);
                                        listB.Add(values[1]);
                                    }
                                }
                            }*/


                foreach (var category in categories)
                {
                    string temp_group = category.ToString();
                    List<Element> filtered_elements = Select_elements(doc, worksets_user_black_list, category);

                    // write parameter "ИмяСистемы"
                    using (Transaction tr = new Transaction(doc, "tr"))
                    {
                        tr.Start();

                        foreach (Element element in filtered_elements)
                        {
                            try
                            {
                                if (element.Category.Name == "Mechanical Equipment")
                                {
                                    Set_parameter_SystemName_mechanical(doc, element, "ИмяСистемы", systemsNew);
                                    systemsCount[temp_group]++;
                                }
                                else
                                {
                                    element.LookupParameter("ИмяСистемы").Set(systemsNew[element.LookupParameter("System Name").AsString()]);
                                    systemsCount[temp_group]++;
                                }
                            }
                            catch (Exception e)
                            {
                                log.WriteLine("FAIL   " + e + " - element - " + element.Name + "\n");
                            }
                        }

                        tr.Commit();
                    }

                    // group elements by category for schedules
                    using (Transaction tr = new Transaction(doc, "tr"))
                    {
                        tr.Start();
                        foreach (Element element in filtered_elements)
                        {
                            try
                            {
                                element.LookupParameter("ADSK_Группирование").Set(groupNew[temp_group]);
                            }
                            catch (Exception e)
                            {
                                log.WriteLine("!!!FAIL!!!!   " + e + " - element - " + element.Name);
                            }
                        }
                        tr.Commit();
                    }
                }

                // show TaskDialog window with count of handled elements 
                TaskDialog td = new TaskDialog("nameTD");
                td.MainInstruction = "";
                List<string> keysSystemsCount = systemsCount.Keys.ToList();

                foreach (var key in keysSystemsCount)
                {
                    td.MainInstruction += key + " --- " + systemsCount[key] + "шт.\n";
                }
                td.MainInstruction += doc.Title.ToString() + "\n";
                td.MainInstruction += doc.Title.ToString().Replace("_" + doc.Application.Username, "");
                td.MainInstruction += doc.Title.ToString().Replace("_" + doc.Application.Username, "") + "\n";
                td.MainInstruction += doc.Title.ToString().Replace("_" + doc.Application.Username, "").ToList().Last().ToString() + "\n";
                td.Show();
            }


            return Result.Succeeded;
        }


        /// <summary>
        /// Selection of elements by BuiltInCategory with blacklist workset setup
        /// </summary>
        /// <param name="Document _doc"></param>
        /// <param name="List<string> worksets_user_black_list"></param>
        /// <param name="BuiltInCategory category"></param>
        /// <returns>List<Elements></returns>
        private List<Element> Select_elements(Document _doc, List<string> worksets_user_black_list, BuiltInCategory category)
        {
            List<Workset> worksets_user = new FilteredWorksetCollector(_doc)
                .OfKind(WorksetKind.UserWorkset)
                .ToList();

            IEnumerable<WorksetId> mass_workset_target_ids = from workset in worksets_user where !worksets_user_black_list.Contains(workset.Name) select workset.Id;

            FilteredElementCollector elements = new FilteredElementCollector(_doc)
                .OfCategory(category)
                .WhereElementIsNotElementType();

            List<Element> filtered_elements = new List<Element>();

            foreach (var element in elements)
            {
                if (!worksets_user_black_list.Contains(element.WorksetId.ToString()))
                {
                    filtered_elements.Add(element);
                }
            }

            return filtered_elements;
        }

        /// <summary>
        /// Function that write "ИмяСистемы" parameter special for Mechanical Equipment category
        /// </summary>
        private void Set_parameter_SystemName_mechanical(Document _doc, Element element, string ParamStr, Dictionary<string, string> systemsNew)
        {
            List<string> keysSystemName = systemsNew.Keys.ToList();

            if (keysSystemName.Any(s => element.LookupParameter("System Name").AsString().Contains(s)))
            {
                foreach (var key in keysSystemName)
                {
                    if (element.LookupParameter("System Name").AsString().Contains(key))
                    {
                        element.LookupParameter(ParamStr).Set(systemsNew[key]);
                    }
                }
            }
        }



    }
}
