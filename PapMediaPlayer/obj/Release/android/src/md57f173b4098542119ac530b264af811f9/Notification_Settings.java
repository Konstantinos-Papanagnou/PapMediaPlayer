package md57f173b4098542119ac530b264af811f9;


public class Notification_Settings
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
		mono.android.Runtime.register ("PapMediaPlayer.Activities_and_fragments.Settings.Notification_Settings, PapMediaPlayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", Notification_Settings.class, __md_methods);
	}


	public Notification_Settings () throws java.lang.Throwable
	{
		super ();
		if (getClass () == Notification_Settings.class)
			mono.android.TypeManager.Activate ("PapMediaPlayer.Activities_and_fragments.Settings.Notification_Settings, PapMediaPlayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
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
