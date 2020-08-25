using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Xml.Linq;
using PapMediaPlayer.XmlParser.Exceptions;

namespace PapMediaPlayer.XmlParser
{
    public sealed class XmlPlaylistsHolderParser : IXmlParsable
    {
        public struct PlaylistHolder
        {
            public string PlaylistName { get; set; }
            public string PlaylistPath { get; set; }
        }

        private XDocument doc;
        private string _filepath;

        /// <summary>
        /// Initializes a new instance of XmlPlaylistsHolderParser by opening an existing xml file
        /// </summary>
        /// <param name="filepath">The location for the existing xml file</param>
        public XmlPlaylistsHolderParser(string filepath, string path)
        {
            this._filepath = filepath;
            try
            {
                doc = XDocument.Load(_filepath);
            }
            catch
            {
                CreateXml(null, path);
            }
        }

        /// <summary>
        /// Initializes a new instance of XmlPlaylistsHolderParser by creating a new xml file
        /// </summary>
        /// <param name="filepath">The path to create the xml file</param>
        /// <param name="holder">Data to add</param>
        public XmlPlaylistsHolderParser(string filepath, string path, PlaylistHolder holder)
        {
            this._filepath = filepath;
            CreateXml(holder, path);
        }

        private void CreateXml(PlaylistHolder? datastream, string path)
        {
            System.IO.Directory.CreateDirectory(path);
            doc = new XDocument(new XElement("Root"));
            if(!datastream.HasValue)
            {
                doc.Save(_filepath);
                return;
            }

            XElement element = new XElement("Playlist", new object[] { new XAttribute("Name", datastream.Value.PlaylistName), new XAttribute("Path", datastream.Value.PlaylistPath) });
            doc.Root.Add(element);
            doc.Save(_filepath);
        }

        /// <summary>
        /// Adds a playlist
        /// </summary>
        /// <param name="OtobeAdded">PlaylistHolder to add</param>
        public void AddItem(object OtobeAdded)
        {
            try
            {
                PlaylistHolder datastream = (PlaylistHolder)OtobeAdded;
                if (Exists(datastream))
                    return;
                XElement element = new XElement("Playlist", new object[] { new XAttribute("Name", datastream.PlaylistName), new XAttribute("Path", datastream.PlaylistPath) });
                doc.Root.Add(element);
                doc.Save(_filepath);
            }
            catch (InvalidCastException ex)
            {
                throw new WrongParametersException("Object is not of type PlaylistHolder", ex.StackTrace);
            }
        }

        /// <summary>
        /// Add a list of PlaylistHolder
        /// </summary>
        /// <param name="OtobeAdded">List of PlaylistHolder to add</param>
        public void AddItems(object OtobeAdded)
        {
            try
            {
                List<PlaylistHolder> datastream = (List<PlaylistHolder>)OtobeAdded;
                foreach (PlaylistHolder data in datastream)
                    AddItem(data);
            }
            catch (InvalidCastException ex)
            {
                throw new WrongParametersException("Object is not of type List<PlaylistHolder>", ex.StackTrace);
            }
        }

        /// <summary>
        /// Releases all associated resources
        /// </summary>
        public void Dispose()
        {
            doc = null;
            _filepath = null;
        }

        /// <summary>
        /// Fetches items from the xml file
        /// </summary>
        /// <returns>List of PlaylistHolder</returns>
        public object FetchItems()
        {
            List<PlaylistHolder> holder = new List<PlaylistHolder>();
            foreach(XElement element in doc.Root.Elements())
                holder.Add(new PlaylistHolder { PlaylistName = (string)element.Attribute("Name"), PlaylistPath = (string)element.Attribute("Path") });

            return holder;
        }

        /// <summary>
        /// Removes a playlist
        /// </summary>
        /// <param name="OtobeRemoved">PlaylistHolder to remove</param>
        public void RemoveItem(object OtobeRemoved)
        {
            try
            {
                PlaylistHolder datastream = (PlaylistHolder)OtobeRemoved;
                if (!Exists(datastream))
                    return;
                doc.Root.Elements("Playlist").Where(x => (string)x.Attribute("Name") == datastream.PlaylistName).Remove();
                doc.Save(_filepath);
            }
            catch(InvalidCastException ex)
            {
                throw new WrongParametersException("Object is not of type PlaylistHolder", ex.StackTrace);
            }
        }

        /// <summary>
        /// Removes a list of playlists
        /// </summary>
        /// <param name="OtobeRemoved">List of PlaylistHolder to remove</param>
        public void RemoveItems(object OtobeRemoved)
        {
            try
            {
                List<PlaylistHolder> datastream = (List<PlaylistHolder>)OtobeRemoved;
                foreach (var data in datastream)
                    RemoveItem(data);
            }
            catch (InvalidCastException ex)
            {
                throw new WrongParametersException("Object is not of type List<PlaylistHolder>", ex.StackTrace);
            }
        }

        private bool Exists(PlaylistHolder data)
        {
            foreach (XElement element in doc.Root.Descendants("Playlist"))
            {
                if (data.PlaylistName == (string)element.Attribute("Name"))
                    return true;
            }
            return false;
        }
    }
}