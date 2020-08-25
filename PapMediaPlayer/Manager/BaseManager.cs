using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PapMediaPlayer.Manager
{
    public interface IReplayManager: Java.IO.ISerializable
    {
        int HandleAutoReplay(int max, int position);
        int HandleUserActivity(int max, int position, bool backwards = false);
        int GetIcon();
        string GetTitle();
        RepeatMethod Enumerate();
        IReplayManager Next();
    }

    public abstract class BaseClass : Java.Lang.Object, IReplayManager
    {

        public abstract int GetIcon();

        public abstract string GetTitle();

        public abstract RepeatMethod Enumerate();

        public abstract int HandleAutoReplay(int max, int position);

        public virtual int HandleUserActivity(int max, int position, bool backwards = false)
        {
            if (backwards)
            {
                if (position <= 0)
                    return max - 1;
                return --position;
            }
            if (position >= max - 1)
                return 0;
            return ++position;
        }

        public abstract IReplayManager Next();
    }

    public class RandomReplay : BaseClass
    {

        private int prevPos = 0;

        public override int GetIcon()
        {
            return Resource.Drawable.Random24;
        }

        public override RepeatMethod Enumerate()
        {
            return RepeatMethod.RandomReplay;
        }

        public override string GetTitle()
        {
            return "Random";
        }

        public override int HandleAutoReplay(int max, int position)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            prevPos = position;
            int value = rand.Next(0, max);
            if (value == position && max > 8)
                value = HandleAutoReplay(max, position);
            return value;
        }

        public override int HandleUserActivity(int max, int position, bool backwards = false)
        {
            if (backwards && position != prevPos && prevPos < max)
                return prevPos;
            Random rand = new Random(DateTime.Now.Millisecond);
            int value = rand.Next(0, max);
            if (value == position && max > 8)
            {
                value = HandleUserActivity(max, position);
            }
            prevPos = position;
            return value;
        }

        public override IReplayManager Next()
        {
            return new NoRepeat();
        }
    }

    public class RepeatOnce : BaseClass
    {
        public override RepeatMethod Enumerate()
        {
            return RepeatMethod.RepeatOnce;
        }

        public override int GetIcon()
        {
            return Resource.Drawable.RepeatOnce24;
        }

        public override string GetTitle()
        {
            return "Repeat Once";
        }

        public override int HandleAutoReplay(int max, int position)
        {
            if (position < max - 1)
            {
                return ++position;
            }
            return -1;
        }

        public override IReplayManager Next()
        {
            return new RepeatAllInOrder();
        }
    }

    public class NoRepeat : BaseClass
    {
        public override RepeatMethod Enumerate()
        {
            return RepeatMethod.NoRepeat;
        }

        public override int GetIcon()
        {
            return Resource.Drawable.NoLoopback24;
        }

        public override string GetTitle()
        {
            return "No Loopback";
        }

        public override int HandleAutoReplay(int max, int position)
        {
            return position;
        }
        public override IReplayManager Next()
        {
            return new RepeatOne();
        }
    }

    public class RepeatAllInOrder : BaseClass
    {
        public override RepeatMethod Enumerate()
        {
            return RepeatMethod.RepeatAllInOrder;
        }

        public override int GetIcon()
        {
            return Resource.Drawable.LoopInOrder24;
        }

        public override string GetTitle()
        {
            return "Loop IN Order";
        }

        public override int HandleAutoReplay(int max, int position)
        {
            if (position >= max - 1)
                return 0;
            return ++position;
        }

        public override IReplayManager Next()
        {
            return new RandomReplay();
        }
    }

    public class RepeatOne : BaseClass
    {
        public override RepeatMethod Enumerate()
        {
            return RepeatMethod.RepeatOne;
        }

        public override int GetIcon()
        {
            return Resource.Drawable.RepeatOne24;
        }

        public override string GetTitle()
        {
            return "Repeat One";
        }

        public override int HandleAutoReplay(int max, int position)
        {
            if (position > max - 1)
                return 0;
            return position;
        }

        public override IReplayManager Next()
        {
            return new RepeatOnce();
        }
    }
}