package md512a0ec41906b782714e5bcf84bdac370;


public class SongAdapterViewHolder
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("PapMediaPlayer.Adapters.SongAdapterViewHolder, PapMediaPlayer", SongAdapterViewHolder.class, __md_methods);
	}


	public SongAdapterViewHolder ()
	{
		super ();
		if (getClass () == SongAdapterViewHolder.class)
			mono.android.TypeManager.Activate ("PapMediaPlayer.Adapters.SongAdapterViewHolder, PapMediaPlayer", "", this, new java.lang.Object[] {  });
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
