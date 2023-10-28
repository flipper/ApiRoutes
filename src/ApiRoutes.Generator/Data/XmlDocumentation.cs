using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ApiRoutes.Generator;

public class XmlDocumentation
{
    public string Summary { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;

    private static readonly Regex _xmlRegex = new(@"^\s*(\/\/\/)", RegexOptions.Multiline, TimeSpan.FromSeconds(5));

    public static XmlDocumentation Parse(SyntaxNode node)
    {
        var documentation = new XmlDocumentation();

        var stringBuilder = new StringBuilder("<xml>");

        foreach (var trivia in node.GetLeadingTrivia())
        {
            if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            {
                stringBuilder.Append(_xmlRegex.Replace(trivia.GetStructure()!.ToString(), string.Empty));
            }else if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                stringBuilder.AppendLine(_xmlRegex.Replace(trivia.ToString().TrimStart(), string.Empty));
            }
        }

        stringBuilder.Append("</xml>");

        var xml = stringBuilder.ToString();

        if (!string.IsNullOrEmpty(xml))
        {
            var document = new XmlDocument();
            document.LoadXml($"<xml>{xml}</xml>");


            XmlNodeList summary = document.GetElementsByTagName("summary");

            if (summary.Count > 0)
            {
                documentation.Summary = summary[0].InnerText.Trim();
            }

            XmlNodeList remarks = document.GetElementsByTagName("remarks");

            if (remarks.Count > 0)
            {
                documentation.Remarks = remarks[0].InnerText.Trim();
            }
        }

        return documentation;
    }
}