using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

namespace NameChangeSimulator.Shared.Utils
{
    public class ConvertPDF
    {
        private static byte[] StringToAnsiZ(string str)
        {
            int intCounter;
            var intElementCount = str.Length;
            var aAnsi = new byte[intElementCount + 1];
            for (intCounter = 0; intCounter < intElementCount; intCounter++)
            {
                var bChar = (byte)str[intCounter];
                aAnsi[intCounter] = bChar;
            }

            aAnsi[intElementCount] = 0;
            return aAnsi;
        }

        public void Convert(string inputFile, string outputFile,
            int firstPage, int lastPage, string deviceFormat, int width, int height)
        {
            if (!File.Exists(inputFile))
                return;

            int intCounter;
            var sArgs = GetGeneratedArgs(inputFile, outputFile,
                firstPage, lastPage, deviceFormat, width, height);

            var intElementCount = sArgs.Length;
            var aAnsiArgs = new object[intElementCount];
            var aPtrArgs = new IntPtr[intElementCount];
            var aGCHandle = new GCHandle[intElementCount];

            for (intCounter = 0; intCounter < intElementCount; intCounter++)
            {
                aAnsiArgs[intCounter] = StringToAnsiZ(sArgs[intCounter]);
                aGCHandle[intCounter] = GCHandle.Alloc(aAnsiArgs[intCounter], GCHandleType.Pinned);
                aPtrArgs[intCounter] = aGCHandle[intCounter].AddrOfPinnedObject();
            }

            var gchandleArgs = GCHandle.Alloc(aPtrArgs, GCHandleType.Pinned);
            var intptrArgs = gchandleArgs.AddrOfPinnedObject();
            var intReturn = gsapi_new_instance(out var intGSInstanceHandle, _objHandle);
            try
            {
                intReturn = gsapi_init_with_args(intGSInstanceHandle, intElementCount, intptrArgs);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                for (intCounter = 0; intCounter < intReturn; intCounter++)
                    aGCHandle[intCounter].Free();

                gchandleArgs.Free();
                gsapi_exit(intGSInstanceHandle);
                gsapi_delete_instance(intGSInstanceHandle);
            }
        }

        private string[] GetGeneratedArgs(string inputFile, string outputFile,
            int firstPage, int lastPage, string deviceFormat, int width, int height)
        {
            OutputFormat = deviceFormat;
            ResolutionX = width;
            ResolutionY = height;

            var lstExtraArgs = new ArrayList();
            if (OutputFormat == "jpg" && JPEGQuality > 0 && JPEGQuality < 101)
                lstExtraArgs.Add("-dJPEGQ=" + JPEGQuality);
            if (Width > 0 && Height > 0)
                lstExtraArgs.Add("-g" + Width + "x" + Height);
            if (FitPage)
                lstExtraArgs.Add("-dPDFFitPage");
            if (ResolutionX > 0)
            {
                if (ResolutionY > 0)
                    lstExtraArgs.Add("-r" + ResolutionX + "x" + ResolutionY);
                else
                    lstExtraArgs.Add("-r" + ResolutionX);
            }

            var iFixedCount = 17;
            var iExtraArgsCount = lstExtraArgs.Count;
            var args = new string[iFixedCount + lstExtraArgs.Count];

            args[0] = "pdf2img";
            args[1] = "-dNOPAUSE";
            args[2] = "-dBATCH";
            args[3] = "-dPARANOIDSAFER";
            args[4] = "-sDEVICE=" + OutputFormat;
            args[5] = "-q";
            args[6] = "-dQUIET";
            args[7] = "-dNOPROMPT";
            args[8] = "-dMaxBitmap=500000000";
            args[9] = $"-dFirstPage={firstPage}";
            args[10] = $"-dLastPage={lastPage}";
            args[11] = "-dAlignToPixels=0";
            args[12] = "-dGridFitTT=0";
            args[13] = "-dTextAlphaBits=4";
            args[14] = "-dGraphicsAlphaBits=4";

            for (var i = 0; i < iExtraArgsCount; i++)
                args[15 + i] = (string)lstExtraArgs[i];

            args[15 + iExtraArgsCount] = $"-sOutputFile={outputFile}";
            args[16 + iExtraArgsCount] = $"{inputFile}";
            return args;
        }


        #region GhostScript Import

        [DllImport("gsdll64.dll", EntryPoint = "gsapi_new_instance")]
        private static extern int gsapi_new_instance(out IntPtr pinstance, IntPtr caller_handle);

        [DllImport("gsdll64.dll", EntryPoint = "gsapi_init_with_args")]
        private static extern int gsapi_init_with_args(IntPtr instance, int argc, IntPtr argv);

        [DllImport("gsdll64.dll", EntryPoint = "gsapi_exit")]
        private static extern int gsapi_exit(IntPtr instance);

        [DllImport("gsdll64.dll", EntryPoint = "gsapi_delete_instance")]
        private static extern void gsapi_delete_instance(IntPtr instance);

        #endregion

        #region Variables

        private readonly IntPtr _objHandle;

        #endregion

        #region Proprieties

        private string OutputFormat { get; set; }

        private int Width { get; set; }

        private int Height { get; set; }

        private int ResolutionX { get; set; }

        private int ResolutionY { get; set; }

        private bool FitPage { get; set; }

        private int JPEGQuality { get; set; }

        #endregion

        #region Init

        public ConvertPDF(IntPtr objHandle)
        {
            _objHandle = objHandle;
        }

        public ConvertPDF()
        {
            _objHandle = IntPtr.Zero;
        }

        #endregion
    }
}