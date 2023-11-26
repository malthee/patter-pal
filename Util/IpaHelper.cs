using System.Text.RegularExpressions;

namespace patter_pal.Util
{
    /// <summary>
    /// Sometimes the chat answer contains IPA characters, we can pronounce them differently in ssml. 
    /// This class helps us find IPA characters in a string.
    /// </summary>
    public class IpaHelper
    {
        // Constant for IPA characters (Unicode range, common characters)
        private const string IpaCharacters = "ˈˌɑ-ɔɛ-ɪʌʊæœɐɜɞɘɵʉɨʉɯɪəɚɤɝɫɹɻʀʁʂʃʈʧʊʋβθðʒʔʕʢʡɕɧɱɳɲŋɴʎɭɹ̠˔ɻ˞ʍɥɡɢʡʔɸʋɹɾɽɮɺɭʎʟɥʜʢʡɕɧɬɮɺɭʎʟɰʃʒɕʑʂʐʝʎʟɽɱɳɲŋɴˈˌ";
        // Any Ipa in /slash/     
        private const string IpaRegex = $@"/[^/]*[{IpaCharacters}][^/]*/";

        /// <summary>
        /// Allows processing IPA matches in a string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="processor"></param>
        /// <returns></returns>
        public static string ProcessIpa(string input, Func<string, string> processor)
        {
            return Regex.Replace(input, IpaRegex, match => processor(match.Value.Trim('/')));
        }

    }
}
