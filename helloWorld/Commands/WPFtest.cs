using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFApllication;

namespace Technopark.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class WPFtest : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var view = new MainView();
            view.ShowDialog();

            return Result.Succeeded;
        }
    }
}
