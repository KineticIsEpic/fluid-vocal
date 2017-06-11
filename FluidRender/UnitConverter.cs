/*====================================================*\
 *||          Copyright(c) KineticIsEpic.             ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;

namespace FluidSys {
    public class UnitConverter {
        public const string Base64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        public static int Decode(string input, string alphabet) {
            int value = 0;
            for (int i = 0; i < input.Length; i++) {
                int index = alphabet.IndexOf(input[i]);
                value += index * (int)Math.Pow(alphabet.Length, input.Length - i - 1);
            }
            return value;
        }

        public static string Encode(int value, string alphabet) {
            string encoded = "";
            while (value > 0) {
                int index = value % alphabet.Length;
                encoded = alphabet[index] + encoded;
                value = (int)Math.Floor((double)value / alphabet.Length);
            }
            return encoded;
        }
    }
}
