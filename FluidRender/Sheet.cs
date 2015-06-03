/*====================================================*\
 *|| Copyright(c) KineticIsEpic. All Rights Reserved. ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidSys {
    /// <summary>
    /// A sequence of notes to be rendered.
    /// </summary>
    public class Sheet {
        /// <summary>
        /// The path to the sample bank used within this FluidSys.Sheet.
        /// </summary>
        public string Voicebank { get; set; }
        /// <summary>
        /// The path to the resynthesis engine used within this FluidSys.Sheet.
        /// </summary>
        public string Resampler { get; set; }
        /// <summary>
        /// The name used for this FluidSys.Sheet.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the tempo for this FluidSys.Sheet, in beats per minute (BPM).
        /// </summary>
        public int Bpm { get; set; }
        /// <summary>
        /// Gets or sets a list of strings representing the coolest things ever.
        /// </summary>
        public List<string> rendParams = new List<string>();
        /// <summary>
        /// The list of notes contained within this FluidSys.Sheet.
        /// </summary>
        public List<Note> notes = new List<Note>();
    }
}
