using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection.Emit;
using System.Collections.ObjectModel;
using ApplicationNamespace;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Analysis;

namespace app_EnergyAnalysis
{
    [Transaction(TransactionMode.Manual)]
    public class EnergyAnalysis : IExternalCommand
    {
        readonly string pathLogs = @"\\atptlp.local\dfs\MOS-TLP\GROUPS\ALLGEMEIN\06_HKLS\MID\logs\log_EnergyAnalysis.txt";
        string xmlFilePath = @"C:\Users\MID\Desktop\20139_XC_R_01_L1-RF_ATP_R22_MID.xml";
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (RevitAPI.UIApplication == null)
            {
                RevitAPI.Initialize(commandData);
            }
            Document doc = RevitAPI.Document;


            using (StreamWriter log = new StreamWriter(pathLogs))
            {
                try
                {
                    //gbXML_Model gbXml_model = new gbXML_Model(xmlFilePath);


                    IList<EnergyAnalysisSpace> analyticalSpaces = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_AnalyticSpaces)
                        .WhereElementIsNotElementType()
                        .Select(x=> x as EnergyAnalysisSpace).ToList();                      
                    
                    log.WriteLine($"analyticalSpaces.Count - {analyticalSpaces.Count()}");

                    IList<EnergyAnalysisSurface> analyticalSurfaces = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_AnalyticSurfaces)
                        .WhereElementIsNotElementType()
                        .Select(x=> x as EnergyAnalysisSurface).ToList();

                    log.WriteLine($"analyticalSurfaces.Count - {analyticalSurfaces.Count()}");

            

                    foreach (EnergyAnalysisSurface surface in analyticalSurfaces)
                    {
                        try
                        {
                            if (surface.Type == gbXMLSurfaceType.SurfaceAir)
                            {
                                log.WriteLine(surface.SurfaceName + " --- " + surface.SurfaceType.ToString());
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }




                }
                catch (Exception)
                {
                }
       
                



             






            }
            return Result.Succeeded;
        }

    }
}
