package crc64868f28a9e7eac848;


public class NoRepeat
	extends crc64868f28a9e7eac848.BaseClass
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("PapMediaPlayer.Manager.NoRepeat, PapMediaPlayer", NoRepeat.class, __md_methods);
	}


	public NoRepeat ()
	{
		super ();
		if (getClass () == NoRepeat.class)
			mono.android.TypeManager.Activate ("PapMediaPlayer.Manager.NoRepeat, PapMediaPlayer", "", this, new java.lang.Object[] {  });
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
