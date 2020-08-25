using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PapMediaPlayer.Models;
using PapMediaPlayer.XmlParser.Exceptions;
using Newtonsoft.Json;
using System.IO;

namespace PapMediaPlayer.XmlParser
{
    /// <summary>
    /// Use this class to easily Parse/Read/Write Xml files as playlists.
    /// This Class implements IXmlParsable and IDisposable.
    /// </summary>
    /// <exception cref="WrongParametersException">Throws WrongParametersException when the parameters are not valid</exception>
    public sealed class JsonPlaylistParser : IXmlParsable
    {
        private string path;
        List<Paths> Items;

        /// <summary>
        /// Creates an instance of a XmlPlayListParser by loading an existing xml file
        /// </summary>
        /// <param name="path">The specified path to the playlist name without the *.xml extention</param>
        /// <param name="playlistName">The name of the playlist and xml file [playlistName].xml</param>
        public JsonPlaylistParser(string path, string playlistName)
        {
            this.path = System.IO.Path.Combine(path, playlistName);
            if (File.Exists(this.path))
                Items = JsonConvert.DeserializeObject<List<Paths>>(File.ReadAllText(this.path));
            else Items = new List<Paths>();
        }

        /// <summary>
        /// Creates an instance of a XmlPlayListParser by creating a new xml file
        /// </summary>
        /// <param name="path">The specified path to the playlist name without the *.xml extention</param>
        /// <param name="playlistName">The name of the playlist and xml file [playlistName].xml</param>
        /// <param name="tobeAdded">The list of songs to add</param>
        public JsonPlaylistParser(string path, string playlistName, List<Paths> tobeAdded)
        {
            this.path = System.IO.Path.Combine(path, playlistName);
            if (File.Exists(this.path))
                Items = JsonConvert.DeserializeObject<List<Paths>>(File.ReadAllText(this.path));
            else Items = new List<Paths>(); if (tobeAdded != null)
                AddItems(tobeAdded);
        }

        public void Dispose()
        {
            path = null;
            Items = null;
        }

        private bool Exists(Paths track)
        {
            foreach (var item in Items)
            {
                if (track.Path == item.Path)
                    return true;
            }
            return false;
        }

        public object FetchItems()
        {
            return Items;
        }

        public void AddItem(object OtobeAdded)
        {
            try
            {
                Paths tobeAdded = (Paths)OtobeAdded;
                if (Exists(tobeAdded))
                    return;
                Items.Add(tobeAdded);
                File.WriteAllText(path, JsonConvert.SerializeObject(Items));
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
                    Items.Add(track);
                }
                File.WriteAllText(path, JsonConvert.SerializeObject(Items));
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
                Items.Remove(tobeRemoved);
                File.WriteAllText(path, JsonConvert.SerializeObject(Items));
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
                    Items.Remove(track);
                }
                File.WriteAllText(path, JsonConvert.SerializeObject(Items));
            }
            catch (InvalidCastException iex)
            {
                throw new WrongParametersException("Object tobeAdded is not Paths struct.", iex.StackTrace);
            }
       }
    }
}