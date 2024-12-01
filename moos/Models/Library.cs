using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;


namespace moos.Models
{
    public class Library
    {
        public ObservableCollection<Song> LocalLibraryCollection { get; set; }

        public Library() 
        {
            LocalLibraryCollection = [];
        }

        public void LoadLocalCollection(string folderPath)
        {
            LocalLibraryCollection = new ObservableCollection<Song>();

            if (Directory.Exists(folderPath))
            {
                foreach (string filePath in Directory.GetFiles(folderPath, "*.mp3"))
                {
                    Console.WriteLine($"{filePath}");
                }
            }
            else 
            {
                throw new DirectoryNotFoundException($"Local folder not found at \"{folderPath}\"");
            }
        }
    }
}
