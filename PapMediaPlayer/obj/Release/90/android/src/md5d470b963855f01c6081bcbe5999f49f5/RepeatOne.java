package md5d470b963855f01c6081bcbe5999f49f5;


public class RepeatOne
	extends md5d470b963855f01c6081bcbe5999f49f5.BaseClass
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("PapMediaPlayer.Manager.RepeatOne, PapMediaPlayer", RepeatOne.class, __md_methods);
	}


	public RepeatOne ()
	{
		super ();
		if (getClass () == RepeatOne.class)
			mono.android.TypeManager.Activate ("PapMediaPlayer.Manager.RepeatOne, PapMediaPlayer", "", this, new java.lang.Object[] {  });
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
