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
using app_FacadePanelsInfo;
using ApplicationNamespace;

namespace app_SpaceHeatLossesFill
{
    [Transaction(TransactionMode.Manual)]
    public class SpaceHeatLossesFillCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            if (RevitAPI.UIApplication == null)
            {
                RevitAPI.Initialize(commandData);
            }

            var viewModel = new SpaceHeatLossesFillViewModel();
            var view = new SpaceHeatLossesFillView(viewModel);

            viewModel.CloseRequest += (s, e) => view.Close();
            viewModel.HideRequest += (s, e) => view.Hide();
            viewModel.ShowRequest += (s, e) => view.ShowDialog();

            view.ShowDialog();

            return Result.Succeeded;

        }

    }
}
