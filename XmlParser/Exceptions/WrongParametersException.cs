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
    public class WrongParametersException : Exception
    {
        private string message;
        private string stackTrace;
        public override string StackTrace
        {
            get
            {
                return stackTrace;
            }
        }
        public string FullMessage { get { return message; } private set { message = value; } }
        public WrongParametersException(string message, string stackTrace) : base(message)
        {
            this.stackTrace = stackTrace;
            FullMessage = message + ". Origin: " + StackTrace;
        }
    }
}