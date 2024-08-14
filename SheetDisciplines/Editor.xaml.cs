using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GongSolutions.Wpf.DragDrop;

namespace SheetDisciplines
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : Window, IDropTarget
    {
        Configuration _config;
        Settings _settings;

        public Editor(UIApplication app, Configuration config)
        {
            InitializeComponent();
            Owner = HwndSource.FromHwnd(app.MainWindowHandle).RootVisual as Window;
            _config = config;
            _settings = _config.GetSettings();
            AssignDisciplineListToTable(_settings.Disciplines);
        }

        void AssignDisciplineListToTable(DisciplineList disciplines)
        {
            DisciplineTable.ItemsSource = new ObservableCollection<Discipline>(disciplines.GetList());
        }

        DisciplineList ExtractDisciplineListFromTable()
        {
            var newList = new DisciplineList();
            foreach (Discipline discipline in DisciplineTable.ItemsSource)
            {
                newList.Add(discipline.Designator, discipline.Name);
            }
            return newList;
        }

        public void Save(object sender, RoutedEventArgs e)
        {
            _settings.Disciplines = ExtractDisciplineListFromTable();
            _config.SaveSettings(_settings);
            DialogResult = true;
            Close();
        }

        public void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Discipline)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        void IDropTarget.DragEnter(IDropInfo dropInfo) { }
        void IDropTarget.DragLeave(IDropInfo dropInfo) { }
        void IDropTarget.Drop(IDropInfo dropInfo) { }

        public void Export(object sender, RoutedEventArgs e)
        {
            var dialog = new FileSaveDialog("Tab-Delimited Text Files|*.tsv|Plain Text Files|*.txt|All Files|*.*");
            dialog.Title = "Save Sheet Disciplines File";

            if (dialog.Show() == ItemSelectionDialogResult.Confirmed)
            {
                var modelPath = dialog.GetSelectedModelPath();
                var path = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                ExtractDisciplineListFromTable().WriteFile(path);
            }
        }

        public void Import(object sender, RoutedEventArgs e)
        {
            var openDialog = new FileOpenDialog("Tab-Delimited Text Files|*.tsv|Plain Text Files|*.txt|All Files|*.*");
            openDialog.Title = "Browse for Sheet Disciplines File";

            if (openDialog.Show() == ItemSelectionDialogResult.Confirmed)
            {
                var modelPath = openDialog.GetSelectedModelPath();
                var path = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);

                try
                {
                    var disciplines = DisciplineList.FromFile(path);
                    AssignDisciplineListToTable(disciplines);
                }
                catch (Exception ex)
                {
                    var errorDialog = new TaskDialog("Import Sheet Disciplines")
                    {
                        TitleAutoPrefix = false,
                        MainInstruction = ex.ToString(),
                    };
                    errorDialog.Show();
                }
            }
        }

    }
}
