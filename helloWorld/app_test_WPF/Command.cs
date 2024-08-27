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

namespace app_test_WPF
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

            viewModel.CloseRequest += (s, e) => view.Close();
            viewModel.HideRequest += (s, e) => view.Hide();
            viewModel.ShowRequest += (s, e) => view.ShowDialog();

            view.ShowDialog();

            return Result.Succeeded;
        }
    }
}
