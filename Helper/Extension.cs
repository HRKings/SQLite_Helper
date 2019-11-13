using System;

namespace SQLite_Helper.Helper
{
    public static class Extensions
    {
        /// <summary>
        /// Joins an array of pairs of (string, dynamic) into a single (string, string) pair containing all values in string form
        /// </summary>
        /// <param name="pairs">The array which contains the pairs</param>
        /// <returns>A single (string, string) pair where the Item1 is all the Item1 of the pairs in the array seperated by comma and the Item2 is all of the pairs Item2 in string form separeted by comma</returns>
        public static (string, string) PairsToString(this (string, dynamic)[] pairs)
        {
            (string, string) result = ("", "");

            string[] items1 = new string[pairs.Length];
            string[] items2 = new string[pairs.Length];

            for (int i = 0; i < pairs.Length; i++)
            {
                items1[i] = pairs[i].Item1;
                // Only adds an Item2 if it isn't null
                items2[i] = pairs[i].Item2 != null ? pairs[i].Item2.ToString() : null;
            }

            result = (String.Join(",", items1), String.Join(",", items2));

            //The result is ("Item1, Item1, Item1, Item 1", "Item2", "Item2", "Item2", "Item2
            return result;
        }
    }
}
