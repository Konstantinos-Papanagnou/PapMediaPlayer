package md5998dbd540df1000ab82b54a0202a9092;


public class RepeatOnce
	extends md5998dbd540df1000ab82b54a0202a9092.BaseClass
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("PapMediaPlayer.Manager.RepeatOnce, PapMediaPlayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", RepeatOnce.class, __md_methods);
	}


	public RepeatOnce () throws java.lang.Throwable
	{
		super ();
		if (getClass () == RepeatOnce.class)
			mono.android.TypeManager.Activate ("PapMediaPlayer.Manager.RepeatOnce, PapMediaPlayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
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
