using System;
using System.IO;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI.Events;
using System.Diagnostics;
using System.Linq;

namespace NewPanelNamespace
{
    public class CsAddPanel : IExternalApplication
    {
        // Both OnStartup and OnShutdown must be implemented as public method
        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel panel = RibbonPanel(application);

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            if (panel.AddItem(new PushButtonData("Technopark.Button_0", "Get Panels info", thisAssemblyPath, "Technopark.Commands.FacadePanels"))
                is PushButton button)
            {
                button.ToolTip = "Выгрузка фасадных панелей для отопления"
                    + Environment.NewLine
                    + "для расчета теплопотерь."
                    + Environment.NewLine
                    + "Наружные ограждения сопоставить с размещенными инженерными спэйсами";

                Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Revit.ico"));
                BitmapImage bitmapImage = new BitmapImage(uri);
                button.LargeImage = bitmapImage;

            }

            panel.AddSeparator();

            if (panel.AddItem(new PushButtonData("Technopark.Button_1", "Get Panels info_2", thisAssemblyPath, "Technopark.Commands.FacadePanels"))
                is PushButton button2)
            {
                button2.ToolTip = "Выгрузка фасадных панелей для отопления"
                    + Environment.NewLine
                    + "для расчета теплопотерь."
                    + Environment.NewLine
                    + "Наружные ограждения сопоставить с размещенными инженерными спэйсами";

                Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Revit.ico"));
                BitmapImage bitmapImage = new BitmapImage(uri);
                button2.LargeImage = bitmapImage;

            }

            panel.AddSeparator();

            AddStackedButtonGroup(panel);

            panel.AddSeparator();

            return Result.Succeeded;

        }

        public RibbonPanel RibbonPanel(UIControlledApplication application)
        {
            string tab = "MID_DEV";
            RibbonPanel ribbonPanel = null;

            try
            {
                application.CreateRibbonTab(tab);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            try
            {
                application.CreateRibbonPanel(tab, "Technopark_OT");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            List<RibbonPanel> panels = application.GetRibbonPanels(tab);
            foreach (RibbonPanel p in panels.Where(p => p.Name == "Technopark_OT"))
            {
                ribbonPanel = p;
            }

            return ribbonPanel;
        }

        private void AddStackedButtonGroup(RibbonPanel panel)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Revit.ico"));
            BitmapImage bitmapImage = new BitmapImage(uri);

            PushButtonData WireFrame = new PushButtonData("WireFrame", "WireFrame", thisAssemblyPath, "StackedButton.Commands.WireFrameCommand");
            WireFrame.ToolTip = "Change visual style to WireFrame";
            WireFrame.Image = bitmapImage;

            PushButtonData HiddenLine = new PushButtonData("HiddenLine", "HiddenLine", thisAssemblyPath, "StackedButton.Commands.HiddenLine");
            HiddenLine.ToolTip = "Change visual style to HiddenLine";
            HiddenLine.Image = bitmapImage;

            PushButtonData Shaded = new PushButtonData("Shaded", "Shaded", thisAssemblyPath, "StackedButton.Commands.Shaded");
            Shaded.ToolTip = "Change visual style to Shaded";
            Shaded.Image = bitmapImage;

            PushButtonData Realistic = new PushButtonData("Realistic", "Realistic", thisAssemblyPath, "StackedButton.Commands.Realistic");
            Realistic.ToolTip = "Change visual style to Realistic";
            Realistic.Image = bitmapImage;

            IList<RibbonItem> ribbonItemsA = panel.AddStackedItems(WireFrame, HiddenLine);
            IList<RibbonItem> ribbonItemsB = panel.AddStackedItems(Shaded, Realistic);

        }

        public Result OnShutdown(UIControlledApplication application)
          {
             // nothing to clean up in this simple case
             return Result.Succeeded;
          }
       }


   
}