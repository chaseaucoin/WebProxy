﻿using System;

namespace WebProxy.Swagger
{
    public class TypeDefinition
    {
        public TypeDefinition(string typeName, string name, string[] enumValues = null, bool isNullableType = false)
        {
            this.TypeName = typeName;
            this.Name = name;
            this.EnumValues = enumValues;
            this.IsNullableType = isNullableType;
        }

        public string Name { get; set; }
        public string TypeName { get; set; }
        public string[] EnumValues { get; set; }
        public bool IsNullableType { get; set; }

        public string GetCleanTypeName()
        {
            while (Name.Contains("-"))
            {
                int index = Name.IndexOf("-", StringComparison.InvariantCulture);
                var letter = Name[index + 1].ToString().ToUpper();
                Name = Name.Remove(index, 2);
                Name = Name.Insert(index, letter);

            }
            return Name.Replace("$", "");
        }
    }
}