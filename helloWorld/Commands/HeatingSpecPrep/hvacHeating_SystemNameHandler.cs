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

namespace Technopark.HeatingSpecPrep.Commands
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

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            Autodesk.Revit.DB.View activeView = doc.ActiveView;
            //Parameter param = activeView.get_Parameter(BuiltInParameter.MODEL_GRAPHICS_STYLE);

            string userlog = doc.PathName;
            string[] userlog_data = userlog.Split(new char[] { '\\' });
            string datelog = DateTime.Now.ToLocalTime().ToString("yyyyMMdd_HHmmss");
            string docBuilding = doc.Title.ToString().Replace("_" + doc.Application.Username, "").ToList().Last().ToString();








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
