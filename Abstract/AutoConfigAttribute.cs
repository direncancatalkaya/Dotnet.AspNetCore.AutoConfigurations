using System;

namespace AutoConfigurations.Abstract
{
    public class AutoConfigAttribute : Attribute
    {
        public string SectionName { get; set; }
        public bool Type { get; set; }

        public AutoConfigAttribute(string sectionName)
        {
            SectionName = sectionName;
        }
    }
}