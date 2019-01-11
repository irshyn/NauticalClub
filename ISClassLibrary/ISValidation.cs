/*
 * The ISValidations class belongs to the ISClassLibrary library and contains functions
 * used during the input data validation and formatting
 * Assignment 4
 * Revision History
 *          Iryna Shynkevych 2018-11-25 : created
 */
using System.Text.RegularExpressions;

namespace ISClassLibrary
{
    // definition of the ISValidation class
    public static class ISValidations
    {
        // this function capitilizes the word passed in the string argument and returns
        // the resulting string
        public static string ISCapitalize(string line)
        {

            string[] nameSplit;
            if (line == null) return "";

            nameSplit = line.Split();
            line = "";
            foreach (string item in nameSplit)
            {
                if (item != "")
                    line += item.Substring(0, 1).ToUpper() + item.Substring(1).ToLower() + " ";
            }
            return line.Trim();
        }

        // extracts all digits contained in the string passed in argument and returns them
        public static string ISExtractDigits(string line)
        {
            string result = "";

            if (line == null) return null;

            foreach (char letter in line)
            {
                if (char.IsDigit(letter))
                {
                    result += letter;
                }
            }
            return result;
        }

        // checks if the string passed in parameters matches the pattern of canadian postal code
        // and, if so, retuns true (also returns true for empty string)
        public static bool ISPostalCodeValidation(string line)
        {
            string pattern = "^[ABCEGHJ-NPRSTVXY]{1}[0-9]{1}[ABCEGHJ-NPRSTV-Z]{1}[ ]?[0-9]{1}[ABCEGHJ-NPRSTV-Z]{1}[0-9]{1}$";
            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);

            if (line == null)
            {
                return true;
            }
            else
            {
                line = line.Trim();
                if (line == "")
                {
                    return false;
                }
                else
                {
                    return reg.IsMatch(line);
                }
            }
        }
        // formats Canadian postal code passed in parameters
        public static string ISPostalCodeFormat(string line)
        {

            if (line == null) return null;

            if (line.Length == 6)
            {
                line = line.Substring(0, 3) + " " + line.Substring(3);
            }
            return line.ToUpper();
        }
        // validates and formats zip code passed in parameters
        public static bool ISZipCodeValidation(ref string line)
        {
            string temp;

            if (line == null || line == string.Empty)
            {
                line = "";
                return true;
            }

            temp = ISExtractDigits(line);
            if (temp.Length == 5)
            {
                line = temp;
                return true;
            }

            if (temp.Length == 9)
            {
                line = temp.Substring(0, 5) + "-" + temp.Substring(5);
                return true;
            }
            return false;
        }
    }
}
