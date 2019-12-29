using PhotoSlideshow.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace PhotoSlideshow
{
    class Program
    {
        static void Main(string[] args)
        {
            
            int fileToRead = 3;
            int numberOfIterations = 10;

            Random random = new Random();
            Solution solution = new Solution();

            string[] files = Directory.GetFiles($"Samples", "*.txt");

            List<Slide> slides = new List<Slide>();
            Instance instance = Extensions.IO.ReadInput(files[fileToRead]);

            Console.WriteLine($"Number of photos: {instance.NumberOfPhotos}\n");
            
            //solution.HillClimbing(numberOfIterations);
           
            solution.IteratedLocalSearch(numberOfIterations, 1000);
           // solution.HillClimbingWithAdditionalFeatures(numberOfIterations);

        
            solution.GenerateOutputFile($"{Path.GetFileNameWithoutExtension(files[fileToRead])}_result_{DateTime.Now.Ticks}.txt");

            Console.WriteLine($"Number of slides: { solution.Slides.Count() }\n");

            Console.ReadKey();
           
        }
    }
}
