using ApplicationNamespace;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace app_test_WPF
{
    public class ViewModel : INotifyPropertyChanged
    {
        private readonly ISelectionFilter _selectionFilter;
        private string _inputInt = string.Empty;
        private string _inputString = string.Empty;
        private Parameter _selectedParameter;

        public ViewModel(Reference reference) 
        {
            Test_RelayCommand = new RelayCommand(PrintWelcomeMessage, TypeCheckingInputInt);
            CollectParameters(reference);
        }

        public RelayCommand Test_RelayCommand { get; set; }

        public string InputInt 
        { 
            get => _inputInt;
            set
            {
                _inputInt = value;
                OnPropertyChanged();
            } 
        }
        public string InputString
        {
            get => _inputString;
            set
            {
                _inputString = value;
                OnPropertyChanged();
            }
        }
        public Parameter SelectedParameter
        {
            get => _selectedParameter;
            set
            {
                _selectedParameter = value;
                OnPropertyChanged();
            }
        }
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();


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

        private bool TypeCheckingInputInt(object param)
        {
            return int.TryParse(InputInt, out _) 
                && SelectedParameter != null
                && InputString.Length > 0;
        }

        public void PrintWelcomeMessage(object obj)
        {
            try
            {
                // here we write our main functionality with transaction etc.
                TaskDialog.Show("title", InputString);
            }
            catch (Exception)
            {
                throw new NotImplementedException();
            }
        }



        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
        
        public event EventHandler HideRequest;
        private void RaiseHideRequest()
        {
            HideRequest?.Invoke(this, EventArgs.Empty);
        }
                
        public event EventHandler ShowRequest;
        private void RaiseShowRequest()
        {
            ShowRequest?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
