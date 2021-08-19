using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeadReckoning.Abstracts
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

    }

    public class Religion
    {

    }
}
