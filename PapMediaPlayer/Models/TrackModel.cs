using Android.Graphics;
using System.Collections.Generic;

namespace PapMediaPlayer.Models
{

    public struct Paths
    {
        public string Title { get; set; }
        public string Path { get; set; }
    }
    [System.Serializable]
    public class Track
    { 
        public string FullTitle { get; set; }
        public bool ContainsImage { get; set; }
        public string Name { get; set; }
        public string ContainingFolderName { get; set; }
        public string Album { get; set; }
        public string AuthorName { get; set; }
        public Bitmap Image { get; set; }
        public string Length { get; set; }
        public double DurationMins { get; set; }
        public string Path { get; set; }
        public string Copyrights { get; set; }
        public string Genre { get; set; }
        public uint Year { get; set; }
        public bool isCorrupted { get; set; }
        public IEnumerable<string> CorruptionReason { get; set; }
    }
}