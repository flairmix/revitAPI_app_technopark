using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationNamespace
{
    public static class RevitAPI
    {
        public static UIApplication UIApplication { get; set; }
        public static UIDocument UIDocument { get => UIApplication.ActiveUIDocument; }
        public static Document Document { get => UIDocument.Document; }

        public static void Initialize (ExternalCommandData commandData)
        {
            UIApplication = commandData.Application;
        }
    }
}
