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

namespace PapMediaPlayer.Adapters
{
    public class ForListViewMultiSelectWithColorChange : BaseAdapter
    {
        public class DataType
        {
            public string title { get; set; }
            public bool selected { get; set; }
            public string Path { get; set; }
        }

        Context context;
        List<DataType> data;
        public ForListViewMultiSelectWithColorChange(Context context, ref List<DataType> data)
        {
            this.context = context;
            this.data = data;
        }


        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            ForListViewMultiSelectWithColorChangeViewHolder holder = null;

            if (view != null)
                holder = view.Tag as ForListViewMultiSelectWithColorChangeViewHolder;

            if (holder == null)
            {
                holder = new ForListViewMultiSelectWithColorChangeViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //replace with your item and your holder items
                //comment back in
                view = inflater.Inflate(Resource.Layout.MultiListViewRow, parent, false);
                holder.Title = view.FindViewById<TextView>(Resource.Id.MultiListViewRow_TextView);

                
                view.Tag = holder;
            }

            if (data[position].selected)
                view.SetBackgroundColor(Android.Graphics.Color.Blue);
            else view.SetBackgroundColor(Android.Graphics.Color.Transparent);
            //fill in your items
            holder.Title.Text = data[position].title;

            return view;
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return data.Count;
            }
        }

    }

    class ForListViewMultiSelectWithColorChangeViewHolder : Java.Lang.Object
    {
        public TextView Title { get; set; }
    }
}