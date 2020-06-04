using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ArmaDotNetCore
{
    public class ArmaExtension
    {
        //This tells the compiler to create an entrypoint named 'RVExtension'. This line should be added
        // to each method you want to export. Only public static are accepted.
        [UnmanagedCallersOnly(EntryPoint = "RVExtension")]

        //The callExtension command passes 3 parameters:
        /*

        output: Pointer to the string that will be returned to the sqf end
        outputSize : Maximum amount of bytes allowed in the string pointed by output,make sure not to exceed this limit
        function : Pointer to the string specified to the right of the callExtension command

        */    
        public static void RVExtension(IntPtr output, int outputSize, IntPtr function)
        {
            //Let's grab the string from the pointer passed from the Arma call to our extension
            string parameter = Marshal.PtrToStringAnsi(function);

            //Now we have to call the other function to reverse our string
            char[] strToArr = reverse(parameter);

            string finalString = new string(strToArr) + char.MinValue;

            /*
            Now that we have our reversed string terminated by the null character,
            we have to convert it to a byte array in order to allow the arma extension loader (Which is c/c++)
            to "decode" our string. We'll basically copy our string into the location pointed by the 'output' pointer.
            */
            byte[] byteFinalString = Encoding.ASCII.GetBytes(finalString);

            //We're done, now that we have our properly encoded byte array, we have to 'assign' its value to the
            //memory location pointed by output pointer. Aaand it's gone.
            Marshal.Copy(byteFinalString,0,output,byteFinalString.Length);
        }


        [UnmanagedCallersOnly(EntryPoint = "RVExtensionVersion")]
        public static void RVExtensionVersion(IntPtr output, int outputSize)
        {
            string greetingsString = "|Arma .NET Core Sample|";

            
            string finalString = greetingsString + char.MinValue;

            byte[] byteFinalString = Encoding.ASCII.GetBytes(finalString);
            
            Marshal.Copy(byteFinalString,0,output,byteFinalString.Length);
        }


        public static char[] reverse(string parameter) {

            // Convert the string to char array
            char[] arr = parameter.ToCharArray();
            
            //Reverse the array
            Array.Reverse( arr );

            //Return reversed array
            return arr;
        }
    }
}
