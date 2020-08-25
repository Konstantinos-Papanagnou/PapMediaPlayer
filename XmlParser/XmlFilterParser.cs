using PapMediaPlayer.XmlParser.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PapMediaPlayer.XmlParser
{
    /// <summary>
    /// Parse/Read/Write filter files
    /// This Class implements IXmlParsable and IDisposable.
    /// </summary>
    /// <exception cref="WrongParametersException">Throws WrongParametersException when the parameters are not of the right type</exception>
    public sealed class XmlFilterParser: IXmlParsable
    {
        private XDocument doc;
        private string path;

        public struct RVal
        {
            public string[] filters { get; set; }
            public char[] specialChars { get; set; }
        }

        /// <summary>
        /// Creates an instance of XmlFilterParser by opening an existing filters.xml file. If it fails it creates a new one with basic filters and special chars
        /// </summary>
        /// <param name="path">The specified path without the *.xml extention</param>
        public XmlFilterParser(string path)
        {
            this.path = path + "/filters.xml";
            try
            {
                doc = XDocument.Load(path);
            }
            catch
            {
                CreateXml(new string[] { "$####-####", "%extSdCard" }, new char[] { '#', '*' } );
            }
        }

        /// <summary>
        /// Creates an instance of XmlFilterParser by creating a new filters.xml file with specified filters and special chars only
        /// </summary>
        /// <param name="path">The specified path without the *.xml extention</param>
        /// <param name="filter">The filter to add</param>
        /// <param name="specialChar">The special char to add</param>
        public XmlFilterParser(string path, string filter, char specialChar)
        {
            this.path = path + "/filters.xml";
            CreateXml(new string[] { filter }, new char[] { specialChar });
        }

        /// <summary>
        /// Creates an instance of XmlFilterParser by creating a new filters.xml file with specified filters and special chars only
        /// </summary>
        /// <param name="path">The specified path without the *.xml extention</param>
        /// <param name="filters">The filters to add</param>
        /// <param name="specialChars">The special chars to add</param>
        public XmlFilterParser(string path, string[] filters, char[] specialChars)
        {
            this.path = path + "/filters.xml";
            CreateXml(filters, specialChars);
        }

        private bool Exists(char c)
        {
            foreach(XElement element in doc.Root.Element("specialChars").Elements())
            {
                if (element.Value == c.ToString())
                    return true;
            }
            return false;
        }

        private bool Exists(string s)
        {
            foreach (XElement element in doc.Root.Element("filters").Elements())
            {
                if (element.Value == s)
                    return true;
            }
            return false;
        }

        public void AddItem(object OtobeAdded)
        {
            try
            {
                char tobeAdded = (char)OtobeAdded;
                if (Exists(tobeAdded))
                    return;
                XElement element = new XElement("specialChar", tobeAdded);
                doc.Root.Element("specialChars").Add(element);
            }
            catch (InvalidCastException)
            {
                try
                {
                    string tobeAdded = (string)OtobeAdded;
                    if (Exists(tobeAdded))
                        return;
                    XElement element = new XElement("filter", tobeAdded);
                    doc.Root.Element("filters").Add(element);
                }
                catch(InvalidCastException)
                {
                    throw new WrongParametersException("Invalid parameters not of type string or char", "XmlFilterParser.AddItem");
                }
            }
            finally { doc.Save(path); }
        }

        public void AddItems(object OtobeAdded)
        {
            try
            {
                char[] tobeAdded = (char[])OtobeAdded;
                foreach (char specialChar in tobeAdded)
                    AddItem(specialChar);
            }
            catch (InvalidCastException)
            {
                try
                {
                    string[] tobeAdded = (string[])OtobeAdded;
                    foreach (string filter in tobeAdded)
                        AddItem(filter);
                }
                catch (InvalidCastException)
                {
                    throw new WrongParametersException("Invalid parameters not of type string[] or char[]", "XmlFilterParser.AddItems");
                }
            }
        }

        public void Dispose()
        {
            doc = null;
            path = null;
        }

        public object FetchItems()
        {
            List<string> filters = new List<string>();
            List<char> specialChars = new List<char>();
            foreach(XElement element in doc.Root.Element("filters").Elements())
                filters.Add(element.Value);
            foreach (XElement element in doc.Root.Element("specialChars").Elements())
                specialChars.Add(element.Value[0]);
            return new RVal() { filters = filters.ToArray(), specialChars = specialChars.ToArray() };
        }

        public void RemoveItem(object OtobeRemoved)
        {
            try
            {
                char tobeRemoved = (char)OtobeRemoved;
                if (!Exists(tobeRemoved))
                    return;
                doc.Root.Element("specialChars").Elements().Where(x => x.Value == tobeRemoved.ToString()).Remove();
            }
            catch(InvalidCastException)
            {
                try
                {
                    string tobeRemoved = (string)OtobeRemoved;
                    if (!Exists(tobeRemoved))
                        return;
                    doc.Root.Element("filters").Elements().Where(x => x.Value == tobeRemoved).Remove();
                } catch (InvalidCastException)
                {
                    throw new WrongParametersException("Parameters are invalid not of type string or char", "XmlFilterParser.RemoveItem");
                }
            }
            finally { doc.Save(path); }
        }

        public void RemoveItems(object OtobeRemoved)
        {
            try
            {
                char[] tobeRemoved = (char[])OtobeRemoved;
                foreach (char c in tobeRemoved)
                    RemoveItem(c);
            }
            catch(InvalidCastException)
            {
                try
                {
                    string[] tobeRemoved = (string[])OtobeRemoved;
                    foreach (string s in tobeRemoved)
                        RemoveItem(s);
                }
                catch (InvalidCastException)
                {
                    throw new WrongParametersException("Parameters are invalid not of type string[] or char[]", "XmlFilterParser.RemoveItems");
                }
            }
        }

        private void CreateXml(string[] filters, char[] specialChars)
        {
            string dirpath = path.Remove(path.Count() - 12);
            System.IO.Directory.CreateDirectory(dirpath);
            doc = new XDocument(new XElement("Config"));
            XElement element = new XElement("filters");
            XElement element2 = new XElement("specialChars");
            foreach (string filter in filters)
                element.Add(new XElement("filter", filter));
            foreach (char specialChar in specialChars)
                element2.Add(new XElement("specialChar", specialChar));
            doc.Root.Add(element);
            doc.Root.Add(element2);
            doc.Save(path);
        }
    }
}
