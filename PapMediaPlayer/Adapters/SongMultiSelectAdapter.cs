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

namespace PapMediaPlayer.Models
{
    class SongMultiSelectAdapter : BaseAdapter
    {

        Context context;
        EventList<SelectableModel<Track>> data;
        public SongMultiSelectAdapter(Context context, ref EventList<SelectableModel<Track>> data)
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
            SongMultiSelectAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as SongMultiSelectAdapterViewHolder;

            if (holder == null)
            {
                holder = new SongMultiSelectAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //replace with your item and your holder items
                //comment back in
                view = inflater.Inflate(Resource.Layout.SongMultiSelectRow, parent, false);
                holder.Title = view.FindViewById<TextView>(Resource.Id.SongMultiSelectRow_title);
                holder.ImageView = view.FindViewById<ImageView>(Resource.Id.SongMultiSelectRow_imageView);
                holder.Validator = view.FindViewById<Switch>(Resource.Id.SongMultiSelectRow_Validator);
                holder.Validator.CheckedChange += Validator_CheckedChange;
                
                view.Tag = holder;           
            }
            holder.Title.Text = data[position].Data.FullTitle;
            if (data[position].Data.ContainsImage)
                holder.ImageView.SetImageBitmap(data[position].Data.Image);
            else
            {
                using (Android.Graphics.BitmapFactory.Options opt = new Android.Graphics.BitmapFactory.Options())
                {
                    opt.OutHeight = 48;
                    opt.OutWidth = 48;
                    opt.InSampleSize = 2;
                    holder.ImageView.SetImageBitmap(Android.Graphics.BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.Note, opt));
                }
            }
            holder.Validator.Checked = data[position].Selected;

            //fill in your items
            //holder.Title.Text = "new text here";

            return view;
        }

        private void Validator_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            var obj = sender as Switch;
            var row = obj.Parent as LinearLayout;
            var parent = row.Parent as GridView;
            if (parent == null)
                return;
            int position = parent.GetPositionForView(row);
            data[position].Selected = e.IsChecked;
            //Toast.MakeText(context, data[position].Data.FullTitle, ToastLength.Long).Show();
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

    class SongMultiSelectAdapterViewHolder : Adapters.SongAdapterViewHolder
    {
        //Your adapter views to re-use
        //public TextView Title { get; set; }
        public Switch Validator { get; set; }
    }
}