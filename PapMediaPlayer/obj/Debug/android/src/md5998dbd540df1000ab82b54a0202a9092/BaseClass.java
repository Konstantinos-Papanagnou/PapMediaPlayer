package md5998dbd540df1000ab82b54a0202a9092;


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
		mono.android.Runtime.register ("PapMediaPlayer.Manager.BaseClass, PapMediaPlayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", BaseClass.class, __md_methods);
	}


	public BaseClass () throws java.lang.Throwable
	{
		super ();
		if (getClass () == BaseClass.class)
			mono.android.TypeManager.Activate ("PapMediaPlayer.Manager.BaseClass, PapMediaPlayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
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
