﻿using System;
using RapidCMS.Common.Data;
using RapidCMS.Common.Enums;
using RapidCMS.Common.ValueMappers;

namespace RapidCMS.Common.Models.UI
{
    public class FieldUI : Element
    {
        public string CustomAlias { get; set; }

        public EditorType Type { get; set; }

        public IValueMapper ValueMapper { get; set; }
        public IPropertyMetadata Property { get; set; }
        public IDataProvider DataProvider { get; set; }

        public object GetValue(IEntity entity)
        {

            return ValueMapper.MapToEditor(null, Property.Getter(entity));
        }

        public void SetValue(IEntity entity, object value)
        {

            Property.Setter(entity, ValueMapper.MapFromEditor(null, value));
        }

        [Obsolete("This should depend on an IPropertyMetadata but on an IExpressionMetadata")]
        public string GetReadonlyValue(IEntity entity)
        {
            return ValueMapper.MapToView(null, Property.Getter(entity));
        }
    }
}
