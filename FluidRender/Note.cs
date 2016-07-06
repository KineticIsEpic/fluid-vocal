/*====================================================*\
 *||          Copyright(c) KineticIsEpic.             ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OTOmate;

namespace FluidSys {
    /// <summary>
    /// Represents a single note to be rendered.
    /// </summary>
    public class Note {
        /// <summary>
        /// Gets or sets the sample pack location used to render this note.e
        /// This property is ignored of UseDefaultVb is set to true.
        /// </summary>
        public string VbPath { get; set; }
        /// <summary>
        /// Gets or sets the user-friendly name for this note.
        /// </summary>
        public string DispName { get; set; }
        /// <summary>
        /// Gets or sets the name of the sample used for rendering, path excluded.
        /// Concatenated with VbPath to create a full path (use FullPath for convenience).
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Gets a full file path for the sample to render.
        /// </summary>
        public string FullPath { get { return VbPath + "\\" + FileName; } }
        /// <summary>
        /// Gets or sets the pitch of the note, represented e.g. C#4.
        /// </summary>
        public string NotePitch { get; set; }
        /// <summary>
        /// Gets or set addtional arguments used at render time.
        /// </summary>
        public string Args { get; set; }
        /// <summary>
        /// Gets or sets the Velocity property for this FluidSys.Note.
        /// </summary>
        public int Velocity { get; set; }
        /// <summary>
        /// Gets or sets the Length property for this FluidSys.Note.
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// Gets or sets the location of this note in the sheet, 
        /// relative to the positioning of this and the last note.
        /// </summary>
        public int Location { get; set; }
        /// <summary>
        /// Gets or sets the modulation property of this FluidSys.Note.
        /// </summary>
        public int Modulation { get; set; }
        /// <summary>
        /// Gets or sets the overlap property of this FluidSys.Note.
        /// </summary>
        public int Overlap { get; set; }
        /// <summary>
        /// Gets or sets the volume property of this FluidSys.Note.
        /// </summary>
        public int Volume { get; set; }
        /// <summary>
        /// Set to true to use the sample pack path indicated by the 
        /// containing FluidSys.Sheet class at render time.
        /// </summary>
        public bool UseDefaultVb { get; set; }
        /// <summary>
        /// Gets or sets the pitch code associated with this FluidSys.Note.
        /// </summary>
        public string PitchCode { get; set; }
        /// <summary>
        /// Gets the voice properties of this FluidSys.Note.
        /// </summary>
        public VoiceProp VoiceProperties { get; set; }
        /// <summary>
        /// The length of this FluidSys.Note in Utau Units. 
        /// </summary>
        public int UUnitLength { get; set; }
        /// <summary>
        /// Gets or sets the envelope configuration of this FluidSys.Note.
        /// </summary>
        public List<int[]> Envelope = new List<int[]> { new int[2], new int[2], new int[2], new int[2] };
        
        /// <summary>
        /// Create a new instance of the FluidSys.Note class using the
        /// specified VoiceProp.
        /// </summary>
        public Note(VoiceProp vProp) {
            UseDefaultVb = true;
            VoiceProperties = vProp;
            Location = 0;
            Length = 1024;
            Velocity = 100;
            Volume = 100;
            Modulation = 0;
            Args = "B0";
        }

        /// <summary>
        /// Creates a new instance of the FLuidSys.Note class.
        /// </summary>
        public Note() {
            UseDefaultVb = true;
            VoiceProperties = new VoiceProp();
            Location = 0;
            Length = 1024;
            Velocity = 100;
            Volume = 100;
            Modulation = 0;
            Args = "B0";
        }

        public void GenerateDefaultEnvelope() {
            Envelope[0][0] = 0;
            Envelope[0][1] = 0;
            Envelope[1][0] = 60;
            Envelope[1][1] = 100;
            Envelope[2][0] = Length - 60;
            Envelope[2][1] = 100;
            Envelope[3][0] = Length;
            Envelope[3][1] = 0;
        }
    }
}
