using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackedButton.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Realistic : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            View activeView = doc.ActiveView;
            Parameter param = activeView.get_Parameter(BuiltInParameter.MODEL_GRAPHICS_STYLE);

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Change visual style");

                param.Set(6);

                trans.Commit();
            }


            return Result.Succeeded;
        }
    }
}
