using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moos.Models
{
    public class PlaylistItem(Track track, float speed = 100, float pitch = 0)
    {
        public int Id { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public bool IsPlaying { get; set; } = false;
        public Track Track { get; set; } = track;
        public float Speed { get; set; } = speed;
        public float Pitch { get; set; } = pitch;
        public string DisplaySpeed
        {
            get { return Speed != Constants.DefaultPlayingSpeed ? Speed.ToString() + "%" : ""; }
        }
        public string DisplayPitch
        {
            get { return Pitch != Constants.DefaultPlayingPitch ? Pitch.ToString("0.0") : ""; }
        }
    }
}
