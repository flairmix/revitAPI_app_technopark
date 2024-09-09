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



            string folderPath = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\";
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Выберите место сохранения файла:";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = folderPath;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = folderPath;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            string folder = null;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                folder = dlg.FileName;
            }

            var viewModel = new SpaceScheduleExportViewModel(folder);
            var view = new SpaceScheduleExportView(viewModel);


            viewModel.CloseRequest += (s, e) => view.Close();
            viewModel.HideRequest += (s, e) => view.Hide();
            viewModel.ShowRequest += (s, e) => view.ShowDialog();

            view.ShowDialog();

            return Result.Succeeded;
        }
    }
}
