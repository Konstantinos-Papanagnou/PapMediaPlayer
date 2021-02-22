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
using Android.Media;
using PapMediaPlayer.Models;
using PapMediaPlayer.Adapters;

namespace PapMediaPlayer
{
    public enum ViewMethods
    {
        AllTracks,
        ContainingFolders,
        Playlist,
        Selection,
        ContainingFolderSongs
    }

    public enum RepeatMethod
    {
        RepeatOne,
        RepeatAllInOrder,
        NoRepeat,
        RepeatOnce,
        RandomReplay
    }

    public class ViewMethodEventArgs
    {
        public ViewMethods newVMethod { get; private set; }
        public ViewMethodEventArgs(ViewMethods VMethod)
        {
            newVMethod = VMethod;
        }
    }

    public class RepeatMethodEventArgs:EventArgs
    {
        public RepeatMethod newRMethod { get; private set; }
        public RepeatMethodEventArgs(RepeatMethod RMethod)
        {
            newRMethod = RMethod;
        }
    }

    public class ViewPlayManager
    {

        public bool reachedEnd = false;
        private int prevPos = 0;

        public delegate void ViewMethodChanged(object sender, ViewMethodEventArgs e);
        public event ViewMethodChanged OnViewMethodChanged;

        public delegate void RepeatMethodChanged(object sender, RepeatMethodEventArgs e);
        public event RepeatMethodChanged OnRepeatMethodChanged;

        private RepeatMethod rep;
        private ViewMethods viewm;

        public RepeatMethod Repeat
        {
            get { return rep; }
            set
            {
                rep = value;
                if (OnRepeatMethodChanged == null)
                    return;
                RepeatMethodEventArgs args = new RepeatMethodEventArgs(value);
                OnRepeatMethodChanged(this, args);
            }
        }

        public ViewMethods ViewMethod
        {
            get { return viewm; }
            set
            {
                viewm = value;
                if (OnViewMethodChanged == null)
                    return;
                ViewMethodEventArgs args = new ViewMethodEventArgs(value);
                OnViewMethodChanged(this, args);
            }
        }

        #region Constructors
        /// <summary>
        /// Creates a new instance of ViewPlayManager with default values
        /// Default Values: ViewMethod -> AllTracks
        /// RepeatMode -> RepeatAllInOrder
        /// </summary>
        public ViewPlayManager() : this(ViewMethods.AllTracks, RepeatMethod.RepeatAllInOrder)
        {
        }

        /// <summary>
        /// Creates a new instance of ViewPlayManager with specified viewMethod and default RepeatMode
        /// Default Value: RepeatMode -> RepeatAllInOrder
        /// </summary>
        /// <param name="viewMethod">View Method style</param>
        public ViewPlayManager(ViewMethods viewMethod) : this(viewMethod, RepeatMethod.RepeatAllInOrder)
        {
        }

        /// <summary>
        /// Creates a new instance of ViewPlayManager with specified RepeatMethod and default ViewMethod
        /// Default Value: ViewMethod -> AllTracks
        /// </summary>
        /// <param name="RepMethod">Repeat Method style</param>
        public ViewPlayManager(RepeatMethod RepMethod) : this(ViewMethods.AllTracks, RepMethod)
        {
        }

        /// <summary>
        /// Creates a new instance of ViewPlayManager
        /// </summary>
        /// <param name="viewMethod">View Method style</param>
        /// <param name="RepMethod">Repeat Method style</param>
        public ViewPlayManager(ViewMethods viewMethod, RepeatMethod RepMethod)
        {
            viewm = viewMethod;
            rep = RepMethod;
        }
        #endregion

        /// <summary>
        /// Handles repeats and returns the new position to go and seek in the list
        /// </summary>
        /// <param name="max">Max of the list</param>
        /// <param name="position">The position in the tracks list</param>
        /// <returns>The new position</returns>
        public int HandleAutoRepeat(int max, int position)
        {
            switch (Repeat)
            {
                case RepeatMethod.RandomReplay:
                    Random rand = new Random(DateTime.Now.Millisecond);
                    prevPos = position;
                    int value = rand.Next(0, max);
                    if (value == position && max > 8)
                        value = HandleAutoRepeat(max, position);
                    return value;
                case RepeatMethod.RepeatOne:
                    if (position > max - 1)
                        return 0;
                    return position;
                case RepeatMethod.NoRepeat:
                    return position;
                case RepeatMethod.RepeatOnce:
                    if (position < max - 1)
                    {
                        reachedEnd = false;
                        return ++position;
                    }
                    reachedEnd = true;
                    return position;
                default:
                    if (position >= max - 1)
                        return 0;
                    return ++position;
            }
        }

        /// <summary>
        /// Handles user activity
        /// </summary>
        /// <param name="max">Max of the list</param>
        /// <param name="position">The position in the tracks list</param>
        /// <returns>The new position</returns>
        public int HandleUserActivity(int max, int position, bool backwards = false)
        {
            if (Repeat == RepeatMethod.RandomReplay)
            {
                if (backwards && position != prevPos && prevPos < max)
                    return prevPos;            
                Random rand = new Random(DateTime.Now.Millisecond);
                int value = rand.Next(0, max);
                if (value == position && max > 8)
                {
                    value = HandleUserActivity(max, position);
                }
                prevPos = position;           
                return value;
            }
            else
            {
                if (backwards)
                {
                    if (position <= 0)
                        return max - 1;
                    return --position;
                }
                if (position >= max - 1)
                    return 0;
                return ++position;
            }
        }
        /// <summary>
        /// Sets the adapter accordingly based on the ViewMethod enumeration
        /// </summary>
        /// <param name="context"></param>
        /// <param name="view"></param>
        /// <param name="tracks"></param>
        /// <param name="contFolders"></param>
        public void SetAdapter(Context context, ref GridView view, List<Track> tracks, List<string> contFolders)
        {
            switch (ViewMethod)
            {
                case ViewMethods.AllTracks:
                    view.Adapter = new SongAdapter(context, tracks);
                    break;
                case ViewMethods.ContainingFolders:
                    view.Adapter = new ContainingFolderAdapter(context, contFolders);
                    break;
                case ViewMethods.Playlist:
                    view.Adapter = new SongAdapter(context, tracks);
                    break;
                case ViewMethods.Selection:
                    view.Adapter = new SongMultiSelectAdapter(context, ref MainActivity.selectedItems);
                    break;
                case ViewMethods.ContainingFolderSongs:
                    view.Adapter = new SongAdapter(context, tracks);
                    break;
            }
        }
    }
}