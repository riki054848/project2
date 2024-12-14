using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


    // מחלקת HtmlHelper לטעינת תגיות מתוך קבצי JSON
    public class HtmlHelper
    {
        private static HtmlHelper _instance;
        private List<string> _allTags;
        private List<string> _selfClosingTags;

        private HtmlHelper()
        {
            _allTags = LoadTags("allTags.json");
            _selfClosingTags = LoadTags("selfClosingTags.json");
        }

        public static HtmlHelper Instance => _instance ??= new HtmlHelper();

        private List<string> LoadTags(string fileName)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(fileName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    return JsonSerializer.Deserialize<List<string>>(json);
                }
            }
        }

        public List<string> GetAllTags() => _allTags;
        public List<string> GetSelfClosingTags() => _selfClosingTags;

    }
