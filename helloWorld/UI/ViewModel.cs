using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationNamespace
{
    public class ViewModel
    {
        public ViewModel(Reference reference) 
        {
            CollectParameters(reference);
        }
        public int inputInt { get; set; }
        public string inputString { get; set; }
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public Parameter SelectedParameter { get; set; }


        private void CollectParameters(Reference reference) { 
            var element = RevitAPI.Document.GetElement(reference);
            ParameterSet parameterSet = element.Parameters;
            foreach (Parameter paramObj in parameterSet)
            {
                var parameter = (Parameter)paramObj;
                if(!parameter.IsReadOnly && parameter.StorageType != StorageType.ElementId)
                {
                    Parameters.Add(parameter);
                }
            }
        }
    }
}
