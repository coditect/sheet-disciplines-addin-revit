using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SheetDisciplines
{
    abstract public class EntityWrapper
    {
        protected Entity _entity;

        public EntityWrapper()
        {
            var schema = GetSchema();
            _entity = new Entity(schema);
        }

        public EntityWrapper(Element element)
        {
            var schema = GetSchema();
            _entity = element.GetEntity(schema);
        }

        public EntityWrapper(Entity entity)
        {
            Entity = entity;
        }

        public Entity Entity
        {
            get => _entity;
            set
            {
                var expectedGuid = GetSchemaGUID();
                if (value.SchemaGUID == expectedGuid)
                {
                    _entity = value;
                }
                else
                {
                    throw new ArgumentException($"Entity does not conform to schema {expectedGuid}");
                }
            }
        }

        public Schema GetSchema()
        {
            return Schema.Lookup(GetSchemaGUID()) ?? BuildSchema();
        }

        abstract public Guid GetSchemaGUID();
        abstract public Schema BuildSchema();

        public void Save(Element element)
        {
            element.SetEntity(_entity);
        }
    }

    public class Settings : EntityWrapper
    {
        public static readonly Guid SchemaGUID = new Guid("D78139EB-D508-4FB0-8A25-85D809ABD57E");
        public const string NameParameterGUIDField = "nameParameterGUID";
        public const string OrderParameterGUIDField = "orderParameterGUID";
        public const string DisciplinesField = "disciplines";

        public Settings() : base() { }
        public Settings(Element e) : base(e) { }
        public Settings(Entity e) : base(e) { }

        public Guid NameParameterGUID
        {
            get => _entity.Get<Guid>(NameParameterGUIDField);
            set => _entity.Set(NameParameterGUIDField, value);
        }

        public Guid OrderParameterGUID
        {
            get => _entity.Get<Guid>(OrderParameterGUIDField);
            set => _entity.Set(OrderParameterGUIDField, value);
        }

        public DisciplineList Disciplines
        {
            get => new DisciplineList(_entity.Get<IList<Entity>>(DisciplinesField));
            set => _entity.Set(DisciplinesField, value.GetEntityList());
        }

        public override Guid GetSchemaGUID()
        {
            return SchemaGUID;
        }

        public override Schema BuildSchema()
        {
            var schemaBuilder = new SchemaBuilder(GetSchemaGUID());
            schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
            schemaBuilder.SetWriteAccessLevel(AccessLevel.Vendor);
            schemaBuilder.SetVendorId("com.nicholasorenrawlings");
            schemaBuilder.SetSchemaName("SheetDisciplinesSettings");
            schemaBuilder.SetDocumentation("Contains settings for the Sheet Discipline add-in");

            var field1Builder = schemaBuilder.AddSimpleField(NameParameterGUIDField, typeof(Guid));
            field1Builder.SetDocumentation("Stores the GUID of the Sheet Discipline parameter");

            var field2Builder = schemaBuilder.AddSimpleField(OrderParameterGUIDField, typeof(Guid));
            field2Builder.SetDocumentation("Stores the GUID of the Sheet Discipline Order parameter");

            var field3Builder = schemaBuilder.AddArrayField(DisciplinesField, typeof(Entity));
            field3Builder.SetDocumentation("Stores the list of disciplines");
            field3Builder.SetSubSchemaGUID(Discipline.SchemaGUID);

            return schemaBuilder.Finish();
        }
    }

    public class Discipline : EntityWrapper, INotifyPropertyChanged
    {
        public static readonly Guid SchemaGUID = new Guid("C18B8D03-3007-4DD0-B140-2A2CA81DD210");
        public const string DesignatorField = "designator";
        public const string NameField = "name";

        public event PropertyChangedEventHandler PropertyChanged;
        private int _index;

        public Discipline() : base() { }
        public Discipline(Entity e, int index) : base(e) {
            _index = index;
        }
        public Discipline(string designator, string name, int index) : base()
        {
            Designator = designator;
            Name = name;
            _index = index;
        }
        
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string Designator
        {
            get => _entity.Get<string>(DesignatorField);
            set
            {
                if (_entity.Get<string>(DesignatorField) != value)
                {
                    _entity.Set(DesignatorField, value);
                    NotifyPropertyChanged();
                }
            }
        }

    public string Name
        {
            get => _entity.Get<string>(NameField);
            set
            {
                if (_entity.Get<string>(NameField) != value)
                {
                    _entity.Set(NameField, value);
                    NotifyPropertyChanged();
                }
            }
        }

        public int Index
        {
            get => _index;
            set
            {
                if (_index != value)
                {
                    _index = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public override Guid GetSchemaGUID()
        {
            return SchemaGUID;
        }

        public override Schema BuildSchema()
        {
            var schemaBuilder = new SchemaBuilder(GetSchemaGUID());
            schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
            schemaBuilder.SetWriteAccessLevel(AccessLevel.Vendor);
            schemaBuilder.SetVendorId("com.nicholasorenrawlings");
            schemaBuilder.SetSchemaName("SheetDiscipline");
            schemaBuilder.SetDocumentation("Contains information about a discipline that a sheet can be assigned to");

            var field1Builder = schemaBuilder.AddSimpleField(DesignatorField, typeof(string));
            field1Builder.SetDocumentation("Stores the sheet number prefix for the discipline");

            var field2Builder = schemaBuilder.AddSimpleField(NameField, typeof(string));
            field2Builder.SetDocumentation("Stores the name of the discipline");

            return schemaBuilder.Finish();
        }
    }

    public class DisciplineList
    {
        List<Discipline> _list = new List<Discipline>();
        Dictionary<string, Discipline> _map = new Dictionary<string, Discipline>();

        public DisciplineList() {}

        public DisciplineList(IList<Entity> entities)
        {
            var i = 0;
            foreach (var entity in entities)
            {
                var discipline = new Discipline(entity, i);
                _map.Add(discipline.Designator, discipline);
                _list.Add(discipline);
                i++;
            }
        }

        public void Add(string designator, string name)
        {
            var discipline = new Discipline(designator, name, _list.Count);
            _map.Add(designator, discipline);
            _list.Add(discipline);
        }

        public Discipline this[int index]
        {
            get => _list[index];
        }

        public Discipline this[string index]
        {
            get => _map[index];
        }

        public bool ContainsKey(string key)
        {
            return _map.ContainsKey(key);
        }

        public List<Discipline> GetList() { return _list; }

        public IList<Entity> GetEntityList()
        {
            var list = new List<Entity>() as IList<Entity>;
            foreach (var item in _list)
            {
                list.Add(item.Entity);
            }
            return list;
        }

        public static DisciplineList GetDefault()
        {
            var defaultDisciplines = new DisciplineList();
            defaultDisciplines.Add("G", "General");
            defaultDisciplines.Add("H", "Hazardous Materials");
            defaultDisciplines.Add("V", "Survey/Mapping");
            defaultDisciplines.Add("B", "Geotechnical");
            defaultDisciplines.Add("C", "Civil");
            defaultDisciplines.Add("L", "Landscape");
            defaultDisciplines.Add("S", "Structural");
            defaultDisciplines.Add("AD", "Architectural Demolition");
            defaultDisciplines.Add("A", "Architectural");
            defaultDisciplines.Add("AF", "Architectural Furniture");
            defaultDisciplines.Add("I", "Interiors");
            defaultDisciplines.Add("Q", "Equipment");
            defaultDisciplines.Add("FP", "Fire Protection");
            defaultDisciplines.Add("PD", "Plumbing Demolition");
            defaultDisciplines.Add("P", "Plumbing");
            defaultDisciplines.Add("D", "Process");
            defaultDisciplines.Add("MD", "Mechanical Demolition");
            defaultDisciplines.Add("M", "Mechanical");
            defaultDisciplines.Add("ED", "Electrical Demolition");
            defaultDisciplines.Add("E", "Electrical");
            defaultDisciplines.Add("W", "Distributed Energy");
            defaultDisciplines.Add("T", "Telecommunications");
            defaultDisciplines.Add("R", "Resource");
            defaultDisciplines.Add("X", "Other Disciplines");
            defaultDisciplines.Add("Z", "Contractor/Shop Drawings");
            defaultDisciplines.Add("O", "Operations");
            return defaultDisciplines;
        }
    }
}
