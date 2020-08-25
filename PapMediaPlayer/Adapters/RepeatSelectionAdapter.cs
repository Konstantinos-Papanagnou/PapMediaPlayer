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
using Android.Graphics;

namespace PapMediaPlayer.Adapters
{
    public class RepeatSelectionAdapter : BaseAdapter
    {
        public struct Input
        {
            public Bitmap Icon;
            public string Text;
        }
        
        Context context;
        List<Input> input;

        public RepeatSelectionAdapter(Context context, List<Input> data)
        {
            this.context = context;
            input = data;
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
            RepeatSelectionAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as RepeatSelectionAdapterViewHolder;

            if (holder == null)
            {
                holder = new RepeatSelectionAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //replace with your item and your holder items
                //comment back in
                view = inflater.Inflate(Resource.Layout.RepeatSelectionRow, parent, false);
                holder.Text = view.FindViewById<TextView>(Resource.Id.RepeatSelectionRow_TextView);
                holder.Image = view.FindViewById<ImageView>(Resource.Id.RepeatSelectionRow_ImageView);
                view.Tag = holder;
            }


            //fill in your items
            holder.Text.Text = input[position].Text;
            holder.Image.SetImageBitmap(input[position].Icon);

            return view;
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return input.Count;
            }
        }

    }

    class RepeatSelectionAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Text { get; set; }
        public ImageView Image { get; set; }
    }
}