using System;

namespace PapMediaPlayer.XmlParser
{
    /// <summary>
    /// Interface to talk to a specified XmlParser
    /// </summary>
    public interface IXmlParsable: IDisposable
    {
        /// <summary>
        /// <para>For <see cref="XmlFilterParser.FetchItems"/> return type <see cref="XmlFilterParser.RVal"/></para>
        /// <para>For <see cref="XmlConfigParser.FetchItems"/> return type <see cref="XmlConfigParser.Configs"/></para>
        /// <para>For <see cref="XmlPlaylistParser.FetchItems"/> return type <see cref="Models.Paths"/></para>
        /// <para>For <see cref="XmlPlaylistsHolderParser.FetchItems"/> return type <see cref="XmlPlaylistsHolderParser.PlaylistHolder"/></para>
        /// </summary>
        /// <returns>Object to cast to a specific type</returns>
        object FetchItems();
        /// <summary>
        /// <para>For <see cref="XmlFilterParser.AddItem(object)"/> param type string or char</para>
        /// <para>For <see cref="XmlConfigParser.AddItem(object)"/> param type <see cref="XmlConfigParser.Configs"/></para>
        /// <para>For <see cref="XmlPlaylistParser.AddItem(object)"/> param type <see cref="Models.Track"/></para>
        /// <para>For <see cref="XmlPlaylistsHolderParser.AddItem(object)"/> param type <see cref="XmlPlaylistsHolderParser.PlaylistHolder"/></para> 
        /// </summary>
        /// <param name="OtobeAdded">Item to add</param>
        void AddItem(object OtobeAdded);
        /// <summary>
        /// <para>For <see cref="XmlFilterParser.AddItems(object)"/> param type string[] or char[]</para>
        /// <para>Dont call <see cref="XmlConfigParser.AddItems(object)"/> Exception: <see cref="Exceptions.FailCallException"/></para>
        /// <para>For <see cref="XmlPlaylistParser.AddItems(object)"/> param type List of <see cref="Models.Track"/></para>
        /// <para>For <see cref="XmlPlaylistsHolderParser.AddItems(object)"/> param type List of <see cref="XmlPlaylistsHolderParser.PlaylistHolder"/></para>
        /// </summary>
        /// <param name="OtobeAdded">Items to add</param>
        void AddItems(object OtobeAdded);
        /// <summary>
        /// <para>For <see cref="XmlFilterParser.RemoveItem(object)"/> param type string or char</para>
        /// <para>Dont call this method for <see cref="XmlConfigParser.RemoveItem(object)"/> Exception: <see cref="Exceptions.FailCallException"/></para>
        /// <para>For <see cref="XmlPlaylistParser.RemoveItem(object)"/> param type <see cref="Models.Track"/></para>
        /// <para>For <see cref="XmlPlaylistsHolderParser.RemoveItem(object)"/> param type <see cref="XmlPlaylistsHolderParser.PlaylistHolder"/></para>
        /// </summary>
        /// <param name="OtobeRemoved">Item to remove</param>
        void RemoveItem(object OtobeRemoved);
        /// <summary>
        /// <para>For <see cref="XmlFilterParser.RemoveItems(object)"/> param type string[] or char[]</para>
        /// <para>Dont call this method for <see cref="XmlConfigParser.RemoveItems(object)"/> Exception: <see cref="Exceptions.FailCallException"/></para>
        /// <para>For <see cref="XmlPlaylistParser.RemoveItems(object)"/> param type List of<see cref="Models.Track"/></para>
        /// <para>For<see cref="XmlPlaylistsHolderParser.RemoveItems(object)"/> param type List of<see cref="XmlPlaylistsHolderParser.PlaylistHolder"/></para>
        /// </summary>
        /// <param name="OtobeRemoved">Items to remove</param>
        void RemoveItems(object OtobeRemoved);
    }
}