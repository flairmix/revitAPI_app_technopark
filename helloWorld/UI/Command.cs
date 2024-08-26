using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationNamespace;

namespace WPFApp
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (RevitAPI.UIApplication == null)
            {
                RevitAPI.Initialize(commandData);
            }

            var activeView = RevitAPI.Document.ActiveView;

            var reference = RevitAPI.UIDocument.Selection.PickObject(ObjectType.Element, "Выберите элемент для сбора параметров");

            var viewModel = new ViewModel(reference);
            var view = new MainView(viewModel);
            view.ShowDialog();

            return Result.Succeeded;
        }
    }
}
