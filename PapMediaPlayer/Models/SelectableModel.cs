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

namespace PapMediaPlayer.Models
{
    public class SelectableEventArgs: EventArgs
    {
        public bool Selected { get; set; }
        public SelectableEventArgs(bool Selected)
        {
            this.Selected = Selected;
        }
    }

    public class SelectableModel<T>
    {
        public T Data { get; set; }
        private bool selected;
        public bool Selected { get { return selected; } set { selected = value; if(OnSelectionChanged != null && OnSelectionChanged.GetInvocationList().Any()) OnSelectionChanged(this, new SelectableEventArgs(value)); } }

        public delegate void SelectionChanged(object sender, SelectableEventArgs e);
        public event SelectionChanged OnSelectionChanged;
    }

    public class PosArgs:EventArgs
    {
        public int Position { get; private set; }
        public PosArgs(int Position)
        {
            this.Position = Position;
        }
    }

    public class EventList<T>: List<T>
    {
        public delegate void AddItem();
        public event AddItem OnItemAdded;

        public delegate void RemoveItem(PosArgs e);
        public event RemoveItem OnItemRemoved;

        public delegate void ClearList();
        public event ClearList OnListCleared;

        public new void Add(T item)
        {
            base.Add(item);
            if (OnItemAdded != null && OnItemAdded.GetInvocationList().Any())
            {
                OnItemAdded();
            }
        }

        public void RemoveAndNotify(int position)
        {
            if(OnItemRemoved != null && OnItemRemoved.GetInvocationList().Any())
            {
                OnItemRemoved(new PosArgs(position));
            }
            base.RemoveAt(position);
        }

        public new void Clear()
        {
            if(OnListCleared != null && OnListCleared.GetInvocationList().Any())
            {
                OnListCleared();
            }
            base.Clear();
        }
    }
}