using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWPrecinctEditor
{
    class Area
    {
        public string Name { get; set; }
        public int ID { get; set; }            
        public int PointsCount { get; set; }
        public int MarkersCount { get; set; }
        public int DomainID { get; set; }
        public int DestinationMapID { get; set; }
        public int SoundTracksCount { get; set; }
        public int SoundtrackInterval { get; set; }
        public bool LoopSoundtrack { get; set; } // 0 = false, 1 = true
        public int OriginMapID { get; set; }
        public int Priority { get; set; }
        public bool PKProtection { get; set; } // 0 = false, 1 = true
        public Point Respawn { get; set; }
        public List<Point> Points = new List<Point>();
        public List<Marker> Markers = new List<Marker>();
        public string DaySoundtrack { get; set; }
        public List<string> Musics = new List<string>();
        public string NightSoundtrack { get; set; }
      
        public void AddPoint(Point point)
        {
            this.Points.Add(point);
            PointsCount++;
        }
        public void AddMarker(Marker marker)
        {
            this.Markers.Add(marker);
            MarkersCount++;
        }
        public void AddMusic(string music)
        {
            this.Musics.Add(music);
            SoundTracksCount++;
        }

        public void RemovePoint(Point pointRemoved)
        {
            int id = 1;
            this.Points.Remove(pointRemoved);
            foreach(Point point in Points)            
                point.ID = id++;

            PointsCount = Points.Count;
        }
        /*public void AddMarker(Marker marker)
        {
            this.Markers.Add(marker);
            MarkersCount++;
        }*/
        public void RemoveMusic(string musicRemoved)
        {           
            this.Musics.Remove(musicRemoved);
            SoundTracksCount--;
        }
    }
    
}
