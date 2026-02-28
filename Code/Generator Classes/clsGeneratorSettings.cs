using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code_Generator.Generator_Classes
{
    public class clsGeneratorSettings
    {
        public enum enFKSearchMode { All, JustThis }
        public enum enStaticMethodsMode { Yes, No }

        public enFKSearchMode FKSearchMode { get; set; } = enFKSearchMode.All;
        public enStaticMethodsMode StaticMethodsMode { get; set; } = enStaticMethodsMode.Yes;
    }
}
