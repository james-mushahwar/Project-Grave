using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace _Scripts._Game.Localisation{
    
    public class CSVLoader
    {
        private TextAsset _csvFile;
        private char _lineSeparator = '\n';
        private char _surround = '"';
        private string[] _fieldSeparator = { "\", \"" };

        public void LoadCSV()
        {
            _csvFile = Resources.Load<TextAsset>("Localisation/localisation");


        }

        public Dictionary<string, string> GetDictionaryValues(string attributeID)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            string[] lines = _csvFile.text.Split(_lineSeparator);

            int attributeIndex = -1;

            string[] headers = lines[0].Split(_fieldSeparator, System.StringSplitOptions.None);

            for (int i = 0; i < headers.Length; i++)
            {
                if (headers[i].Contains(attributeID))
                {
                    attributeIndex = i;
                    break;
                }
            }

            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))/");

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];

                string[] fields = CSVParser.Split(line);

                for(int j = 0; j < fields.Length; j++)
                {
                    fields[j] = fields[j].TrimStart(' ', _surround);
                    fields[j] = fields[j].TrimEnd(_surround);
                }

                if (fields.Length > attributeIndex)
                {
                    var key = fields[0];

                    if (dict.ContainsKey(key))
                    {
                        continue;
                    }

                    var value = fields[attributeIndex];

                    dict.Add(key, value);
                }
            }

            return dict;
        }
    }
    
}
