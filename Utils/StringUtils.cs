﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;

namespace SatelliteStorage.Utils
{
    class StringUtils
    {
        public static string GetStackCount(int count)
        {
            string text = count.ToString();
            if (count >= 1000) text = MathF.Round(count/1000)+"k";
            if (count >= 1000000) text = MathF.Round(count / 1000000) + "m";
            if (count >= 1000000000) text = "999m";

            return text;
        }

        public static string GetLangTextValueIfExists(string key, string defaultValue = "")
        {
            return 
                Language.Exists(key) ? Language.GetTextValue(key) : defaultValue;
        }

        public static string CleanAffixName(string name)
        {
            return 
                name.Split("[")[0].Trim();
        }
    }
}
