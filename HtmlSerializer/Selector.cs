using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public class Selector
    {
        public string TagName { get; set; }
        public string Id { get; set; }
        public List<string> Classes { get; set; }
        public Selector Parent { get; set; }

        public Selector()
        {
            Classes = new List<string>();
        }

    public static Selector Parse(string query)
    {
        var parts = query.Split(' ');
        Selector root = new Selector();
        Selector current = root;

        foreach (var part in parts)
        {
            var selector = new Selector();
            if (part.StartsWith("#"))
            {
                selector.Id = part.Substring(1);  // אם יש # בסלקטור, זה ID
            }
            else if (part.StartsWith("."))
            {
                selector.Classes.Add(part.Substring(1));  // אם יש . בסלקטור, זה Class
            }
            else
            {
                selector.TagName = part;  // אחרת זה תגית
            }

            current.Parent = selector;
            current = selector;
        }

        return root;
    }
}

