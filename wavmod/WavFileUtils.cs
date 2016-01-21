/*====================================================*\
 *||          Copyright(c) KineticIsEpic.             ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

// code from http://mark-dot-net.blogspot.com/2009/09/trimming-wav-file-using-naudio.html

using NAudio.Wave;
using System;

public static class WavFileUtils {
    /// <summary>
    /// Removes the specified time from the beginning and end of a .wav file and saves the result.
    /// </summary>
    /// <param name="inPath">The input .wav file to trim.</param>
    /// <param name="outPath">The path to save the output to.</param>
    /// <param name="cutFromStart">The amount of time to remove from the start of the file.</param>
    /// <param name="cutFromEnd">The amount of time to remove from the end of the file.</param>
    public static void TrimWavFile(string inPath, string outPath, TimeSpan cutFromStart, TimeSpan cutFromEnd) {
        using (WaveFileReader reader = new WaveFileReader(inPath)) {
            using (WaveFileWriter writer = new WaveFileWriter(outPath, reader.WaveFormat)) {
                int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

                int startPos = (int)cutFromStart.TotalMilliseconds * bytesPerMillisecond;
                startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

                int endBytes = (int)cutFromEnd.TotalMilliseconds * bytesPerMillisecond;
                endBytes = endBytes - endBytes % reader.WaveFormat.BlockAlign;
                int endPos = (int)reader.Length - endBytes;

                TrimWavFile(reader, writer, startPos, endPos);
            }
        }
    }

    private static void TrimWavFile(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos) {
        reader.Position = startPos;
        byte[] buffer = new byte[1024];
        while (reader.Position < endPos) {
            int bytesRequired = (int)(endPos - reader.Position);
            if (bytesRequired > 0) {
                int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                int bytesRead = reader.Read(buffer, 0, bytesToRead);
                if (bytesRead > 0) {
                    writer.WriteData(buffer, 0, bytesRead);
                }
            }
        }
    }
}