using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

/*
 To Do:
 - Optimize (there is un-necessary memeory alloc during the process)

    Ideas:
    - Could explose the BinaryWriter to objects to handle writing, then manually calculate written size. Might reduce the generation of byte[] slightly?
        > Could be un-safe, as users could modify the Pointer beyond what is expected.
 */

/// <summary>
/// A file writer for writing binary files using a safer format
/// </summary>
public static class BinaryFileWriter
{
    public delegate object Deserializer(int version, byte[] chunk, object[] children);

    static Dictionary<string, Deserializer> deserializers = new Dictionary<string, Deserializer>();

    public static void RegisterDeserializer(string headerId, Deserializer function)
    {
        deserializers.Add(headerId, function);
    }

    public static void WriteToFile(IWrittable @object, string fullPathAndName)
    {
        var stream = File.OpenWrite(fullPathAndName);
        var writer = new BinaryWriter(stream);

        SerializeObject(@object);

        writer.Close();
        writer.Dispose();

        stream.Close();
        stream.Dispose();

        void SerializeObject(IWrittable toSerialize)
        {   // Get object data
            Header header = GetHeader(toSerialize, out byte[] chunk, out IWrittable[] children);

            // Add the data to our output file
            writer.Write(header.ToBytes(), 0, 16);

            if (chunk != null)
                writer.Write(chunk, 0, chunk.Length);

            // Write child objects
            foreach (var child in children)
                SerializeObject(child);
        }
    }

    public static (object LoadedFile, string Header) ReadFile(string fullPathAndName)
    {
        var stream = File.OpenRead(fullPathAndName);
        var reader = new BinaryReader(stream);

        byte[] headerBuffer = new byte[16];

        // Deserialize file
        var result = Deserialize();

        reader.Close();
        reader.Dispose();

        stream.Close();
        stream.Dispose();

        (object Object, string Id) Deserialize()
        {   // Read the header
            reader.Read(headerBuffer, 0, 16);
            Header header = Header.FromBytes(headerBuffer);

            // Read the rest of the data.
            string headerId = Encoding.UTF8.GetString(header.id);
            byte[] chunk = reader.ReadBytes(header.chunkSize);
            object[] children = new object[header.childCount];

            // Deserialize each child before we deserialize the main object
            for (int i = 0; i < header.childCount; i++)
                children[i] = Deserialize().Object;

            // Deserialize children
            return (deserializers[headerId](header.version, chunk, children), headerId);
        }

        return result;
    }

    static bool ValidateHeader(string header, out byte[] headerBytes)
    {
        headerBytes = Encoding.UTF8.GetBytes(header);

        return headerBytes.Length == 4;
    }

    static Header GetHeader(IWrittable @object, out byte[] chunk, out IWrittable[] children)
    {
        Header header = new Header();
        var idData = @object.GetHeader();
        // Validate header can be used
        if (ValidateHeader(idData.UTF8Header, out header.id))
            throw new Exception(string.Format("Header '{0}' is not 4 bytes in length. Header must be a 4 byte long utf8 string!", idData.UTF8Header));

        chunk = @object.GetChunk();
        children = @object.GetChildren();

        header.version = idData.Version;
        header.chunkSize = chunk.Length;
        header.childCount = children.Length;

        return header;
    }

    ref struct Header
    {
        public byte[] id;
        public int version;
        public int chunkSize;
        public int childCount;

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[16];
            byte[] versionBytes = BitConverter.GetBytes(version);
            byte[] sizeBytes = BitConverter.GetBytes(chunkSize);
            byte[] childBytes = BitConverter.GetBytes(childCount);
            // Write bytes into array
            Array.Copy(id, 0, bytes, 0, 4);
            Array.Copy(versionBytes, 0, bytes, 4, 4);
            Array.Copy(sizeBytes, 0, bytes, 8, 4);
            Array.Copy(childBytes, 0, bytes, 12, 4);

            return bytes;
        }

        public static Header FromBytes(byte[] bytes)
        {
            Header header = new Header();
            header.id = new byte[4];
            // Extract header
            Array.Copy(bytes, 0, header.id, 0, 4);

            // Extract ints
            header.version = BitConverter.ToInt32(bytes, 4);
            header.chunkSize = BitConverter.ToInt32(bytes, 8);
            header.childCount = BitConverter.ToInt32(bytes, 12);

            return header;
        }
    }
}

public interface IWrittable
{
    /// <summary>
    /// Gets header Data
    /// </summary>
    /// <returns>Returns a 4 byte UTF 8 string and a version</returns>
    (string UTF8Header, int Version) GetHeader();
    /// <summary>
    /// The data for this object
    /// </summary>
    /// <returns></returns>
    byte[] GetChunk();
    /// <summary>
    /// Dependent children objects
    /// </summary>
    /// <returns></returns>
    IWrittable[] GetChildren();
}