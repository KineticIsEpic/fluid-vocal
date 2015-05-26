/*====================================================*\
 *|| Copyright(c) KineticIsEpic. All Rights Reserved. ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluidSys;
using OTOmate;

namespace FluidCmd {
    class CmdSys {
        string projString = "New Project";

        bool vbset = false;
        bool rndset = false;
        bool showRender = false;

        Sheet noteSheet = new Sheet();
        OtoReader or = new OtoReader();

        public void Cmd() {
            Console.Out.WriteLine();
            Console.Out.Write(projString);
            Console.Out.Write(">");

            string cmdtext = Console.In.ReadLine();
            string command = getCmdName(cmdtext);
            string args = getCmdArgs(cmdtext);

            if (command == "echo") {
                Echo(args);
                Cmd();
            }
            else if (command == "kineticisepic") {
                PrinteEpicText();
                Cmd();
            }
            else if (command == "print") {
                PrintNotes();
                Cmd();                
            }
            else if (command == "renderconsole") {
                ShowRenderConsole(args);
                Cmd();
            }
            else if (command == "del") {
                Delete(args);
                Cmd();
            }
            else if (command == "delzero") {
                DelZero(args);
                Cmd();
            }
            else if (command == "play") {
                Playback(args);
                Cmd();
            }
            else if (command == "playsmp") {
                PlaySamp(args);
                Cmd();
            }
            else if (command == "setvb") {
                SetVB(args);
                Cmd();
            }
            else if (command == "setrender") {
                SetRenderer(args);
                Cmd();
            }
            else if (command == "add") {
                Add(args);
                Cmd();
            }
            else if (command == "clrcache") {
                ClearCache();
                Cmd();
            }
            else if (command == "new") {
                MakeNewProj();
                Cmd();
            }
            else if (command == "rename") {
                projString = args;
                Cmd();
            }
            else if (command == "clear") {
                Console.Clear();
                Cmd();
            }
            else if (command == "exit") {
                ClearCache();
                return;
            }
            else {
                if (command != "")
                    Console.Out.WriteLine("e: Invalid command or parameter \"" + command + "\".");
                Cmd();
            }
        }

        public void PrinteEpicText() {
            System.IO.StreamReader sr = new System.IO.StreamReader("KINETICISEPIC.TXT");
            string epicText = sr.ReadToEnd();
            sr.Close();

            Console.Out.WriteLine(epicText);
        }

        public void Delete(string args) {
            try { noteSheet.notes.RemoveAt(int.Parse(args) - 1); }
            catch (Exception ex) { Console.Out.WriteLine("e: " + ex.Message); }
        }

        public void DelZero(string args) {
            try { noteSheet.notes.RemoveAt(int.Parse(args)); }
            catch (Exception ex) { Console.Out.WriteLine("e: " + ex.Message); }
        }

        public void ShowRenderConsole(string args) {
            if (args == "true") showRender = true;
            else if (args == "1") showRender = true;
            else if (args == "false") showRender = false;
            else if (args == "0") showRender = false;

            else {
                Console.Out.WriteLine("e: Invalid arguments.");
                Console.Out.WriteLine("Usage:");
                Console.Out.WriteLine("renderconsole <bool>");
                Console.Out.WriteLine();
                Console.Out.WriteLine("<bool> = a value indicating if the renderer's console window " +
                    "should be displayed.");
            }
        }

        public void MakeNewProj() {
            Console.Out.WriteLine("WARNING: This will erase ALL data associated with the current project.");
            Console.Out.WriteLine("Continue? <y/n>: ");
            if (Console.In.ReadLine() == "y") {

                noteSheet = new Sheet();
                or = new OtoReader();
                projString = "New Project";
                vbset = rndset = false;

                Console.Out.WriteLine("Done.");
            }
        }

        public void ClearCache() {
            try {
                System.IO.Directory.Delete(Environment.ExpandEnvironmentVariables("%LocalAppData%") +
                    "\\FluidSynth\\RenCache", true);

                Console.Out.WriteLine("Render cache cleared.");
            }
            catch (Exception ex) { Console.Out.WriteLine("e: " + ex.Message); }
        }

        public void PrintNotes() {
            int noteNumber = 0;

            foreach (var note in noteSheet.notes) {
                noteNumber++;
                Console.Out.WriteLine("Note " + noteNumber.ToString() + ":");
                Console.Out.WriteLine("  Phonetic: " + note.DispName);
                Console.Out.WriteLine("  Length: " + note.Length.ToString());
                Console.Out.WriteLine("  Pitch: " + note.NotePitch);
                Console.Out.WriteLine();
            }
        }

        public void SetRenderer(string args) {
            if (System.IO.File.Exists(args)) {
                noteSheet.Resampler = args;
                Console.Out.WriteLine("Set renderer to \"" + args + "\"");
                rndset = true;
            }
            else Console.Out.WriteLine("e: The file \"" + args + "\" does not exist.");
        }

        public void Playback(string args) {
            wavmod.WavMod wvmd = new wavmod.WavMod();
            Renderer rnd = new Renderer(noteSheet);

            if (rndset && vbset) {
                // Show render console if specified
                if (showRender) rnd.ShowRenderWindow = true;

                // Begin render
                Console.Out.Write("Rendering... ");
                rnd.Render();

                Console.Out.WriteLine("Done. ");

                // Play back render
                try { wvmd.PlaybackTemp(rnd.TemporaryDir, noteSheet); }
                catch (Exception) { Console.Out.WriteLine("Playback failed. "); return; }
            }
            else {
                Console.Out.WriteLine("e: Sample bank and/or renderer not specified.");
            }
        }

        public void PlaySamp(string args) {
            // Play back raw sample
            try { new System.Media.SoundPlayer(noteSheet.Voicebank + "\\" + or.GetVoicePropFromSampleName(args).FileName).Play(); }
            catch (Exception ex) { Console.Out.WriteLine("e: " + ex.Message); }
        }

        public void SetVB(string args) {
            Console.Out.Write("Reading samples... ");
            try {
                noteSheet.Voicebank = args;
                or.OpenFile(args + "\\oto.ini");
                Console.Out.WriteLine("Done. ");
                vbset = true;
            }
            catch (Exception ex) {
                Console.Out.WriteLine();
                Console.Out.WriteLine("e: " + ex.Message);
            }
        }

        public void Rename(string args) {
            projString = args;
        }

        public void Add(string args) {
            // String array for agruments
            string[] arglist = new string[4];

            int argCount = 0;
            int lastIndex = 0;
            int index = 0;

            // Seperate arguments
            foreach (var item in args) {
                index++;
                if (item == ',') {
                    arglist[argCount] = args.Substring(lastIndex, index - lastIndex - 1);
                    argCount++;
                    lastIndex = index;
                }
            }

            // Get last argument
            arglist[argCount] = args.Substring(lastIndex);

            // Increase argCount to compensate for 0-based indexing
            argCount++;

            // Add note if there is the correct amount of args
            if (argCount >= 3) {
                Note n = new Note();
                try {
                    // Set note properties
                    n.VoiceProperties = or.GetVoicePropFromSampleName(arglist[0]);
                    n.DispName = arglist[0];
                    n.Length = int.Parse(arglist[1]);
                    n.NotePitch = arglist[2];
                }
                catch (Exception ex) {
                    Console.Out.WriteLine("e: " + ex.Message);
                    return;
                }

                // If index is specified, add note there
                if (argCount == 4) {
                    try { noteSheet.notes.Insert(int.Parse(arglist[3]), n); }
                    catch (Exception ex) {
                        Console.Out.WriteLine("w: " + ex.Message);
                        Console.Out.WriteLine("Note will be added to end of project instead.");
                        noteSheet.notes.Add(n);
                    }
                }
                else noteSheet.notes.Add(n);

                Console.Out.WriteLine("Note added to project.");
            }
            else {
                Console.Out.WriteLine("e: Invalid arguments. " + args);
                Console.Out.WriteLine("Usage:");
                Console.Out.WriteLine("add <phonetic>,<length>,<pitch>,<index>");
                Console.Out.WriteLine();
                Console.Out.WriteLine("<phonetic> = phonetic for the note e.g. da");
                Console.Out.WriteLine("<length> = length of note in miliseconds e.g. 1000");
                Console.Out.WriteLine("<pitch> = pitch of the note e.g C#4");
                Console.Out.WriteLine("<index> = index to insert note, leave blank to add to end");
            }
        }

        public void Echo(string args) {
            Console.Out.WriteLine(args);
            Cmd();
        }

        private string getCmdName(string input) {
            if (input.IndexOf(" ") != -1) return input.Substring(0, input.IndexOf(" "));
            else return input;
        }

        private string getCmdArgs(string input) {
            string str = getCmdName(input);
            try { return input.Replace(str, "").Trim(); }
            catch (Exception) { return ""; }
        }
    }
}
