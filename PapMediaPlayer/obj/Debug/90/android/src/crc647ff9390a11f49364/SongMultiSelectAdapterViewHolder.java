package crc647ff9390a11f49364;


public class SongMultiSelectAdapterViewHolder
	extends crc6490ae823641bf8813.SongAdapterViewHolder
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("PapMediaPlayer.Models.SongMultiSelectAdapterViewHolder, PapMediaPlayer", SongMultiSelectAdapterViewHolder.class, __md_methods);
	}


	public SongMultiSelectAdapterViewHolder ()
	{
		super ();
		if (getClass () == SongMultiSelectAdapterViewHolder.class)
			mono.android.TypeManager.Activate ("PapMediaPlayer.Models.SongMultiSelectAdapterViewHolder, PapMediaPlayer", "", this, new java.lang.Object[] {  });
	}

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
