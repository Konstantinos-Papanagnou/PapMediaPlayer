package md5998dbd540df1000ab82b54a0202a9092;


public class NoRepeat
	extends md5998dbd540df1000ab82b54a0202a9092.BaseClass
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("PapMediaPlayer.Manager.NoRepeat, PapMediaPlayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", NoRepeat.class, __md_methods);
	}


	public NoRepeat () throws java.lang.Throwable
	{
		super ();
		if (getClass () == NoRepeat.class)
			mono.android.TypeManager.Activate ("PapMediaPlayer.Manager.NoRepeat, PapMediaPlayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
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
