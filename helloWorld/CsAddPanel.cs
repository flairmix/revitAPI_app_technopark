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


            if (panel.AddItem(new PushButtonData("FAQ", "Info", thisAssemblyPath, "Technopark.Commands.FAQ"))
                is PushButton buttonFAQ)
            {
                buttonFAQ.ToolTip = "Информация о работе плагина";

                Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "ImageFAQ.png"));
                BitmapImage bitmapImage = new BitmapImage(uri);
                buttonFAQ.LargeImage = bitmapImage;
            }

            panel.AddSeparator();

            if (panel.AddItem(new PushButtonData("Technopark.Button_0", "Get Panels info", thisAssemblyPath, "Technopark.Commands.FacadePanels"))
                is PushButton button)
            {
                button.ToolTip = "Выгрузка фасадных панелей для отопления"
                    + Environment.NewLine
                    + "для расчета теплопотерь."
                    + Environment.NewLine
                    + "Наружные ограждения сопоставить с размещенными инженерными спэйсами";

                Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image_Emoji1.png"));
                BitmapImage bitmapImage = new BitmapImage(uri);
                button.LargeImage = bitmapImage;

            }

            panel.AddSeparator();

            AddStackedButtonGroup(panel);

            panel.AddSeparator();


            if (panel.AddItem(new PushButtonData("Heating.Heating_1", "Heating_1", thisAssemblyPath, "HeatingSpecPrep.Commands.hvacHeating_SystemNameHandler"))
                is PushButton buttonHeating_1)
            {
                buttonHeating_1.ToolTip = "Спецификация отопления. Перезапись имени системы";

                Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image_Emoji1.png"));
                BitmapImage bitmapImage = new BitmapImage(uri);
                buttonHeating_1.LargeImage = bitmapImage;

            }


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

            Uri uri_2 = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image2.png"));
            BitmapImage bitmapImage_2 = new BitmapImage(uri_2);            
            
            Uri uri_5lvl = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image5lvl.png"));
            BitmapImage bitmapImage_5lvl = new BitmapImage(uri_5lvl);

            Uri uri_6lvl = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image6lvl.png"));
            BitmapImage bitmapImage_6lvl = new BitmapImage(uri_6lvl);

            PushButtonData SpaceHeatLossesFill = new PushButtonData("Technopark.Button_3", 
                "Заполнить теплопотери", 
                thisAssemblyPath, 
                "Technopark.Commands.SpaceHeatLossesFill");
            SpaceHeatLossesFill.ToolTip = "Запись теплопотерь из расчета в Spaces";
            SpaceHeatLossesFill.Image = bitmapImage_2;

            PushButtonData ConvectorsPower_to_space_5lvl = new PushButtonData("ConvectorsPower_to_space_5lvl", 
                "ConvectorsPower_to_space_5lvl", 
                thisAssemblyPath, 
                "StackedButton.Commands.ConvectorsPower_to_space_5lvl");
            ConvectorsPower_to_space_5lvl.ToolTip = "Запись мощности конвекторов из приборов в Space";
            ConvectorsPower_to_space_5lvl.Image = bitmapImage_5lvl;

            PushButtonData ConvectorsPower_to_space_6lvl = new PushButtonData("ConvectorsPower_to_space_6lvl", 
                "ConvectorsPower_to_space_6lvl", 
                thisAssemblyPath, 
                "StackedButton.Commands.ConvectorsPower_to_space_6lvl");
            ConvectorsPower_to_space_6lvl.ToolTip = "Запись мощности конвекторов из приборов в Space";
            ConvectorsPower_to_space_6lvl.Image = bitmapImage_6lvl;

            PushButtonData Shaded = new PushButtonData("Shaded", "Shaded", thisAssemblyPath, "StackedButton.Commands.Shaded");
            Shaded.ToolTip = "Change visual style to Shaded";
            Shaded.Image = bitmapImage;

            PushButtonData Realistic = new PushButtonData("Realistic", "Realistic", thisAssemblyPath, "StackedButton.Commands.Realistic");
            Realistic.ToolTip = "Change visual style to Realistic";
            Realistic.Image = bitmapImage;

            IList<RibbonItem> ribbonItemsA = panel.AddStackedItems(SpaceHeatLossesFill, ConvectorsPower_to_space_5lvl, ConvectorsPower_to_space_6lvl);
            //IList<RibbonItem> ribbonItemsB = panel.AddStackedItems(Shaded, Realistic);

        }

        public Result OnShutdown(UIControlledApplication application)
          {
             // nothing to clean up in this simple case
             return Result.Succeeded;
          }
       }


   
}