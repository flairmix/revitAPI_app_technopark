using ApplicationNamespace;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;

namespace app_EquipmentPowerToSpace
{
    [Transaction(TransactionMode.Manual)]

    public class EquipmentPowerToSpaceCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (RevitAPI.UIApplication == null)
            {
                RevitAPI.Initialize(commandData);
            }

            //var reference = RevitAPI.UIDocument.Selection.PickObject(ObjectType.Element, "Выберете элемент для сбора параметров");

            var viewModel = new EquipmentPowerToSpaceViewModel();
            var view = new EquipmentPowerToSpaceView(viewModel);

            viewModel.CloseRequest += (s, e) => view.Close();
            viewModel.HideRequest += (s, e) => view.Hide();
            viewModel.ShowRequest += (s, e) => view.ShowDialog();

            view.ShowDialog();


            return Result.Succeeded;
        }
    }
}
