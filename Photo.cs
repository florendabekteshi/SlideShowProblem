using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoSlideshow.Models
{
    [Serializable]
    public class Photo
    {
        public Photo(int id, Orientation orientation, List<string> tags)
        {
            Id = id;
            Orientation = orientation;
            Tags = tags;
        }

        public int Id { get; set; }
        public Orientation Orientation { get; set; }
        public List<string> Tags { get; set; }
    }

    public enum Orientation
    {
        V,
        H
    }
}
