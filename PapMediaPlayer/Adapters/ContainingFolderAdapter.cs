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
    class ContainingFolderAdapter : BaseAdapter
    {
        Context context;
        private List<string> _containingFolders;


        public ContainingFolderAdapter(Context context, List<string> ContainingFolders)
        {
            this.context = context;
            this._containingFolders = ContainingFolders;
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
            ContainingFolderAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as ContainingFolderAdapterViewHolder;

            if (holder == null)
            {
                holder = new ContainingFolderAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //replace with your item and your holder items
                //comment back in
                view = inflater.Inflate(Resource.Layout.ContainingFolderRow, parent, false);
                holder.ll = view.FindViewById<LinearLayout>(Resource.Id.ContainingFolderRow_LinearLayout);
                holder.Title = view.FindViewById<TextView>(Resource.Id.ContainingFolderRow_TextView);
                view.Tag = holder;
               // selected.Add(new SelectedItemsModel { text = holder.ll, IsSelected = false});
            }


            holder.Title.Text = _containingFolders[position];
            return view;
        }

        //Fill in cound here, currently _containingFolders.Count
        public override int Count
        {
            get
            {
                return _containingFolders.Count;
            }
        }


    }

    class ContainingFolderAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
        public LinearLayout ll { get; set; }
    }
}