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


namespace Technopark.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class ADSK_Zone_by_Space : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            string p_ADSK_Zone = "ADSK_Зона";
            int phaseConvectorInt = 3;
            int worksetIdHeating = 440;
            int worksetIdCooling = 375;

            PhaseArray phases = doc.Phases;
            Phase phaseConvector = null;

            int countConvectorZoneChanges = 0;
            int countConvectorZoneNoChanges = 0;

            foreach (Phase phase in phases)
            {
                if (phase.Id.IntegerValue == phaseConvectorInt)
                {
                    phaseConvector = phase;
                }
            }

            using (Transaction tr = new Transaction(doc, "CopyParameter"))
            {
                tr.Start();

                IList<Element> convectors = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
                        .WhereElementIsNotElementType().ToList()
                        .Where(x => x.LookupParameter("ADSK_Обозначение") != null &&
                                 (x.WorksetId.IntegerValue == worksetIdCooling || x.WorksetId.IntegerValue == worksetIdHeating)).ToList();

                IList<Element> spaces = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_MEPSpaces)
                        .WhereElementIsNotElementType()
                        .ToList();

                for (int i = 0; i < convectors.Count(); i++)
                {

                    XYZ convectorLocationPoint = (convectors[i].Location as LocationPoint).Point;
                    XYZ convectorLocationPointZ = new XYZ(convectorLocationPoint.X, convectorLocationPoint.Y, convectorLocationPoint.Z + 3.0);

                    try
                    {
                        Space spaceWhereConvector = doc.GetSpaceAtPoint(convectorLocationPointZ, phaseConvector) as Space;
                        string spaceZone = spaceWhereConvector.LookupParameter(p_ADSK_Zone).AsString();

                        if (convectors[i].LookupParameter(p_ADSK_Zone).AsString() == spaceZone)
                        {
                            countConvectorZoneNoChanges++;
                        }
                        else { 
                            convectors[i].LookupParameter(p_ADSK_Zone).Set(spaceZone);
                            countConvectorZoneChanges++;
                        }

                    }
                    catch (Exception)
                    {

                    }
                }
                tr.Commit();
            }

            TaskDialog.Show("Result", 
                "Конвекторов без изменения параметра ADKS_Зона: " + countConvectorZoneNoChanges + Environment.NewLine +
                "Конвекторов c изменением параметра ADKS_Зона: " + countConvectorZoneChanges );

            return Result.Succeeded;
        }
    }
}
