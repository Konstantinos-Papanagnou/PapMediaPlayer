package crc64868f28a9e7eac848;


public abstract class BaseClass
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		java.io.Serializable
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("PapMediaPlayer.Manager.BaseClass, PapMediaPlayer", BaseClass.class, __md_methods);
	}


	public BaseClass ()
	{
		super ();
		if (getClass () == BaseClass.class)
			mono.android.TypeManager.Activate ("PapMediaPlayer.Manager.BaseClass, PapMediaPlayer", "", this, new java.lang.Object[] {  });
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
