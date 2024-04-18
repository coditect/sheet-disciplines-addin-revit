using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System.Reflection;

namespace SheetDisciplines
{
    /// <summary>
    /// The Application class defines event listeners that handle registration and unregistration of the Updater.
    /// </summary>
    public class Application : IExternalApplication
    {
        private AddInId _addInId;

        public Result OnStartup(UIControlledApplication application)
        {
            _addInId = application.ActiveAddInId;

            // Add event listeners
            application.ControlledApplication.DocumentOpened += DocumentOpened;
            application.ControlledApplication.DocumentCreated += DocumentOpened;
            application.ControlledApplication.DocumentClosing += DocumentClosing;

            // Add toolbar buttons
            var panel = application.CreateRibbonPanel(Tab.AddIns, "Sheet Disciplines");
            var thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var settingsButton = new PushButtonData("Edit Disciplines", "Edit Disciplines", thisAssemblyPath, "SheetDisciplines.EditCommand");
            var updateAllButton = new PushButtonData("Update All Sheets", "Update All Sheets", thisAssemblyPath, "SheetDisciplines.UpdateAllCommand");
            panel.AddStackedItems(settingsButton, updateAllButton);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private void DocumentOpened(object sender, RevitAPIPostDocEventArgs e)
        {
            var updater = new Updater(_addInId);
            var id = updater.GetUpdaterId();
            UnregisterUpdaterIfRegistered(id);
            
            UpdaterRegistry.RegisterUpdater(updater);

            ElementCategoryFilter sheetFilter = new ElementCategoryFilter(BuiltInCategory.OST_Sheets);
            UpdaterRegistry.AddTrigger(id, sheetFilter, Element.GetChangeTypeElementAddition());
            UpdaterRegistry.AddTrigger(id, sheetFilter, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.SHEET_NUMBER)));
        }

        private void DocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            var updater = new Updater(_addInId);
            var id = updater.GetUpdaterId();
            UnregisterUpdaterIfRegistered(id);
        }

        private void UnregisterUpdaterIfRegistered(UpdaterId id)
        {
            if (UpdaterRegistry.IsUpdaterRegistered(id))
            {
                UpdaterRegistry.RemoveAllTriggers(id);
                UpdaterRegistry.UnregisterUpdater(id);
            }
        }
    }
}
