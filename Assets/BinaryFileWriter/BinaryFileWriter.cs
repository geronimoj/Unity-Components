using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

/*
    To Do:
    - Optimize (there is un-necessary memeory alloc during the process)
    - Add Safety Catches to avoid exceptions
        > File MIA or cannot be opened
        > Header Id not registered
        > Exception when loading or serializing object

    - Async functions?

    Ideas:
    - Could explose the BinaryWriter to objects to handle writing, then manually calculate written size. Might reduce the generation of byte[] slightly?
        > Could be un-safe, as users could modify the Pointer beyond what is expected.
 */

/// <summary>
/// A file writer for writing binary files using a safer format
/// </summary>
public static class BinaryFileWriter
{
    /// <summary>
    /// Functions for deserializing a chunk
    /// </summary>
    /// <param name="version">The version of the serializer used to make the chunk</param>
    /// <param name="chunk">The byte[] of data</param>
    /// <param name="children">Any child objects the serializer said it had.</param>
    /// <returns>Return the object generated from the data</returns>
    public delegate object Deserializer(int version, byte[] chunk, object[] children);

    /// <summary>
    /// The registered deserializers
    /// </summary>
    static Dictionary<string, Deserializer> registeredDeserializers = new Dictionary<string, Deserializer>();

    /// <summary>
    /// Register a deserializer for a file type
    /// </summary>
    /// <param name="headerId"></param>
    /// <param name="function"></param>
    public static void RegisterDeserializer(string headerId, Deserializer function)
    {
        if (ValidateHeader(headerId))
            throw new Exception(string.Format("Header '{0}' is not 4 bytes in length. Header must be a 4 byte long utf8 string!", headerId));

        registeredDeserializers.Add(headerId, function);
    }

    /// <summary>
    /// Write an object into a file
    /// </summary>
    /// <param name="object"></param>
    /// <param name="fullPathAndName"></param>
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
            HeaderData header = GetHeader(toSerialize, out byte[] chunk, out IWrittable[] children);

            // Add the data to our output file
            writer.Write(header.ToBytes(), 0, 16);

            if (chunk != null)
                writer.Write(chunk, 0, chunk.Length);

            // Write child objects
            foreach (var child in children)
                SerializeObject(child);
        }
    }

    /// <summary>
    /// Read a specific file into an object.
    /// </summary>
    /// <param name="fullPathAndName"></param>
    /// <returns>The object and it's Id (to help cast it to the correct type)</returns>
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
            HeaderData header = HeaderData.FromBytes(headerBuffer);

            // Read the rest of the data.
            string headerId = Encoding.UTF8.GetString(header.id);
            byte[] chunk = reader.ReadBytes(header.chunkSize);
            object[] children = new object[header.childCount];

            // Deserialize each child before we deserialize the main object
            for (int i = 0; i < header.childCount; i++)
                children[i] = Deserialize().Object;

            // Deserialize children
            return (registeredDeserializers[headerId](header.version, chunk, children), headerId);
        }

        return result;
    }

    /// <summary>
    /// Determines if the header is valid
    /// </summary>
    /// <param name="header"></param>
    /// <returns></returns>
    static bool ValidateHeader(string header)
    {
        return Encoding.UTF8.GetByteCount(header) == 4;
    }

    /// <summary>
    /// Determines if the header is valid. Also spits out the bytes for the header for serialization
    /// </summary>
    /// <param name="header"></param>
    /// <param name="headerBytes">The bytes for the header</param>
    /// <returns></returns>
    static bool ValidateHeader(string header, out byte[] headerBytes)
    {
        headerBytes = Encoding.UTF8.GetBytes(header);

        return headerBytes.Length == 4;
    }

    /// <summary>
    /// Generate the header for a Writtable object.
    /// </summary>
    /// <param name="object">The object to serialize</param>
    /// <param name="chunk">The chunk data for this object</param>
    /// <param name="children">The child objects that need to be serialized</param>
    /// <returns></returns>
    /// <remarks>
    /// Also spits out the Chunk & Children objects as they were loaded to get the header (not point in letting data go to waste)
    /// </remarks>
    static HeaderData GetHeader(IWrittable @object, out byte[] chunk, out IWrittable[] children)
    {
        HeaderData header = new HeaderData();
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

    /// <summary>
    /// Struct to hold Header information
    /// </summary>
    ref struct HeaderData
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

        public static HeaderData FromBytes(byte[] bytes)
        {
            HeaderData header = new HeaderData();
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