using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PapMediaPlayer.XmlParser.Exceptions;
namespace PapMediaPlayer.XmlParser
{
    /// <summary>
    /// Parse/Read/Write configuration files
    /// This Class implements IXmlParsable and IDisposable.
    /// </summary>
    /// <exception cref="FailCallException">Throws a FailCallException when you call unspecified methods of this class</exception>
    /// <exception cref="WrongParametersException">Throws a WrongParametersException when the parameters are invalid</exception>
    public sealed class XmlConfigParser : IXmlParsable
    {
        public struct Configs
        {
            public int SongInPlaylist { get; set; }
            public string CurrentPlaylist { get; set; }
        }
        private Configs Default = new Configs { SongInPlaylist = 0, CurrentPlaylist = "None" };
        private XDocument doc;
        private string path;

        /// <summary>
        /// Creates an instance of XmlConfigParser by creating a new configs.xml file with specified configs
        /// </summary>
        /// <param name="path">The specified path without the "configs.xml" extention</param>
        /// <param name="configs">The configs to add. Null to add the defaults(0, None)</param>
        public XmlConfigParser(string path, Configs? configs)
        {
            this.path = path.Contains("configs.xml")? System.IO.Path.Combine(path, "configs.xml"): path;
            CreateXml(configs);
        }

        /// <summary>
        /// Creates an instance of XmlConfigParser by loading an existing configs.xml file.
        /// If the configs.xml file does not exist then a new one will be created with default values(0, None)
        /// </summary>
        /// <param name="path"></param>
        public XmlConfigParser(string path)
        {
            this.path = this.path = path.Contains("configs.xml") ? System.IO.Path.Combine(path, "configs.xml") : path;
            try
            {
                doc = XDocument.Load(path);
            }
            catch
            {
                CreateXml(null);
            }
        }

        private void CreateXml(Configs? config)
        {
            if (config == null)
                config = Default;
            doc = new XDocument(new XElement("Config"));
            AddItem(config);
            doc.Save(path);
        }

        public object FetchItems()
        {
            Configs configs = new Configs();
            configs.SongInPlaylist = Convert.ToInt32(doc.Root.Element("SongInPlaylist").Value);
            configs.CurrentPlaylist = doc.Root.Element("CurrentPlaylist").Value;
            return configs;
        }

        public void AddItem(object OtobeAdded)
        {
            try
            {
                Configs c = (Configs)OtobeAdded;
                if (doc.Root.HasElements)
                    doc.Root.RemoveAll();
                XElement element = new XElement("SongInPlaylist", c.SongInPlaylist);
                XElement element2 = new XElement("CurrentPlaylist", c.CurrentPlaylist);
                doc.Root.Add(new object[] { element, element2 });
                doc.Save(path);
            }catch(InvalidCastException iex)
            {
                throw new WrongParametersException("Object OtobeAdded is not of type Configs.", iex.StackTrace);
            }
        }
        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="OtobeAdded"></param>
        public void AddItems(object OtobeAdded)
        {
            throw new FailCallException("This method has no body", this.GetType());
        }

        public void RemoveItem(object OtobeRemoved)
        {
            throw new FailCallException("This method has no body", this.GetType());
        }

        public void RemoveItems(object OtobeRemoved)
        {
            throw new FailCallException("This method has no body", this.GetType());
        }

        public void Dispose()
        {
            doc = null;
            path = null;
        }
    }
}
