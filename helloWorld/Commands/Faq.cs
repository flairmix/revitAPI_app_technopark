using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technopark.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class FAQ : IExternalCommand
    {
        string textFAQ = "в далекой далекой галактике... .";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Информация", textFAQ);

            return Result.Succeeded;
        }
    }
}
