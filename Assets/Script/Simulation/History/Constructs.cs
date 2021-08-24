using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeadReckoning.Constructs
{
    public class Structure
    {
        public int population;

        public Government government;
        public Culture culture;
        public Religion religion;
    }

    public class Government
    {
        public Type type;

        public enum Type { feudal, tribal, nomadic }
    }

    public class Culture
    {
        public bool surnamingScheme = true;
        public bool originNamingScheme = true;
    }

    public class Religion
    {

    }

    public class Language
    {
        public bool forwardNamingScheme = true;
        public bool headInitial = true;
        public string basicGenitive = "of";
    }
}
