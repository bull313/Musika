using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Policy;
using Musika;
using Musika.WAV;

namespace MusikaDriver
{
    /* Driver class - compiles a Musika file to a notesheet and a notesheet to a wav file */
    class Driver
    {
        /* CONSTANTS */

        /* Error Messages */
        public const string BAD_ARGUMENT_MSG            = "An invalid argument was passed in";
        public const string NO_ARGS_MSG                 = "Please enter a filename to compile (with extension: " + Compiler.MUSIKA_FILE_EXT + " to build note sheet and " + Serializer.SERIALIZE_EXT + " to build a " + WAVFile.WAV_FILE_EXT + " file from a notesheet)";
        public const string WRONG_EXT_MSG               = "Compilation error: please input a file with a " + Compiler.MUSIKA_FILE_EXT + " or " + Serializer.SERIALIZE_EXT + " extension.";

        /* File extension to compiler action map */
        public readonly static Dictionary<string, CompilerAction> EXT_TO_ACTION_MAP = new Dictionary<string, CompilerAction>()
        {
            { Compiler.MUSIKA_FILE_EXT, new CodeToNoteSheet() },
            { Serializer.SERIALIZE_EXT, new NoteSheetToWAV() }
        };

        /* Flags */
        public const string DOUBLE_COMPILE_FLAG = "-d"; /* If set, convert a Musika text file straight to wav */

        public readonly static Dictionary<string, CompilerAction> FLAGS = new Dictionary<string, CompilerAction>() /* Set of available flags and their corresponding compiler action */
        {
            { DOUBLE_COMPILE_FLAG, new NoteSheetToWAV() }
        };

        /* / CONSTANTS */

        /* PRIVATE FUNCTIONS */

        private static CompilerAction ExtractCompilerAction(string ext) /* Get a compiler action based on the given file extension */
        {
            /* Local Variables */
            CompilerAction action; /* Compiler action to execute */
            /* / Local Variables */

            /* Assume the action to be nothing initially */
            action = null;

            if (EXT_TO_ACTION_MAP.ContainsKey(ext))
            {
                action = EXT_TO_ACTION_MAP[ext];
            }

            /* Return Result */
            return action;
        }

        private static CompilerAction ParseArguments(string[] args, out string filename) /* Extract a file name and compiler action from arguments */
        {
            /* Local Variables */
            CompilerAction action;                  /* Compiler action to execute   */
            List<CompilerAction> additionalActions; /* Additional compiler actions  */
            bool filenameReceived;                  /* Received a file name         */
            int additionalActionIdx;                /* additional action iterator   */
            string ext;                             /* File extension of given file */
            /* / Local Variables */

            /* Assume the action and filename to be empty initiallly */
            action = null;
            filename = string.Empty;

            switch (args.Length)
            {
                case 0:
                    /* Not enough arguments */
                    Console.WriteLine(NO_ARGS_MSG);
                    break;

                case 1:
                    /* Filename with no arguments */
                    ext         = Path.GetExtension(args[0]);   /* Get the file extension                                       */
                    filename    = Path.GetFileName(args[0]);    /* and file name from the argument                              */
                    action      = ExtractCompilerAction(ext);   /* And use the extension to determine a proper compiler action  */

                    /* Action is null if the extension was not recognized */
                    if (action == null)
                    {
                        Console.WriteLine(WRONG_EXT_MSG);
                    }

                    break;

                default:
                    /* Filename with arguments */
                    filenameReceived    = false;
                    additionalActions   = new List<CompilerAction>();

                    /* Interpret each command line argument */
                    foreach (string arg in args)
                    {
                        /* If this argument is a flag, get its corresponding action */
                        if (FLAGS.ContainsKey(arg))
                        {
                            additionalActions.Add(FLAGS[arg]);
                        }

                        /* If this argument is a file name (and we haven't already seen one), get its corresponding action based on its extension */
                        else if (!filenameReceived)
                        {
                            ext                 = Path.GetExtension(arg);
                            filename            = Path.GetFileName(arg);
                            filenameReceived    = true;
                            action              = ExtractCompilerAction(ext);
                        }

                        /* Unrecognized argument; cancel operation */
                        else
                        {
                            Console.WriteLine(BAD_ARGUMENT_MSG);
                            action      = null;
                            filename    = string.Empty;
                            break;
                        }
                    }

                    /* Wire additional compiler actions together */
                    if (action != null && additionalActions.Count > 0)
                    {
                        action.SetAdditionalAction(additionalActions[0]);

                        for (additionalActionIdx = 0; additionalActionIdx < additionalActions.Count - 1; ++additionalActionIdx)
                        {
                            additionalActions[additionalActionIdx].SetAdditionalAction(additionalActions[additionalActionIdx + 1]);
                        }
                    }
                    break;
            }

            /* Return Result */
            return action;
        }

        /* / PRIVATE FUNCTIONS */

        /* DRIVER FUNCTION */
        static void Main(string[] args)
        {
            /* Local Variables */
            CompilerAction action;  /* Specific action for compiler object to take                  */
            Compiler compiler;      /* Object used to convert one music representation to another   */
            /* / Local Variables */

            try
            {
                /* Extract an action and file name from the command line arguments */
                action = ParseArguments(args, out string filename);

                if (action != null)
                {
                    /* Create a compiler object and perform the desired action with it */
                    compiler = new Compiler(Directory.GetCurrentDirectory(), filename);
                    action.PerformAction(compiler);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /* / DRIVER FUNCTION */
    }

    /* Compiler action abstraction */
    abstract class CompilerAction
    {
        /* PROPERTIES */
        CompilerAction additionalAction;
        /* / PROPERTIES */

        public void SetAdditionalAction(CompilerAction compilerAction) /* Set a pointer to an additional compiler action */
        {
            additionalAction = compilerAction;
        }

        public virtual void PerformAction(Compiler compiler) /* Action to perform */
        {
            /* Run the additional action using the same compiler object */
            if (additionalAction != null)
            {
                additionalAction.PerformAction(compiler);
            }
        }
    }

    /* Convert Musika code to a note sheet binary */
    class CodeToNoteSheet : CompilerAction
    {
        public override void PerformAction(Compiler compiler) /* Action to perform */
        {
            compiler.CompileToNoteSheet();
            base.PerformAction(compiler);
        }
    }

    /* Convert a note sheet binary to an audible WAV file */
    class NoteSheetToWAV : CompilerAction
    {
        public override void PerformAction(Compiler compiler) /* Action to perform */
        {
            compiler.CompileToWAV();
            base.PerformAction(compiler);
        }
    }
}
