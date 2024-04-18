using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SheetDisciplines
{
    public class Updater : IUpdater
    {
        const string Id = "EC25D3E1-3BB2-4164-B4B3-3CD46D0BB22D";
        static UpdaterId _updaterId;

        public Updater(AddInId addInId)
        {
            _updaterId = new UpdaterId(addInId, new Guid(Id));
        }

        public void Execute(UpdaterData data)
        {
            var document = data.GetDocument();

            try
            {
                var config = new Configuration(document);
                config.EnsureRequiredParametersAreDefined();
                UpdateSheets(config, data.GetAddedElementIds());
                UpdateSheets(config, data.GetModifiedElementIds());
            }
            catch (Exception e)
            {
                var dialog = new TaskDialog("Sheet Discipline Updater");
                dialog.TitleAutoPrefix = false;
                dialog.MainInstruction = "The sheet discipline updater ran into trouble.";
                dialog.MainContent = e.ToString();
                dialog.Show();
            }
        }

        public string GetAdditionalInformation()
        {
            return "Updates a “Sheet Discipline” parameter based on the designator at the beginning of the sheet's number";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.Views;
        }

        public UpdaterId GetUpdaterId()
        {
            return _updaterId;
        }

        public string GetUpdaterName()
        {
            return "Sheet Discipline Updater";
        }

        public static void UpdateSheets(Configuration config, System.Collections.Generic.ICollection<ElementId> ids)
        {
            var settings = config.GetSettings();
            var disciplines = settings.Disciplines;

            foreach (ElementId id in ids)
            {
                ViewSheet sheet = config.Document.GetElement(id) as ViewSheet;
                var sheetNumber = sheet.get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString();
                var designator = GetDesignatorFromSheetNumber(sheetNumber);

                try
                {
                    if (disciplines.ContainsKey(designator))
                    {
                        var discipline = disciplines[designator];
                        sheet.get_Parameter(settings.NameParameterGUID).Set(discipline.Name);
                        sheet.get_Parameter(settings.OrderParameterGUID).Set(discipline.Index);
                    }
                    else
                    {
                        sheet.get_Parameter(settings.NameParameterGUID).Set("");
                        sheet.get_Parameter(settings.OrderParameterGUID).Set(int.MaxValue);
                    }
                }
                catch (Exception e)
                {
                    var dialog = new TaskDialog("Sheet Discipline Updater");
                    dialog.TitleAutoPrefix = false;
                    dialog.MainInstruction = "I ran into trouble updating the discipline of sheet {sheetNumber}.";
                    dialog.MainContent = e.ToString();
                    dialog.Show();
                }
            }
        }

        public static string GetDesignatorFromSheetNumber(string sheetNumber)
        {
            int i = 0;
            while (i < sheetNumber.Length && char.IsLetter(sheetNumber[i]))
            {
                i++;
            }
            return sheetNumber.Substring(0, i);
        }

        public static void UpdateAllSheets(Configuration config)
        {
            config.EnsureRequiredParametersAreDefined();
            var collector = new FilteredElementCollector(config.Document).OfCategory(BuiltInCategory.OST_Sheets);
            UpdateSheets(config, collector.ToElementIds());
        }
    }
}

