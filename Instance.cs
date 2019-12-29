using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoSlideshow.Models
{
    public class Instance
    {
        public Instance()
        {
            Photos = new List<Photo>();
        }
        public int NumberOfPhotos { get; set; } // N
        public List<Photo> Photos { get; set; }
    }
}
