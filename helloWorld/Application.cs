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
using System.Security.Cryptography;

namespace ApplicationNamespace
{
    public class Application : IExternalApplication
    {
        
        // Both OnStartup and OnShutdown must be implemented as public method
        public Result OnStartup(UIControlledApplication application)
        {

            RibbonPanel panel = RibbonPanel(application);
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;


            if (panel.AddItem(new PushButtonData("FAQ", "Info", thisAssemblyPath, "ApplicationNamespace.FAQ"))
                is PushButton buttonFAQ)
            {
                buttonFAQ.ToolTip = "Информация о работе плагина";

                Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "ImageFAQ.png"));
                BitmapImage bitmapImage = new BitmapImage(uri);
                buttonFAQ.LargeImage = bitmapImage;
            }

            panel.AddSeparator();

            if (panel.AddItem(new PushButtonData("Technopark.Button_0", "Get Panels info", thisAssemblyPath, "app_FacadePanelsInfo.FacadePanelsInfoCommand"))
                is PushButton button)
            {
                button.ToolTip = "Выгрузка инфо о наружных ограждениях"
                    + Environment.NewLine
                    + "для расчета теплопотерь / теплоппоступлений."
                    + Environment.NewLine
                    + "Наружные ограждения сопоставить с размещенными инженерными спэйсами";

                Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image_Emoji1.png"));
                BitmapImage bitmapImage = new BitmapImage(uri);
                button.LargeImage = bitmapImage;
            }

            panel.AddSeparator();

            if (panel.AddItem(new PushButtonData("Technopark.Button_1", "Get Spaces info", thisAssemblyPath, "app_spaceScheduleExport.SpaceScheduleExportCommand"))
                is PushButton SpaceScheduleExportButton)
            {
                SpaceScheduleExportButton.ToolTip = "Выгрузка информации о пространствах для расчета теплопотерь";

                Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image_Emoji_Spaces.png"));
                BitmapImage bitmapImage = new BitmapImage(uri);
                SpaceScheduleExportButton.LargeImage = bitmapImage;
            }

            panel.AddSeparator();


            if (panel.AddItem(new PushButtonData("Technopark.Button_2", "Fill Spaces", thisAssemblyPath, "app_SpaceHeatLossesFill.SpaceHeatLossesFill"))
                is PushButton buttonSpaceHeatLossesFill)
            {
                buttonSpaceHeatLossesFill.ToolTip = "Запись теплопотерь из расчета в Spaces" +
                    Environment.NewLine + "запись из папки:" +
                    @"\\atptlp.local\dfs\MOS - TLP\PROJEKTE\11899\05_HAUSTECHNIK\01_Planung\05_Ausfuehrungsplanung\01_HVaC\03_Calculations\08_Отопление и теплоснабжение\MID_расчет теплопотерь" +
                    Environment.NewLine + "из подпапки с названием с соответствующим уровнем";


                Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image_Emoji2.png"));
                BitmapImage bitmapImage = new BitmapImage(uri);
                buttonSpaceHeatLossesFill.LargeImage = bitmapImage;
            }


            //AddStackedButtonGroup(panel);

            panel.AddSeparator();

            if (panel.AddItem(new PushButtonData("Technopark.Button_3", "ConvectorZone", thisAssemblyPath, "app_EquipmentFindingZone.EquipmentFindingZone"))
                is PushButton button_ADSK_Zone_by_Space)
            {
                button_ADSK_Zone_by_Space.ToolTip = "Запись ADKS_Зона в конвекторы по значению из Space, в котором они находятся";

                Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image_Emoji3.png"));
                BitmapImage bitmapImage = new BitmapImage(uri);
                button_ADSK_Zone_by_Space.LargeImage = bitmapImage;
            }

            panel.AddSeparator();

            if (panel.AddItem(new PushButtonData("Technopark.Button_4", "power_to_space", thisAssemblyPath, "app_EquipmentPowerToSpace.EquipmentPowerToSpaceCommand"))
                is PushButton button_power_to_space)
            {
                button_power_to_space.ToolTip = "Запись мощности конвекторов из приборов в Space";

                Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image_Emoji4.png"));
                BitmapImage bitmapImage = new BitmapImage(uri);
                button_power_to_space.LargeImage = bitmapImage;
            }

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
                application.CreateRibbonPanel(tab, "Technopark_OT_XC");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            List<RibbonPanel> panels = application.GetRibbonPanels(tab);
            foreach (RibbonPanel p in panels.Where(p => p.Name == "Technopark_OT_XC"))
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
            
            Uri uri_1lvl = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image1lvl.png"));
            BitmapImage bitmapImage_1lvl = new BitmapImage(uri_1lvl);    
                                    
            Uri uri_2lvl = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image2lvl.png"));
            BitmapImage bitmapImage_2lvl = new BitmapImage(uri_2lvl);    
                                    
            Uri uri_3lvl = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image3lvl.png"));
            BitmapImage bitmapImage_3lvl = new BitmapImage(uri_3lvl);    
                                    
            Uri uri_4lvl = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image4lvl.png"));
            BitmapImage bitmapImage_4lvl = new BitmapImage(uri_4lvl);    
                        
            Uri uri_5lvl = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image5lvl.png"));
            BitmapImage bitmapImage_5lvl = new BitmapImage(uri_5lvl);

            Uri uri_6lvl = new Uri(Path.Combine(Path.GetDirectoryName(thisAssemblyPath), "Resources", "Image6lvl.png"));
            BitmapImage bitmapImage_6lvl = new BitmapImage(uri_6lvl);


            PushButtonData ConvectorsPower_to_space_1lvl = new PushButtonData("ConvectorsPower_to_space_1lvl", 
                "ConvectorsPower_to_space_1lvl", 
                thisAssemblyPath,
                "Technopark.Commands.ConvectorsPower_to_space_1lvl");
            ConvectorsPower_to_space_1lvl.ToolTip = "Запись мощности конвекторов из приборов в Space";
            ConvectorsPower_to_space_1lvl.Image = bitmapImage_1lvl;

            PushButtonData ConvectorsPower_to_space_2lvl = new PushButtonData("ConvectorsPower_to_space_2lvl", 
                "ConvectorsPower_to_space_2lvl", 
                thisAssemblyPath,
                "Technopark.Commands.ConvectorsPower_to_space_2lvl");
            ConvectorsPower_to_space_2lvl.ToolTip = "Запись мощности конвекторов из приборов в Space";
            ConvectorsPower_to_space_2lvl.Image = bitmapImage_2lvl;

            PushButtonData ConvectorsPower_to_space_3lvl = new PushButtonData("ConvectorsPower_to_space_3lvl", 
                "ConvectorsPower_to_space_3lvl", 
                thisAssemblyPath,
                "Technopark.Commands.ConvectorsPower_to_space_3lvl");
            ConvectorsPower_to_space_3lvl.ToolTip = "Запись мощности конвекторов из приборов в Space";
            ConvectorsPower_to_space_3lvl.Image = bitmapImage_3lvl;

            PushButtonData ConvectorsPower_to_space_4lvl = new PushButtonData("ConvectorsPower_to_space_4lvl", 
                "ConvectorsPower_to_space_4lvl", 
                thisAssemblyPath,
                "Technopark.Commands.ConvectorsPower_to_space_4lvl");
            ConvectorsPower_to_space_4lvl.ToolTip = "Запись мощности конвекторов из приборов в Space";
            ConvectorsPower_to_space_4lvl.Image = bitmapImage_4lvl;

            PushButtonData ConvectorsPower_to_space_5lvl = new PushButtonData("ConvectorsPower_to_space_5lvl", 
                "ConvectorsPower_to_space_5lvl", 
                thisAssemblyPath,
                "Technopark.Commands.ConvectorsPower_to_space_5lvl");
            ConvectorsPower_to_space_5lvl.ToolTip = "Запись мощности конвекторов из приборов в Space";
            ConvectorsPower_to_space_5lvl.Image = bitmapImage_5lvl;

            PushButtonData ConvectorsPower_to_space_6lvl = new PushButtonData("ConvectorsPower_to_space_6lvl", 
                "ConvectorsPower_to_space_6lvl", 
                thisAssemblyPath,
                "Technopark.Commands.ConvectorsPower_to_space_6lvl");
            ConvectorsPower_to_space_6lvl.ToolTip = "Запись мощности конвекторов из приборов в Space";
            ConvectorsPower_to_space_6lvl.Image = bitmapImage_6lvl;

            PushButtonData Shaded = new PushButtonData("Shaded", "Shaded", thisAssemblyPath, "StackedButton.Commands.Shaded");
            Shaded.ToolTip = "Change visual style to Shaded";
            Shaded.Image = bitmapImage;

            PushButtonData Realistic = new PushButtonData("Realistic", "Realistic", thisAssemblyPath, "StackedButton.Commands.Realistic");
            Realistic.ToolTip = "Change visual style to Realistic";
            Realistic.Image = bitmapImage;

            IList<RibbonItem> ribbonItemsA = panel.AddStackedItems(ConvectorsPower_to_space_1lvl, ConvectorsPower_to_space_2lvl, ConvectorsPower_to_space_3lvl);
            IList<RibbonItem> ribbonItemsB = panel.AddStackedItems(ConvectorsPower_to_space_4lvl, ConvectorsPower_to_space_5lvl, ConvectorsPower_to_space_6lvl);

        }

        public Result OnShutdown(UIControlledApplication application)
          {
             // nothing to clean up in this simple case
             return Result.Succeeded;
          }
       }


   
}