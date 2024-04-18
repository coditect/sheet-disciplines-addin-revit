using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace SheetDisciplines
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    internal class EditCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData data, ref string message, ElementSet elements)
        {
            var document = data.Application.ActiveUIDocument.Document;
            using (var t = new Transaction(document, "Edit sheet disciplines"))
            {
                try
                {
                    t.Start();

                    var config = new Configuration(document);
                    var editor = new Editor(config);
                    var response = editor.ShowDialog();

                    if (response == true)
                    {
                        Updater.UpdateAllSheets(config);
                        t.Commit();
                        return Result.Succeeded;
                    }
                    else
                    {
                        t.RollBack();
                        return Result.Cancelled;
                    }
                }
                catch (Exception e)
                {
                    var dialog = new TaskDialog("Sheet Disciplines");
                    dialog.TitleAutoPrefix = false;
                    dialog.MainInstruction = "The sheet discipline updater ran into trouble.";
                    dialog.MainContent = e.ToString();
                    dialog.Show();

                    t.RollBack();
                    return Result.Failed;
                }
            }
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    internal class UpdateAllCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData data, ref string message, ElementSet elements)
        {
            var document = data.Application.ActiveUIDocument.Document;
            using (var t = new Transaction(document, "Update discipline of all sheets"))
            {
                try
                {
                    t.Start();

                    var config = new Configuration(document);
                    Updater.UpdateAllSheets(config);

                    t.Commit();
                    return Result.Succeeded;
                }
                catch (Exception e)
                {
                    var dialog = new TaskDialog("Sheet Disciplines");
                    dialog.TitleAutoPrefix = false;
                    dialog.MainInstruction = "The sheet discipline updater ran into trouble.";
                    dialog.MainContent = e.ToString();
                    dialog.Show();

                    t.RollBack();
                    return Result.Failed;
                }
            }
        }
    }
}
