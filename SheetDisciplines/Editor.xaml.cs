using System.Collections.ObjectModel;
using System.Windows;
using Autodesk.Revit.UI;
using GongSolutions.Wpf.DragDrop;

namespace SheetDisciplines
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Editor : Window, IDropTarget
    {
        Configuration _config;
        Settings _settings;

        public Editor(Configuration config)
        {
            InitializeComponent();
            _config = config;
            _settings = _config.GetSettings();
            DisciplineTable.ItemsSource = new ObservableCollection<Discipline>(_settings.Disciplines.GetList());
        }

        public void Save(object sender, RoutedEventArgs e)
        {
            var newList = new DisciplineList();
            foreach (Discipline discipline in DisciplineTable.ItemsSource)
            {
                newList.Add(discipline.Designator, discipline.Name);
            }

            _settings.Disciplines = newList;
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
            Discipline sourceItem = dropInfo.Data as Discipline;
            if (sourceItem != null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }
            
        }

        void IDropTarget.DragEnter(IDropInfo dropInfo) { }
        void IDropTarget.DragLeave(IDropInfo dropInfo) { }
        void IDropTarget.Drop(IDropInfo dropInfo) { }

        void DoesNothing(object sender, RoutedEventArgs e)
        {
            var dialog = new TaskDialog("Sheet Disciplines");
            dialog.TitleAutoPrefix = false;
            dialog.MainInstruction = "Sorry, this button doesn't do anything.";
            dialog.Show();
        }

    }
}
