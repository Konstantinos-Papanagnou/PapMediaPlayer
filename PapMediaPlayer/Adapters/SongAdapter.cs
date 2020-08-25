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

namespace PapMediaPlayer.Adapters
{

    public class SongAdapter : BaseAdapter
    {
        List<Track> tracks;
        Context context;

        public SongAdapter(Context context, List<Track> tracks)
        {
            this.tracks = tracks;
            this.context = context;
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
            SongAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as SongAdapterViewHolder;

            if (holder == null)
            {
                holder = new SongAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //replace with your item and your holder items
                //comment back in

                view = inflater.Inflate(Resource.Layout.SongAdapterRow, parent, false);
                holder.Title = view.FindViewById<TextView>(Resource.Id.SongAdapterRow_titleViewLeft);
                holder.ImageView = view.FindViewById<ImageView>(Resource.Id.SongAdapterRow_imageViewLeft);
                holder.LL = view.FindViewById<LinearLayout>(Resource.Id.SongAdapterRow_LinearLayout);
                view.Tag = holder;
            }

            holder.Title.Text = tracks[position].Name;
            if (tracks[position].ContainsImage)
                holder.ImageView.SetImageBitmap(tracks[position].Image);
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
            return view;
        }

        //Fill in cound here, currently tracks.Count
        public override int Count
        {
            get
            {
                return tracks.Count;
            }
        }

    }

    class SongAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
        public ImageView ImageView { get; set; }
        public LinearLayout LL { get; set; }
    }
}