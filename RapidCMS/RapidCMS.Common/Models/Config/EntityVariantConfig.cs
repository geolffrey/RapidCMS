﻿using System;

namespace RapidCMS.Common.Models.Config
{
    internal class EntityVariantConfig
    {
        public EntityVariantConfig(string name, Type type, string? icon = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Icon = icon;
        }

        internal string Name { get; set; }
        internal string? Icon { get; set; }
        internal Type Type { get; set; }
    }
}
