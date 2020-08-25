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

namespace PapMediaPlayer.XmlParser.Exceptions
{
    [Serializable]
    public class FailCallException : Exception
    {
        private string message;
        public override string Message { get { return message; } }
        public FailCallException(string message, Type classtype) : base(message)
        {
            this.message = message + ".Origin :" + classtype.FullName;
        }
    }
}