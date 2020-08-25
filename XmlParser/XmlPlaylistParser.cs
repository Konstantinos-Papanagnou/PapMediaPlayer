using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PapMediaPlayer.Models;
using PapMediaPlayer.XmlParser.Exceptions;

namespace PapMediaPlayer.XmlParser
{
    /// <summary>
    /// Use this class to easily Parse/Read/Write Xml files as playlists.
    /// This Class implements IXmlParsable and IDisposable.
    /// </summary>
    /// <exception cref="WrongParametersException">Throws WrongParametersException when the parameters are not valid</exception>
    public sealed class XmlPlaylistParser : IXmlParsable
    {
        private XDocument doc;
        private string path;
        private string filename;

        /// <summary>
        /// Creates an instance of a XmlPlayListParser by loading an existing xml file
        /// </summary>
        /// <param name="path">The specified path to the playlist name without the *.xml extention</param>
        /// <param name="playlistName">The name of the playlist and xml file [playlistName].xml</param>
        public XmlPlaylistParser(string path, string playlistName)
        {
            filename = playlistName;
            this.path = System.IO.Path.Combine(path, playlistName);
            try
            {
                doc = XDocument.Load(this.path);
                filename = doc.Root.Name.ToString();
            }catch
            {
                filename = playlistName;
                CreateXmlFile(null);
            }
        }

        /// <summary>
        /// Creates an instance of a XmlPlayListParser by creating a new xml file
        /// </summary>
        /// <param name="path">The specified path to the playlist name without the *.xml extention</param>
        /// <param name="filename">The name of the playlist and xml file [playlistName].xml</param>
        /// <param name="tobeAdded">The list of songs to add, null if you dont want to add any</param>
        public XmlPlaylistParser(string path, string playlistName, List<Paths> tobeAdded)
        {
            filename = playlistName;
            this.path = path + "/" + filename;
            CreateXmlFile(tobeAdded);
        }

        private void CreateXmlFile(List<Paths> tobeAdded)
        {
            doc = new XDocument(
                    new XElement("Root"));
            
            if (tobeAdded != null)
            {
                AddItems(tobeAdded);
            }
            doc.Save(path);
        }

        public void Dispose()
        {
            doc = null;
            path = null;
            filename = null;
        }

        private bool Exists(Paths track)
        {
            foreach (XElement element in doc.Root.Descendants("Song"))
            {
                if (track.Path == element.Value)
                    return true;
            }
            return false;
        }

        public object FetchItems()
        {
            List<Paths> tracks = new List<Paths>();
            foreach (XElement element in doc.Root.Elements("Song"))
            {
                tracks.Add(new Paths { Title = (string)element.Attribute("Title"), Path = element.Value });
            }
            return tracks.Count == 0? null: tracks;
        }

        public void AddItem(object OtobeAdded)
        {
            try
            {
                Paths tobeAdded = (Paths)OtobeAdded;
                if (Exists(tobeAdded))
                    return;
                XElement item = new XElement("Song", new object[] { new XAttribute("Title", tobeAdded.Title), tobeAdded.Path });
                doc.Root.Add(item);
                doc.Save(path);
            }
            catch (InvalidCastException iex)
            {
                throw new WrongParametersException("Object tobeAdded is not Paths struct.", iex.StackTrace);
            }  
        }

        public void AddItems(object OtobeAdded)
        {
            try
            {
                foreach (Paths track in (List<Paths>)OtobeAdded)
                {
                    if (Exists(track))
                        continue;
                    XElement item = new XElement("Song", new object[] { new XAttribute("Title", track.Title), track.Path });
                    doc.Root.Add(item);
                }
                doc.Save(path);
            }
            catch (InvalidCastException iex)
            {
                throw new WrongParametersException("Object tobeAdded is not paths struct.", iex.StackTrace);
            }
        }

        public void RemoveItem(object OtobeRemoved)
        {
            try
            {
                Paths tobeRemoved = (Paths)OtobeRemoved;
                if (!Exists(tobeRemoved))
                    return;
                doc.Root.Elements("Song").Where(x => x.Value == tobeRemoved.Path).Remove();
                doc.Save(path);
            }
            catch (InvalidCastException iex)
            {
                throw new WrongParametersException("Object tobeAdded is not Paths struct.", iex.StackTrace);
            }
        }

        public void RemoveItems(object OtobeRemoved)
        {
            try
            {
                foreach (Paths track in (List<Paths>)OtobeRemoved)
                {
                    if (!Exists(track))
                        continue;
                    doc.Root.Elements("Song").Where(x => x.Value == track.Path).Remove();
                }
                doc.Save(path);
            }
            catch (InvalidCastException iex)
            {
                throw new WrongParametersException("Object tobeAdded is not Paths struct.", iex.StackTrace);
            }
       }
    }
}