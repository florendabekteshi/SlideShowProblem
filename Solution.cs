using PhotoSlideshow.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PhotoSlideshow.Models
{
    public class Solution
    {
        public List<Slide> Slides { get; set; }
        public int InterestFactor { get; set; } = int.MinValue;
        private int TotalIterations;
        private double PerturbationPercentage  = 0.5;
        public Solution()
        {
            this.Slides = new List<Slide>();
        }
        public Solution(List<Slide> Slides)
        {
            this.Slides = Slides;
        }
        public void GenerateRandomSolution(List<Photo> photos)
        {
            int slideId = 0;
            Random random = new Random();
            List<int> photosToSkip = new List<int>();

            while (photosToSkip.Count() < photos.Count())
            {

                int randomStart = random.Next(0, photos.Count() - 1);
                Photo photo = photos.Where(x => randomStart == x.Id).FirstOrDefault();

                List<Photo> photosToAdd = new List<Photo>()
                {
                    photo
                };

                if (photo.Orientation == Orientation.V)
                {
                    Photo secondPhoto = photos.FirstOrDefault(x => x.Id != photo.Id && x.Orientation.Equals(Orientation.V) && !photosToSkip.Contains(x.Id));
                    if (secondPhoto != null)
                    {
                        photosToAdd.Add(secondPhoto);
                        photosToSkip.Add(secondPhoto.Id);
                    }
                }

                photosToSkip.Add(photo.Id);
                this.Slides.Add(new Slide(slideId, photosToAdd));
                slideId++;
            }
        }

      
        //Change position of first slide with last
        public List<Photo> ChangePosition(List<Photo> photo)
        {
            photo.OrderByDescending(x => x.Id);
            return photo;
        } 
        public List<Photo> ChangePositionOfPhoto(List<Photo> photo)
        {
            for(int i=0;i<photo.Count; i++)
            {
                var temp = photo[i];
                photo[i] = photo[i + 1];
                photo[i + 1] = temp;
            }   
            return photo;
        }

     

        public List <Slide> DifferentPositionsSlides(List <Slide> slide)
        {
            for (int i = 0; i < slide.Count; i++)
            {
                var temp = slide[i];
                slide[i] = slide[i+1];
                slide[i+1] = temp;
            }
            return slide;
        }
      
       
        public void GenerateSolutionWithHeuristic(List<Photo> photos)
        {
            Random random = new Random();
            int atThisNumber = Convert.ToInt32(Math.Sqrt(photos.Count));
            int slideId = 0;
            int photosCount = photos.Count();

            for (int i = 0; i < photosCount; i++)
            {
                List<Photo> tempPhotos = new List<Photo>(photos.Skip(i * i ).Take(atThisNumber -1));
                int tempPhotosCount = tempPhotos.Count();
                int iterationCount = 0;

                while (iterationCount < tempPhotosCount)
                {
                    Photo photo = tempPhotos.FirstOrDefault();
                    
                    List<Photo> photosToAdd = new List<Photo>()
                    {
                        photo
                    };

                    if (photo.Orientation == Orientation.V)
                    {
                        Photo secondPhoto = tempPhotos
                            .Where(x => x.Id != photo.Id && x.Orientation.Equals(Orientation.V))
                            .OrderByDescending(x =>
                                x.Tags.Where(t => !photo.Tags.Contains(t)).Count() +
                                x.Tags.Where(t => photo.Tags.Contains(t)).Count() +
                                photo.Tags.Where(t => x.Tags.Contains(t)).Count())
                            .FirstOrDefault();

                        if (secondPhoto != null)
                        {
                            photosToAdd.Add(secondPhoto);
                            tempPhotos.Remove(secondPhoto);

                            iterationCount++;
                        }
                    }

                    this.Slides.Add(new Slide(slideId, photosToAdd));
                    tempPhotos.Remove(photo);

                    iterationCount++;
                    slideId++;
                }
            }
        }

        public void Mutate(List<Slide> slides, List<int> randomNumbers)
        {
            Random random = new Random();
            int swapOrChange = random.Next(0, 9);
            List<int> slidesToSwap = slides.Where(x => x.Photos.Count == 2).OrderBy(x => random.Next()).Select(x => x.Id).Take(2).ToList();

            if (swapOrChange < 3 && slidesToSwap.Count == 2)
            {
                int firstSlidePhotoIndex = random.Next(0, 2);
                int secondSlidePhotoIndex = random.Next(0, 2);

                int firstSlideIndex = slides.IndexOf(slides.FirstOrDefault(x => x.Id == slidesToSwap.FirstOrDefault()));
                int secondSlideIndex = slides.IndexOf(slides.FirstOrDefault(x => x.Id == slidesToSwap.LastOrDefault()));

                List<Photo> firstSlidePhotos = new List<Photo>
                {
                    new Photo(slides[firstSlideIndex].Photos.FirstOrDefault().Id, Orientation.V, new List<string>(slides[firstSlideIndex].Photos.FirstOrDefault().Tags)),
                    new Photo(slides[firstSlideIndex].Photos.LastOrDefault().Id, Orientation.V, new List<string>(slides[firstSlideIndex].Photos.LastOrDefault().Tags))
                };

                List<Photo> secondSlidePhotos = new List<Photo>
                {
                    new Photo(slides[secondSlideIndex].Photos.FirstOrDefault().Id, Orientation.V, new List<string>(slides[secondSlideIndex].Photos.FirstOrDefault().Tags)),
                    new Photo(slides[secondSlideIndex].Photos.LastOrDefault().Id, Orientation.V, new List<string>(slides[secondSlideIndex].Photos.LastOrDefault().Tags))
                };

                Slide slideA = new Slide(slides[firstSlideIndex].Id, firstSlidePhotos);
                Slide slideB = new Slide(slides[secondSlideIndex].Id, secondSlidePhotos);

                slideA.Photos[firstSlidePhotoIndex] = slides[secondSlideIndex].Photos[secondSlidePhotoIndex];
                slideB.Photos[secondSlidePhotoIndex] = slides[firstSlideIndex].Photos[firstSlidePhotoIndex];

                slides[firstSlideIndex] = slideA;
                slides[secondSlideIndex] = slideB;
            }
            else if (swapOrChange < 7)
            {
                slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();
                Slide tempSlide = slides[slidesToSwap.FirstOrDefault()];
                slides[slidesToSwap.FirstOrDefault()] = slides[slidesToSwap.LastOrDefault()];
                slides[slidesToSwap.LastOrDefault()] = tempSlide;
            }
            else
            {
                slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();
                Slide slide = slides[slidesToSwap.FirstOrDefault()];
                slides.RemoveAt(slidesToSwap.FirstOrDefault());
                slides.Insert(slidesToSwap.LastOrDefault(), slide);
            }
        }

        public void HillClimbing(int numberOfIterations)
        {
            Random random = new Random();
            List<int> randomNumbers = new List<int>();
            for (int i = 0; i < this.Slides.Count(); i++)
            {
                randomNumbers.Add(i);
            }

            for (int i = 0; i < numberOfIterations; i++)
            {
                List<Slide> tempSolution = this.Slides;
                List<int> slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();

                Mutate(tempSolution, randomNumbers);

                int currentInterestFactor = CalculateInterestFactor(tempSolution);
                if (currentInterestFactor >= this.InterestFactor)
                {
                    this.Slides = new List<Slide>(tempSolution);
                    this.InterestFactor = currentInterestFactor;
                }
            }
        }

        public void HillClimbingWithAdditionalFeatures(int numberOfIterations)
        {
            Random random = new Random();
            List<int> randomNumbers = new List<int>();
            for (int i = 0; i < this.Slides.Count(); i++)
            {
                randomNumbers.Add(i);
            }

            for (int i = 0; i < numberOfIterations; i++)
            {
                List<Slide> tempSolution = DifferentPositionsSlides(this.Slides);
                List<int> slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();

                Mutate(tempSolution, randomNumbers);

                int currentInterestFactor = CalculateInterestFactor(tempSolution);
                if (currentInterestFactor >= this.InterestFactor)
                {
                    this.Slides = new List<Slide>(tempSolution);
                    this.InterestFactor = currentInterestFactor;
                }
            }

        }

        public int CalculateInterestFactor(List<Slide> slides)
        {
            int interestFactor = 0;
            for (int i = 0; i < slides.Count - 1; i++)
            {
                int commonTags = FindCommonTags(slides[i], slides[i + 1]);
                int slideAnotB = FindDifferenteTags(slides[i], slides[i + 1]);
                int slideBnotA = FindDifferenteTags(slides[i + 1], slides[i]);
                interestFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));
            }
            return interestFactor;
        }

        public int FindCommonTags(Slide slideA, Slide slideB)
        {
            return slideA.Tags.Where(x => slideB.Tags.Contains(x)).Count();
        }

        public int FindDifferenteTags(Slide slideA, Slide slideB)
        {
            return slideA.Tags.Where(x => !slideB.Tags.Contains(x)).Count();
        }

        public void GenerateOutputFile(string filename)
        {
            using (StreamWriter file = new StreamWriter(new FileStream(filename, FileMode.CreateNew)))
            {
                file.WriteLine(this.Slides.Count);
                foreach (Slide slide in this.Slides)
                {
                    file.WriteLine($"{string.Join(" ", slide.Photos.Select(x => x.Id).ToList())}");
                }
            }
        }

     //ILS
        public void IteratedLocalSearch(int totalIterations, long IdealSolution)
        {
            int fileToRead = 3;
            List<int> DistributionOfTime = GetRandomIterations(Convert.ToInt32(0.5 * TotalIterations)); // T
            Random random = new Random();
            Solution solution = new Solution();
            string[] files = Directory.GetFiles($"Samples", "*.txt");

            List<Slide> slides = new List<Slide>();
            Instance instance = Extensions.IO.ReadInput(files[fileToRead]);
           // GenerateSolutionWithHeuristic(instance.Photos.ToList());
           GenerateRandomSolution(instance.Photos.OrderBy(x => random.Next()).ToList());// S
           
            List <Slide> S = Slides;
            List <Slide> H = S; // H
            List <Slide> Best = S; // Best
            List <Slide> R;

            Random rnd = new Random();

            while (CalculateInterestFactor(Best) != IdealSolution && TotalIterations > 0)
            {
                int CurrentIterationsPerDistribution = DistributionOfTime[rnd.Next(DistributionOfTime.Count)];

                while (CalculateInterestFactor(S) != IdealSolution && CurrentIterationsPerDistribution > 0 && TotalIterations > 0)
                {
                     R = S;
                    Random ran = new Random();
                    List<int> randomNumbers = new List<int>();
                    for (int i = 0; i < this.Slides.Count(); i++)
                    {
                        randomNumbers.Add(i);
                    }

                    Mutate(R, randomNumbers);
                    

                    if (CalculateInterestFactor(R) > CalculateInterestFactor(S))
                        S = R;

                    CurrentIterationsPerDistribution--;
                    TotalIterations--;
                }

                if (CalculateInterestFactor(S) > CalculateInterestFactor(Best))
                {
                    Best = S;
                }

                H = NewHomeBase(H, S);
                S = Perturb(H);
            }
            Console.WriteLine("Interes factor for best Solution: " + CalculateInterestFactor(Best));
        }
        public List <Slide> NewHomeBase(List <Slide> H, List<Slide> S)
        {
            if (CalculateInterestFactor(S) >= CalculateInterestFactor(H))
                return S;
            else
                return H;
        }
        public List <Slide> Perturb(List <Slide> H)
        {
            int MutationCounter = Convert.ToInt32(PerturbationPercentage * H.Count);

            for (int i = 0; i < MutationCounter; i++)
            {
                List<int> randomNumbers = new List<int>();
                for (int ii = 0; ii < this.Slides.Count(); ii++)
                {
                    randomNumbers.Add(ii);
                }
                Mutate(H, randomNumbers);
            }

            return H;
        }
       
        public List<int> GetRandomIterations(int count)
        {
            // min 30% max 70% e totaliterations
            Random random = new Random();
            int minValueOfIteration = (int)Math.Ceiling(0.3 * TotalIterations);
            int maxValueOfIteration = (int)Math.Ceiling(0.7 * TotalIterations);

            List<int> randomIterations = new List<int>();
            for (int i = 0; i < count; i++)
            {
                randomIterations.Add(random.Next(minValueOfIteration, maxValueOfIteration));
            }

            return randomIterations;
        }
       
    }
}
