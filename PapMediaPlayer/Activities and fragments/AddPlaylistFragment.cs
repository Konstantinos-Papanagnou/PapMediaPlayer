using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace PapMediaPlayer
{
    public class AddPlaylistFragment : DialogFragment
    {
        Button submit;
        EditText entry;
        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            submit.Click += Submit_Click;
            // Create your fragment here
        }

        private void Submit_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(entry.Text))
            {
                Toast.MakeText(Activity, "Empty fields are prohibited", ToastLength.Short).Show();
                return;
            }
            Playlist_Manager.PlaylistManager.CreatePlaylist(entry.Text);
            PlaylistManagerActivity.Finished = true;
            Dismiss();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            base.OnCreateView(inflater, container, savedInstanceState);
            Dialog.SetTitle("Add Playlist");
            View view = inflater.Inflate(Resource.Layout.AddPlaylistFragment, container, false);
            entry = view.FindViewById<EditText>(Resource.Id.AddPlaylistFragment_EditText);
            submit = view.FindViewById<Button>(Resource.Id.AddPlaylistFragment_Button);
            return view;
        }
    }
}