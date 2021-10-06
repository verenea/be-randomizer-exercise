using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;

namespace RandomizerExercise {

    class Program {

        static readonly int min = 1;
        static readonly int max = 10000;
        static Random rnd = new Random();

        static void Main(string[] args) {
            Console.WriteLine($"About to write {max} lines of numbers...");
            Console.WriteLine(Environment.NewLine + "******************************************" + Environment.NewLine);

            // we'll compare the start and end time later...
            DateTime startTime = DateTime.Now;


            // ===============================================
            // 1. RANDOMIZER ALGORITHM STARTS HERE
            // ===============================================

            // stuff a collection from min -> max
            var numbers = Enumerable.Range(min, max).ToArray();

            // version #1 with FY
            var shuffled = Shuffle(numbers);

            // version #2 with linq
            //var shuffled = numbers.OrderBy(x => rnd.Next(min, max));

            // == END ALGORITH ================================


            // ===============================================
            // 2. DISPLAY RESULTS TO CONSOLE
            // ===============================================

            // print numbers to console window for viewing
            shuffled.ToList().ForEach(x => Console.WriteLine(x));

            Console.WriteLine(Environment.NewLine + "******************************************" + Environment.NewLine);

            Console.WriteLine($"Randomization time took: {(DateTime.Now - startTime).TotalSeconds:0.0} seconds" + Environment.NewLine);

            // ===============================================
            // 3. VALIDATION
            // ===============================================

            bool isValid = AreResultsValid(shuffled);

            Console.WriteLine(isValid ? "Results Validated!" : $"RESULTS ARE INVALID, check the code (count: {shuffled.Count()})");

            // ===============================================
            // 4. FILE OUTPUT
            // ===============================================

            if (isValid && getBoolConfig("EnableFileOutput")) {
                try {
                    GenerateResultsFile(shuffled);
                } catch (Exception ex) {
                    Console.WriteLine($"Unable to generate results file: {ex.Message}");
                }
            }

            // ===============================================
            // WE DONE
            // ===============================================

            Console.WriteLine(Environment.NewLine + $"Total processing time: {(DateTime.Now - startTime).TotalSeconds:0.0} seconds");

            // keep console open
            Console.ReadLine();
        }

        static T[] Shuffle<T>(T[] array) {
            int n = array.Length;
            for (int i = 0; i < (n - 1); i++) {
                int r = i + rnd.Next(n - i);
                T t = array[r];
                array[r] = array[i];
                array[i] = t;
                //Console.WriteLine(t);
            }
            return array;
        }

        /// <summary>
        /// Checks result set to confirm total ct equals max, and we don't have any duplicates
        /// </summary>
        static bool AreResultsValid(IEnumerable<int> results) {
            return 
                // make sure we got the desired length
                results.Count() == max
                &&
                // make sure there are no dups
                !results.GroupBy(value => value).Any(@group => @group.Count() > 1)
            ;
        }

        /// <summary>
        /// Renders and saves a .TXT file with a list of numbers
        /// </summary>
        static void GenerateResultsFile(IEnumerable<int> numbers) {

            // convert the results to strings for file
            List<string> lines = numbers.ToList().ConvertAll<string>(x => x.ToString());

            // identify file prefix
            string fileNamePre = getConfigValue("OutputFilePrefix") ?? "results";

            // declare suffix placeholder
            string fileVersion = null;

            // if we want to save subsequent files...
            if (!getBoolConfig("OverwriteLastFile")) {

                // see if prev TXT files with target prefix exist in this directory
                FileInfo lastFile = new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*.txt")
                    //.Where(x => x.Name.StartsWith(fileNamePre))
                    .OrderByDescending(x => x.CreationTime)
                    .FirstOrDefault()
                ;

                // see if we can 'version' number from existing file for incrementing
                if (lastFile != null) {
                    string name = lastFile.Name;
                    string parsedNum = name.Substring(fileNamePre.Length, name.IndexOf(".") - fileNamePre.Length);
                    int lastVer = 1; // <-- default to showing last version
                    if (!string.IsNullOrEmpty(parsedNum)) {
                        // see if prev file was "versioned", 
                        int.TryParse(parsedNum, out lastVer);
                    }
                    // since last file exists, even if it was the first, then this is the second
                    fileVersion = (lastVer + 1).ToString();
                }
            }

            // unique filename
            string fileName = $"{fileNamePre}{fileVersion}.txt";

            // save the file
            File.WriteAllLines(fileName, lines);

            Console.WriteLine($"File generated: {fileName}");
        }

        static string getConfigValue(string key) {
            return ConfigurationSettings.AppSettings[key];
        }

        static bool getBoolConfig(string key) {
            string value = ConfigurationSettings.AppSettings[key];
            if (!string.IsNullOrEmpty(value)) {
                return value.ToLower() == "true";
            }
            return false;
        }
    }
}
