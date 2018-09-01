using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DocsGenerator
{
    internal class ClassPropertyComment
    {
        public string ClassName { get; set; }
        public string PropertyName { get; set; }
        private string rawComment;
        public string RawComment
        {
            get
            {
                return rawComment;
            }
            set
            {
                rawComment = value;
                if(string.IsNullOrWhiteSpace(value))
                {
                    XmlComment = new XDocument();
                    DocumentationElements = new List<XElement>(0);
                }
                else
                {
                    XmlComment = XDocument.Parse(value);
                    DocumentationElements = XmlComment.Elements().First().Elements();

                    CleanedDocumentationTags = new Dictionary<string, string>();
                    foreach(var element in DocumentationElements)
                    {
                        string val = element.Value
                                            .TrimStart()
                                            .TrimStart(Environment.NewLine.ToCharArray())
                                            .TrimEnd()
                                            .TrimEnd(Environment.NewLine.ToCharArray());
                        CleanedDocumentationTags.Add(element.Name.LocalName, val);
                    }
                }
            }
        }
        public XDocument XmlComment { get; private set; }
        public IEnumerable<XElement> DocumentationElements { get; private set; }
        public IDictionary<string, string> CleanedDocumentationTags { get; private set; }
        public string Type { get; set; }
    }
}