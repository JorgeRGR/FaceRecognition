using System;
using System.Collections.Generic;
using System.Text;

namespace FaceRecognition
{
    public class OcrResponse
    {
        public List<Category> Categories { get; set; }
        public Description Description { get; set; }
        public string RequestId { get; set; }
        public Metadata Metadata { get; set; }
        public Color Color { get; set; }
    }

    public class Category
    {
        public string Name { get; set; }
        public double Score { get; set; }
    }

    public class Description
    {
        public List<string> Tags { get; set; }
        public List<Caption> Captions { get; set; }
    }

    public class Caption
    {
        public string Text { get; set; }
        public double Confidence { get; set; }
    }

    public class Metadata
    {
         public double Width { get; set; }
        public double Height { get; set; }
        public string Format { get; set; }
    }

    public class Color
    {
        public string DominantColorForeground { get; set; }
        public string DominantColorBackground { get; set; }
        public List<string> DominantColors { get; set; }
        public string AccentColor { get; set; }
        public bool IsBWImg { get; set; }
    }
}
