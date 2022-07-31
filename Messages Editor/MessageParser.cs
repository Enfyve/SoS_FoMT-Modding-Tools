using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Messages_Editor
{

    class MessageParser
    {
        private string filePath;
        private List<uint> offsets;
        private List<string> entries;

        public MessageParser(string filePath)
        {
            this.filePath = filePath;
        }

        public List<string> LoadEntries()
        {
            entries = new List<string>();
            offsets = new List<uint>();
            using (var fs = File.OpenRead(filePath))
            {
                var br = new BinaryReader(fs);

                // Read until no more strings
                uint index;
                while (true)
                {
                    index = br.ReadUInt32();

                    // Determine if the data read is an offset or part of the first entry
                    if ((index & 0xFFE00000) == 0 && (index & 0xFFFFFFFF) != 0)
                    {
                        offsets.Add(index);
                    }
                    else
                    {
                        br.BaseStream.Seek(-4, SeekOrigin.Current);
                        break;
                    }
                }

                // Read strings
                for (int j = 0; j < offsets.Count; j++)
                {
                    int length;

                    if (j < offsets.Count - 1)
                        length = (int)(offsets[j + 1] - offsets[j]);
                    else
                        length = (int)(br.BaseStream.Length - offsets[j]);

                    entries.Add(Encoding.Unicode.GetString(br.ReadBytes(length)).Replace("\n", Environment.NewLine));
                }

            }

            return entries;
        }

        
        public bool Export(List<String> entries, bool createBackup)
        {
            int offset = (entries.Count * 4); // 4 bytes per entry

            try
            {
                if (createBackup)
                    File.Copy(filePath, filePath + ".bak", true);

                using (var file = File.Open(filePath, FileMode.Truncate))
                {
                    BinaryWriter bw = new BinaryWriter(file);
                    bw.Seek(0, SeekOrigin.Begin);

                    // Write offsets
                    for (int i = 0; i < entries.Count; i++)
                    {
                        bw.Write(offset);
                        int strLength = Encoding.Unicode.GetByteCount(entries[i]);
                        offset += strLength; // Increment offset by the length of the text entry
                    }

                    // Write data
                    foreach (var entry in entries)
                    {
                        var adjustedEntry = entry.Replace("\r\n", "\n");
                        bw.Write(Encoding.Unicode.GetBytes(adjustedEntry));
                    }

                    bw.Flush();
                    bw.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
