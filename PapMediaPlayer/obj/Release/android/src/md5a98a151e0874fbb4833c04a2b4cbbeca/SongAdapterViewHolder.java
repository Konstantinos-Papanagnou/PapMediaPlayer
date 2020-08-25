package md5a98a151e0874fbb4833c04a2b4cbbeca;


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
		mono.android.Runtime.register ("PapMediaPlayer.Adapters.SongAdapterViewHolder, PapMediaPlayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", SongAdapterViewHolder.class, __md_methods);
	}


	public SongAdapterViewHolder () throws java.lang.Throwable
	{
		super ();
		if (getClass () == SongAdapterViewHolder.class)
			mono.android.TypeManager.Activate ("PapMediaPlayer.Adapters.SongAdapterViewHolder, PapMediaPlayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
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
