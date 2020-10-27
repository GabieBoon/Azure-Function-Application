using Microsoft.Spatial;
using System;
using System.Collections.Generic;

namespace SkillsGardenApi.Utils
{
    public class SplitPipeStringUtil
    {
        public static List<T> ParseWords<T>(string s)
        {
            List<T> words = new List<T>();

            int pos = 0;
            while (pos < s.Length)
            {
                int start = pos;

                pos = s.IndexOf('|', pos);
                while (pos > 0 && s[pos - 1] == '\\')
                {
                    pos++;
                    pos = s.IndexOf('|', pos);
                }

                if (pos < 0)
                    pos = s.Length;

                if (!Enum.IsDefined(typeof(T), s.Substring(start, pos - start)))
                {
                    throw new ParseErrorException(s.Substring(start, pos - start));
                }

                words.Add((T)Enum.Parse(typeof(T), s.Substring(start, pos - start)));

                if (pos < s.Length)
                    pos++;
            }
            return words;
        }
    }
}
