using ApplicationNamespace;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace app_SpaceHeatLossesFill
{
    public class SpaceHeatLossesFillViewModel : INotifyPropertyChanged
    {
        Document doc = RevitAPI.Document;

        private string _pathToFolderForReading;
        private string _pathToFileForReading;
        private Level _selectedlevel;
        private Phase _selectedPhase;
        private bool _isBusy;


        private string _status;
        private string _version;

        public SpaceHeatLossesFillViewModel()
        {
            _isBusy = false;
            _version = "ver_240920_0.60_MID";
            _pathToFolderForReading = @"C:\User\Desktop";
            CollectLevels(doc);
            CollectSpaceParameters(doc);

            WriteParameterToSpace = new RelayCommand(Dialog, TypeCheckingInputs);
            Dialog_Command = new RelayCommand(Dialog, x=> true);
        }

        public RelayCommand WriteParameterToSpace { get; set; }
        public RelayCommand Dialog_Command { get; set; }

        public string PathToFileForReading { 
            get => _pathToFileForReading;
            set
            {
                _pathToFileForReading = value;
                OnPropertyChanged();
            }
        }        
        public string Version { get => _version; }
        public string Status { 
            get => _status;
            set { 
                _status = value;
                OnPropertyChanged();
            }
        }
        public bool IsBusy { 
            get => _isBusy;
            set { 
                _isBusy=value;
                OnPropertyChanged();  
            } 
        }
        public Level SelectedLevel
        {
            get => _selectedlevel;
            set { 
            _selectedlevel = value;
            OnPropertyChanged();
            }
        }        
        public Phase SelectedPhase
        {
            get => _selectedPhase;
            set {
                _selectedPhase = value;
            OnPropertyChanged();
            }
        }


        public IList<Level> Levels { get; set; } = new List<Level>();
        public IList<Phase> Phases { get; set; } = new List<Phase>();
        public IList<Parameter> SpaceParameters { get; set; } = new List<Parameter>();


        private void CollectLevels(Document doc)
        {
            Levels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels)
                .ToElements().Select(x => x as Level).ToList();
        }
        private void CollectPhases(Document doc)
        {
            PhaseArray phases = doc.Phases;
            foreach (Phase phase in phases)
            {
                Phases.Add(phase);
            }
        }
        private void CollectSpaceParameters(Document doc)
        {
            Element element = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MEPSpaces)
                .WhereElementIsNotElementType().FirstOrDefault();
            ParameterSet parameterSet = element.Parameters;

            foreach (Parameter paramObj in parameterSet)
            {
                var parameter = (Parameter)paramObj;
                if (!parameter.IsReadOnly && parameter.StorageType != StorageType.ElementId)
                {
                    SpaceParameters.Add(parameter);
                }
            }
            SpaceParameters.OrderBy(x => x.Definition.Name).ToList();
        }

        private bool TypeCheckingInputs(object obj)
        {
            return SelectedLevel != null
                && PathToFileForReading != null
                ;
        }

        private void Dialog(object obj)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Выберите файл csv:";
            dlg.IsFolderPicker = false;
            dlg.InitialDirectory = _pathToFolderForReading;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = _pathToFolderForReading;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;


            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _pathToFileForReading = dlg.FileName;
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
