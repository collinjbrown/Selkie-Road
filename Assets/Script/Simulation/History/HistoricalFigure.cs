using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.Constructs;

namespace DeadReckoning.Sim
{
    public class HistoricalFigure
    {
        #region Naming
        public string Name { get { return GetName(); } }

        private string firstName;
        private bool forwardNamingScheme;

        private bool surnamingScheme;
        private string surname;

        private bool originNamingScheme;
        private bool originPrepositional;
        private string originName;
        private string nameGenitive;

        public string GetName()
        {
            string retName = firstName;

            if (surnamingScheme && forwardNamingScheme && surname != "")
            {
                retName = $"{retName} {surname}";
            }
            else if (surnamingScheme && !forwardNamingScheme && surname != "")
            {
                retName = $"{surname} {retName}";

            }

            if (originNamingScheme && originPrepositional && originName != "")
            {
                retName = $"{retName} {nameGenitive} {originName}";
            }
            else if (originNamingScheme && !originPrepositional && originName != "")
            {
                retName = $"{originName} {nameGenitive} {retName}";
            }

            return retName;
        }
        #endregion



        #region Constructors
        public HistoricalFigure(string firstName, string surname, string originName, Culture culture, Language language)
        {
            this.firstName = firstName;
            this.surname = surname;
            this.originName = originName;

            this.surnamingScheme = culture.SurnamingScheme;
            this.originNamingScheme = culture.OriginNamingScheme;

            this.forwardNamingScheme = language.ForwardNamingScheme; // I'd like to have more varied forms for languages.
            this.originPrepositional = language.HeadInitial;         // We'll have to figure out how to handle that later.
            this.nameGenitive = language.BasicGenitive;
        }
        #endregion
    }
}
