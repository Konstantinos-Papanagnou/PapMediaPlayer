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

using PapMediaPlayer.Models;
using PapMediaPlayer.Receivers;

namespace PapMediaPlayer.ViewManager
{
    public class SelectManager: IViewManager, Search.ISearchManager
    {
        private readonly Context Context;
        private EventList<SelectableModel<Track>> SelectedTracks;
        private int count = 0;
        private readonly IViewManager Prev;
        public SelectManager(Context context, List<Track> tracks, IViewManager prev)
        {
            Prev = prev;
            Context = context;
            SelectedTracks = new EventList<SelectableModel<Track>>();
            SelectedTracks.OnItemAdded += SelectedTracks_OnItemAdded;
            SelectedTracks.OnItemRemoved += SelectedTracks_OnItemRemoved;
            SelectedTracks.OnListCleared += SelectedTracks_OnListCleared;
            if (SelectedTracks.Count > 0)
                SelectedTracks.Clear();
            for (int i = 0; i < tracks.Count; i++)
                SelectedTracks.Add(new SelectableModel<Track>() { Data = tracks[i], Selected = false });
        }

        public string EnumeratePath()
        {
            return string.Empty;
        }

        public BaseAdapter GetAdapter()
        {
            return new SongMultiSelectAdapter(Context, ref SelectedTracks);
        }

        public IViewManager OnBackPressed()
        {
            return Prev;
        }

        public IViewManager OnClickHandler(int position)
        {
            return this;
        }

        public void SynchronizeServiceTracks()
        {
            return;
        }

        private void SelectedTracks_OnListCleared()
        {
            foreach (var item in SelectedTracks)
            {
                item.OnSelectionChanged -= OnSelectionChanged;
            }
        }

        private void SelectedTracks_OnItemRemoved(PosArgs e)
        {
            SelectedTracks[e.Position].OnSelectionChanged -= OnSelectionChanged;
        }

        private void SelectedTracks_OnItemAdded()
        {
            SelectedTracks[SelectedTracks.Count - 1].OnSelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectableEventArgs e)
        {
            if (e.Selected) count++;
            else count--;
            if (count < 0) count = 0;
            if (Context is ICollectionListener)
                ((ICollectionListener)Context).Received(count);
        }

        public int TrackCount()
        {
            return SelectedTracks.Count;
        }

        public EventList<SelectableModel<Track>> GetTracks()
        {
            return SelectedTracks;
        }

        public BaseAdapter OnTextChanged(string Compare)
        {
            return new SongMultiSelectAdapter(Context, ref SelectedTracks);
        }

        public void SetSearch(bool searchActive)
        {
            return;
        }
    }
}