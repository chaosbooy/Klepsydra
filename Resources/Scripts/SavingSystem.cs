using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klepsydra.Resources.Scripts
{
    public static class SavingSystem
    {
        public static Data savedData { get; private set; }
        public static string filePath { get; private set; }

        static SavingSystem()
        {
            savedData = new Data();
            filePath = "Klepsydra.json";
        }

        public static void Initialize(string path)
        {
            // Load the saved data from a file or database
            // For example, you can use JSON serialization to save and load the data
            // savedData = LoadDataFromFile("savedData.json");

            filePath = path;

            Load();
        }

        public static void Initialize()
        {
            Load();
        }

        public static void AddTimer(string name, int time)
        {
            if (savedData.timers.ContainsKey(name))
            {
                savedData.timers[name] = time;
            }
            else
            {
                savedData.timers.Add(name, time);
            }

            Save();
        }

        private static void Load()
        {

        }

        private static void Save()
        {
            
        }
    }
}
