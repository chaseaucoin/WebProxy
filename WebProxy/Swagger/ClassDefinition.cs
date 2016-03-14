using System.Collections.Generic;

namespace WebProxy.Swagger
{
    public class ClassDefinition
    {
        public ClassDefinition(string name)
        {
            this.Name = name;
            this.Properties = new List<TypeDefinition>();
        }

        public string Name { get; set; }
        public List<TypeDefinition> Properties { get; set; }
        public string Inherits { get; set; }
    }
}