package md5ab56a5c012e9c4ca38112ab193deb3dc;


public class SplashScreen3
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"";
		mono.android.Runtime.register ("PapMediaPlayer.Activities_and_fragments.SplashScreen3, PapMediaPlayer", SplashScreen3.class, __md_methods);
	}


	public SplashScreen3 ()
	{
		super ();
		if (getClass () == SplashScreen3.class)
			mono.android.TypeManager.Activate ("PapMediaPlayer.Activities_and_fragments.SplashScreen3, PapMediaPlayer", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);

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
