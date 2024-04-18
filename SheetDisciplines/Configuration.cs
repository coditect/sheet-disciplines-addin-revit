using System;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace SheetDisciplines
{
    public class Configuration
    {
        Document _document;
        DataStorage _storage;

        public Configuration(Document document) {
            _document = document;
            _storage = GetOrCreateStorage();
        }

        public Document Document
        {
            get => _document;
        }

        private DataStorage GetOrCreateStorage()
        {
            var filter = new ExtensibleStorageFilter(Settings.SchemaGUID);
            var collector = new FilteredElementCollector(_document).WherePasses(filter);
            return collector.FirstElement() as DataStorage ?? DataStorage.Create(_document);
        }

        public Settings GetSettings()
        {
            var settings = new Settings(_storage);
            if (settings == null || !settings.Entity.IsValid())
            {
                settings = new Settings();
                settings.Disciplines = DisciplineList.GetDefault();
                settings.Save(_storage);
            }
            return settings;
        }

        public void SaveSettings(Settings settings)
        {
            settings.Save(_storage);
        }

        public void EnsureRequiredParametersAreDefined()
        {
            EnsureParameterIsDefined(Settings.NameParameterGUIDField, "Sheet Discipline", SpecTypeId.String.Text, false, true);
            EnsureParameterIsDefined(Settings.OrderParameterGUIDField, "Sheet Discipline Order", SpecTypeId.Int.Integer, false, true);
        }

        public void EnsureParameterIsDefined(string fieldName, string parameterName, ForgeTypeId parameterType, bool userModifiable, bool visible)
        {
            var settings = GetSettings();
            //TaskDialog.Show("Sheet Discipline", $"Entity {entity.SchemaGUID} {entity.IsValid()}");
            var parameterGUID = settings.Entity.Get<Guid>(fieldName);
            var parameterExists = parameterGUID != null && SharedParameterElement.Lookup(_document, parameterGUID) != null;


            if (!parameterExists)
            {
                var originalSharedParameterFilename = _document.Application.SharedParametersFilename;
                _document.Application.SharedParametersFilename = Path.GetTempFileName();
                //TaskDialog.Show("Sheet Discipline", $"Using temporary shared parameters file at {_document.Application.SharedParametersFilename}");

                using (var file = _document.Application.OpenSharedParameterFile())
                {
                    var options = new ExternalDefinitionCreationOptions(parameterName, parameterType);
                    options.UserModifiable = userModifiable;
                    options.Visible = visible;

                    var group = file.Groups.Create("Sheet Information");
                    var definition = group.Definitions.Create(options) as ExternalDefinition;

                    var categories = _document.Application.Create.NewCategorySet();
                    var sheets = _document.Settings.Categories.get_Item(BuiltInCategory.OST_Sheets);
                    categories.Insert(sheets);

                    var binding = _document.Application.Create.NewInstanceBinding(categories);
                    _document.ParameterBindings.Insert(definition, binding, BuiltInParameterGroup.PG_IDENTITY_DATA);

                    settings.Entity.Set(fieldName, definition.GUID);
                    settings.Save(_storage);
                }
                _document.Application.SharedParametersFilename = originalSharedParameterFilename;
            }
        }
    }
}
