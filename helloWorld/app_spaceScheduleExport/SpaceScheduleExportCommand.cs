using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System.IO;
using ApplicationNamespace;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Controls;


namespace app_spaceScheduleExport
{
    [Transaction(TransactionMode.Manual)]
    public class SpaceScheduleExportCommand : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (RevitAPI.UIApplication == null)
            {
                RevitAPI.Initialize(commandData);
            }


            var viewModel = new SpaceScheduleExportViewModel ();
            var view = new SpaceScheduleExportView (viewModel);

            viewModel.CloseRequest += (s, e) => view.Close();
            viewModel.HideRequest += (s, e) => view.Hide();
            viewModel.ShowRequest += (s, e) => view.ShowDialog();

            view.ShowDialog();

            return Result.Succeeded;
        }
    }
}
