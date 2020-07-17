using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;

namespace ArmaDotNetCore
{
    public class ArmaExtension
    {
        //This tells the compiler to create an entrypoint named 'RVExtension'. This line should be added
        // to each method you want to export. Only public static are accepted.
        [UnmanagedCallersOnly(EntryPoint = "RVExtension")]
        /// <summary>
        /// This is the code that will get executed upon issuing a call to the extension from arma.
        /// </summary>
        /// <code>
        /// "dotnet_a3" callExtension "ourString";
        /// </code>
        /// <param name="output">A pointer to the memory location of a chars array that will be read after issuing callExtension command</param>
        /// <param name="outputSize">An integer that determines the maximum lenght of the array</param>
        /// <param name="function">A pointer to the string passed from arma</param>
        public unsafe static void RVExtension(char* output, int outputSize, char* function)
        {
            //Let's grab the string from the pointer passed from the Arma call to our extension
            //Note the explicit cast
            string parameter = Marshal.PtrToStringAnsi((IntPtr)function);

            //Now we have to call the other function to reverse our string
            char[] strToArr = reverse(parameter);

            string finalString = new string(strToArr) + '\0';

            /*
            Now that we have our reversed string terminated by the null character,
            we have to convert it to a byte array in order to allow the arma extension loader (Which is c/c++)
            to "decode" our string. We'll basically copy our string into the location pointed by the 'output' pointer.
            */
            byte[] byteFinalString = Encoding.ASCII.GetBytes(finalString);

            //We're done, now that we have our properly encoded byte array, we have to 'assign' its value to the
            //memory location pointed by output pointer.
            Marshal.Copy(byteFinalString,0,(IntPtr)output,byteFinalString.Length);
        }

        [UnmanagedCallersOnly(EntryPoint = "RVExtensionArgs")]
        /// <summary>
        /// This is the code that will get executed upon issuing a call to the extension from arma. Pass back to arma a string formatted like an array.
        /// </summary>
        /// <code>
        /// "dotnet_a3" callExtension ["MyFunction",["arg1",2,"arg3"]];
        /// </code>
        /// <param name="output">A pointer to the memory location of a chars array that will be read after issuing callExtension command</param>
        /// <param name="outputSize">An integer that determines the maximum lenght of the array</param>
        /// <param name="function">A pointer to the string passed from arma</param>
        /// <param name="argv">An array of pointers (char**). IE: A pointer to a memory location where pointers are stored(Still, can't be casted to IntPtr)</param>
        /// <param name="argc">Integer that points how many arguments there are in argv</param>

        public unsafe static void RVExtensionArgs(char* output, int outputSize, char* function, char** argv, int argc)
        {
            //Let's grab the string from the pointer passed from the Arma call to our extension
            //Note the explicit cast
            string mainParam = Marshal.PtrToStringAnsi((IntPtr)function);

            //Let's create a list with all the parameters inside
            List<String> parameters = new List<string>(); 
            parameters.Add(mainParam);

            //Populate our List
            for (int i=0; i<argc; i++) {
                string curStr = Marshal.PtrToStringAnsi((IntPtr)argv[i]);
                parameters.Add(curStr);
            }
            
            //Craft an arma array
            string armaResult = ListToArma(parameters) + '\0';

            /*
            Now that we have our reversed string terminated by the null character,
            we have to convert it to a byte array in order to allow the arma extension loader (Which is c/c++)
            to "decode" our string. We'll basically copy our string into the location pointed by the 'output' pointer.
            */
            byte[] byteFinalString = Encoding.ASCII.GetBytes(armaResult);

            //We're done, now that we have our properly encoded byte array, we have to 'assign' its value to the
            //memory location pointed by output pointer.
            Marshal.Copy(byteFinalString,0,(IntPtr)output,byteFinalString.Length);
        }

        [UnmanagedCallersOnly(EntryPoint = "RVExtensionVersion")]
        /// <summary>
        /// This is the code that will get executed once the extension gets loaded from arma.
        /// The output will get printed in RPT logs
        /// </summary>
        /// <param name="output">A pointer to the memory location of a chars array that will be read after the load of the extension.</param>
        /// <param name="outputSize">An integer that determines the maximum lenght of the array</param>
 
        public unsafe static void RVExtensionVersion(char* output, int outputSize)
        {
            string greetingsString = "|Arma .NET Core Sample|";
            
            string finalString = greetingsString + '\0';

            byte[] byteFinalString = Encoding.ASCII.GetBytes(finalString);
            
            Marshal.Copy(byteFinalString,0,(IntPtr)output,byteFinalString.Length);
        }

        /// <summary>
        /// This is a function to reverse a provided string and returns a chars array
        /// </summary>
        /// <param name="parameter">A generic string to be reversed.</param>
        /// <returns>A chars array</returns>
        public static char[] reverse(string parameter) {

            // Convert the string to char array
            char[] arr = parameter.ToCharArray();
            
            //Reverse the array
            Array.Reverse( arr );

            //Return reversed array
            return arr;
        }

        public static string ListToArma(List<String> list)
        {
            {
                //Multiple elements
                var returnString = "";
                foreach (var str in list)
                {
                    if (String.IsNullOrEmpty(returnString))
                    {
                        returnString += "[" + str + ",";
                    }
                    else { returnString += str + ","; }
                }
                returnString = returnString.Remove(returnString.Length - 1);
                returnString += "]";
                return returnString;
            }
        }
    }
}
