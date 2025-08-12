using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using Newtonsoft.Json;
using System.IO;
using Formatting = Newtonsoft.Json.Formatting;

namespace WinCalib_1
{
    public class Param
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public Param(string name, string value) 
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Param> ParamsList { get; } = new ObservableCollection<Param>();
        public ICommand AddParamCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SaveJsonAndExitCommand { get; }


        private string _paramName;
        public string ParamName
        {
            get { return _paramName; }
            set
            {
                _paramName = value;
                OnPropertyChanged(nameof(ParamName));
            }
        }

        private string _paramValue;

        public string ParamValue
        {
            get { return _paramValue; }
            set
            {
                _paramValue = value;
                OnPropertyChanged(nameof(ParamValue));
            }
        }

        private string _errorStr;

        public string ErrorStr
        {
            get { return _errorStr; }
            set
            {
                _errorStr = value;
                OnPropertyChanged(nameof(ErrorStr));
            }
        }

        private string _selectedResult;

        public string SelectedResult
        {
            get => _selectedResult;
            set
            {
                if (_selectedResult != value)
                {
                    _selectedResult = value;
                    OnPropertyChanged(nameof(SelectedResult));
                }
            }
        }

        public MainViewModel()
        {
            AddParamCommand = new RelayCommand(AddParamToList);
            CancelCommand = new RelayCommand(OnCancel);
            SaveJsonAndExitCommand = new RelayCommand(OnSaveJsonAndExit);
        }

        private void OnCancel(object parameter)
        {
            Application.Current.Shutdown();
        }

        private void OnSaveJsonAndExit(object parameter)
        {
            //save json in ../Output folder

            ParamsList.Add(new Param("Result", SelectedResult));
            ParamsList.Add(new Param("Error", ErrorStr));

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in ParamsList)
            {
                dict[p.Name] = p.Value;
            }

            string json = JsonConvert.SerializeObject(dict, Formatting.Indented);

            string outputFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\Output"));
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            File.WriteAllText(Path.Combine(outputFolder, "output.json"), json);

            Application.Current.Shutdown();
        }
        private void AddParamToList(object parameter)
        {
            ParamsList.Add(new Param(ParamName, ParamValue));
            ParamName = string.Empty;
            ParamValue = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
