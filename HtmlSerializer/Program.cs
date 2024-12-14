using System.Text.RegularExpressions;


public class HtmlSerializer
{
    private static readonly HttpClient client = new HttpClient();

    // טוען HTML מאתר ומנקה תווי new line ורווחים מיותרים
    public async Task<string> Load(string url)
    {
        var response = await client.GetAsync(url);
        var html = await response.Content.ReadAsStringAsync();
        html = html.Replace("\n", "").Replace("\r", "").Trim(); // הסרת תווי new line ורווחים מיותרים
        return html;
    }

    // פענוח HTML לפירוק תגיות לרשימה של מחרוזות
    public List<string> ParseHtml(string html)
    {
        var tagPattern = @"<[^>]+>";
        var matches = Regex.Matches(html, tagPattern);
        var tags = new List<string>();

        foreach (Match match in matches)
        {
            tags.Add(match.Value);
        }
        return tags;
    }

    // פענוח תגית HTML לחפירת ID, Class, תוכן פנימי ועוד
    public HtmlElement ParseTag(string tag)
    {
        HtmlElement element = new HtmlElement();

        // חיפוש שם התגית
        var tagNameMatch = Regex.Match(tag, @"<([a-zA-Z0-9]+)");
        if (tagNameMatch.Success)
        {
            element.Name = tagNameMatch.Groups[1].Value;
        }

        // חיפוש ה-ID
        var idMatch = Regex.Match(tag, @"id=""([^""]+)""");
        if (idMatch.Success)
        {
            element.Id = idMatch.Groups[1].Value;
        }

        // חיפוש ה-Class
        var classMatch = Regex.Match(tag, @"class=""([^""]+)""");
        if (classMatch.Success)
        {
            element.Classes.AddRange(classMatch.Groups[1].Value.Split(' '));
        }

        // חיפוש תוכן פנימי (InnerHtml)
        var innerHtmlMatch = Regex.Match(tag, @">(.+)<\/[^>]+");
        if (innerHtmlMatch.Success)
        {
            element.InnerHtml = innerHtmlMatch.Groups[1].Value;
        }

        return element;
    }

    // בניית עץ HTML מהתגיות
    public HtmlElement BuildTree(List<string> tags)
    {
        HtmlElement root = new HtmlElement();
        HtmlElement currentElement = root;

        foreach (var tag in tags)
        {
            // טיפול בתגיות סגירה
            if (tag.StartsWith("</"))
            {
                currentElement = currentElement.Parent;
                continue;
            }

            HtmlElement element = ParseTag(tag);
            if (element != null)
            {
                currentElement.Children.Add(element);
                element.Parent = currentElement;
                currentElement = element;
            }
        }

        return root;
    }

    // פונקציה להדפסת כל האלמנטים עם ה-ID שלהם
    public void PrintHtmlElements(HtmlElement root)
    {
        foreach (var element in Descendants(root))
        {
            // הדפסת ה-ID רק אם הוא קיים
            string idOutput = !string.IsNullOrEmpty(element.Id) ? $"ID: {element.Id}" : null;

            // הדפסת ה-Classים רק אם ישנם
            string classOutput = element.Classes.Any() ? $"Classes: {string.Join(", ", element.Classes)}" : null;

            // הדפסת התוצאה
            string output = $"Tag: {element.Name}";

            // אם יש ID להוסיף
            if (idOutput != null)
            {
                output += $", {idOutput}";
            }

            // אם יש Classes להוסיף
            if (classOutput != null)
            {
                output += $", {classOutput}";
            }

            Console.WriteLine(output);
        }

    }


    // פונקציה לחיפוש כל הצאצאים של תגית
    public IEnumerable<HtmlElement> Descendants(HtmlElement root)
    {
        Queue<HtmlElement> queue = new Queue<HtmlElement>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            yield return current;

            foreach (var child in current.Children)
            {
                queue.Enqueue(child);
            }
        }
    }

    // פונקציה למציאת אלמנטים לפי סלקטור
    public List<HtmlElement> FindElements(HtmlElement root, Selector selector)
    {
        HashSet<HtmlElement> results = new HashSet<HtmlElement>();
        FindElementsRecursive(root, selector, results);
        return results.ToList();
    }

    // פונקציה ריקורסיבית למציאת אלמנטים
    private void FindElementsRecursive(HtmlElement element, Selector selector, HashSet<HtmlElement> results)
    {
        if (Match(element, selector))
        {
            results.Add(element);
        }

        foreach (var child in element.Children)
        {
            FindElementsRecursive(child, selector, results);
        }
    }

    // פונקציה לבדוק אם האלמנט תואם לסלקטור
    private bool Match(HtmlElement element, Selector selector)
    {
        if (selector.TagName != null && element.Name != selector.TagName)
            return false;

        if (selector.Id != null && element.Id != selector.Id)
            return false;

        if (selector.Classes.Any() && !selector.Classes.All(c => element.Classes.Contains(c)))
            return false;

        return true;
    }

    // דוגמה לשימוש בקוד
    public static async Task Main(string[] args)
    {
        HtmlSerializer serializer = new HtmlSerializer();

        // דוגמה לקריאת HTML מאתר
        string url = "https://mail.google.com/mail/u/0/#inbox";
        string html = await serializer.Load(url);

        // פענוח התגיות מתוך ה-HTML
        List<string> tags = serializer.ParseHtml(html);

        // בניית עץ HTML
        HtmlElement root = serializer.BuildTree(tags);

        // הדפסת כל האלמנטים עם ה-ID
        serializer.PrintHtmlElements(root);

    }
}

