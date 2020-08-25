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
using Newtonsoft.Json;
using System.IO;

namespace PapMediaPlayer.XmlParser
{
    public sealed class JsonPlaylistsHolderParser : IXmlParsable
    {
        public struct PlaylistHolder
        {
            public string PlaylistName { get; set; }
            public string PlaylistPath { get; set; }
        }

        private string _filepath;
        readonly List<PlaylistHolder> Holder;

        /// <summary>
        /// Initializes a new instance of XmlPlaylistsHolderParser by opening an existing xml file
        /// </summary>
        /// <param name="filepath">The location for the existing xml file</param>
        public JsonPlaylistsHolderParser(string filepath)
        {
            this._filepath = filepath;
            if (File.Exists(filepath))
                Holder = JsonConvert.DeserializeObject<List<PlaylistHolder>>(File.ReadAllText(filepath));
            else Holder = new List<PlaylistHolder>();
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
                Holder.Add(datastream);
                File.WriteAllText(_filepath, JsonConvert.SerializeObject(Holder));
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
            _filepath = null;
        }

        /// <summary>
        /// Fetches items from the xml file
        /// </summary>
        /// <returns>List of PlaylistHolder</returns>
        public object FetchItems()
        {
            return Holder;
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
                Holder.Remove(datastream);
                File.WriteAllText(_filepath, JsonConvert.SerializeObject(Holder));
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
            foreach (var element in Holder)
            {
                if (data.PlaylistName == element.PlaylistName)
                    return true;
            }
            return false;
        }
    }
}