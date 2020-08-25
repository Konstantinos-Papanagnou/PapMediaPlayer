package md52e7274c294085699d2db8b2bd92a848c;


public class SongMultiSelectAdapterViewHolder
	extends md5a98a151e0874fbb4833c04a2b4cbbeca.SongAdapterViewHolder
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("PapMediaPlayer.Models.SongMultiSelectAdapterViewHolder, PapMediaPlayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", SongMultiSelectAdapterViewHolder.class, __md_methods);
	}


	public SongMultiSelectAdapterViewHolder () throws java.lang.Throwable
	{
		super ();
		if (getClass () == SongMultiSelectAdapterViewHolder.class)
			mono.android.TypeManager.Activate ("PapMediaPlayer.Models.SongMultiSelectAdapterViewHolder, PapMediaPlayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
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
