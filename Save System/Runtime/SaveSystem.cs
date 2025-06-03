using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System;

namespace ergulburak.SaveSystem
{
    public static class SaveSystem
    {
        private static SaveSettings _settings;

        private static SaveSettings Settings
        {
            get
            {
                if (!_settings)
                    _settings = Resources.Load<SaveSettings>("SaveSystem/SaveSettings");
                return _settings;
            }
        }

        public static string SaveDirectory =>
            Path.Combine(Application.persistentDataPath, Settings.savePath);

        public static string GetSaveFilePath(string key) =>
            Path.Combine(SaveDirectory, key + Settings.fileExtension);

        public static string FileExtension => Settings.fileExtension;

        private const int CurrentVersion = 1;

        private static readonly byte[] AesKey = new byte[]
        {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12,
            0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20
        };

        private static readonly byte[] AesIV = new byte[]
            { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10 };

        private static List<Type> cachedSaveableTypes;
        private static readonly Dictionary<string, ISaveable> dataCache = new Dictionary<string, ISaveable>();

        public static void Initialize()
        {
            cachedSaveableTypes = GetSaveableTypes().ToList();
            dataCache.Clear();
        }

        public static async Task InitializeAndLoadAsync(int slotId)
        {
            if (cachedSaveableTypes == null) Initialize();

            foreach (var type in cachedSaveableTypes!)
            {
                var instance = CreateInstanceOfType(type);
                if (instance != null)
                {
                    string filePath = GetFilePath(instance.SaveKey, slotId);
                    string cacheKey = GetCacheKey(instance.SaveKey, slotId);

                    if (File.Exists(filePath))
                    {
                        var loadedData = await LoadFromFileAsync(instance, slotId);
                        dataCache[cacheKey] = loadedData;
                    }
                    else
                    {
                        dataCache[cacheKey] = instance;
                        await SaveAsync(instance, slotId);
                    }
                }
            }
        }

        public static async Task SaveAllAsync(int slotId)
        {
            if (cachedSaveableTypes == null) Initialize();

            foreach (var type in cachedSaveableTypes!)
            {
                var instance = CreateInstanceOfType(type);
                if (instance != null)
                {
                    string cacheKey = GetCacheKey(instance.SaveKey, slotId);
                    ISaveable dataToSave = dataCache.GetValueOrDefault(cacheKey, instance);
                    await SaveAsync(dataToSave, slotId);
                }
            }
        }

        public static async Task SaveAsync(ISaveable saveable, int slotId)
        {
            try
            {
                string cacheKey = GetCacheKey(saveable.SaveKey, slotId);
                dataCache[cacheKey] = saveable;
                string dataJson = JsonUtility.ToJson(saveable, true);
                if (string.IsNullOrEmpty(dataJson) || dataJson == "{}")
                {
                    $"Serialization failed for {saveable.SaveKey}".DebugError();
                    return;
                }

                string json = $"{{\"version\":{CurrentVersion},\"data\":{dataJson}}}";
                string filePath = GetFilePath(saveable.SaveKey, slotId);

                string directory = Path.GetDirectoryName(filePath);
                Directory.CreateDirectory(directory!);

                byte[] dataBytes = Encoding.UTF8.GetBytes(json);
                byte[] compressedData = Compress(dataBytes);
                if (compressedData.Length == 0)
                {
                    $"Compression resulted in empty data for {saveable.SaveKey}".DebugError();
                    return;
                }

                byte[] encryptedData = Encrypt(compressedData);
                if (encryptedData.Length == 0)
                {
                    $"Encryption resulted in empty data for {saveable.SaveKey}".DebugError();
                    return;
                }

                await File.WriteAllBytesAsync(filePath, encryptedData);
                $"Successfully saved {saveable.SaveKey} to {filePath} (Slot {slotId})".Debug();
            }
            catch (Exception ex)
            {
                $"Failed to save {saveable.SaveKey} (Slot {slotId}): {ex.Message}\nStackTrace: {ex.StackTrace}".DebugError();
            }
        }

        public static T LoadFromCache<T>(int slotId) where T : class, ISaveable
        {
            try
            {
                T tempInstance = Activator.CreateInstance<T>();
                string cacheKey = GetCacheKey(tempInstance.SaveKey, slotId);

                return dataCache.TryGetValue(cacheKey, out var value) ? value as T : tempInstance;
            }
            catch (Exception ex)
            {
                $"Failed to load {typeof(T).Name} from cache (Slot {slotId}): {ex.Message}".DebugError();
                return Activator.CreateInstance<T>();
            }
        }

        private static async Task<ISaveable> LoadFromFileAsync(ISaveable saveable, int slotId)
        {
            try
            {
                string filePath = GetFilePath(saveable.SaveKey, slotId);

                if (!File.Exists(filePath))
                {
                    return saveable;
                }

                byte[] encryptedData = await File.ReadAllBytesAsync(filePath);
                if (encryptedData.Length == 0)
                {
                    $"Encrypted file is empty for {saveable.SaveKey} at {filePath} (Slot {slotId})".DebugError();
                    return saveable;
                }

                var decryptedData = Decrypt(encryptedData);
                if (decryptedData.Length == 0)
                {
                    $"Decryption resulted in empty data for {saveable.SaveKey} (Slot {slotId})".DebugError();
                    return saveable;
                }

                byte[] decompressedData = Decompress(decryptedData);
                if (decompressedData.Length == 0)
                {
                    $"Decompression resulted in empty data for {saveable.SaveKey} (Slot {slotId})".DebugError();
                    return saveable;
                }

                string json = Encoding.UTF8.GetString(decompressedData);
                if (string.IsNullOrEmpty(json))
                {
                    $"JSON file is empty for {saveable.SaveKey} at {filePath} (Slot {slotId})".DebugError();
                    return saveable;
                }

                var jsonObject = JsonUtility.FromJson<VersionWrapper>(json);
                if (jsonObject == null || jsonObject.version != CurrentVersion)
                {
                    $"Version mismatch or invalid JSON for {saveable.SaveKey} (Slot {slotId})".DebugWarning();
                    return saveable;
                }

                string dataJson = ExtractDataField(json);
                if (string.IsNullOrEmpty(dataJson))
                {
                    $"Data JSON is empty for {saveable.SaveKey} (Slot {slotId})".DebugError();
                    return saveable;
                }

                var data = JsonUtility.FromJson(dataJson, saveable.GetType()) as ISaveable;
                return data ?? saveable;
            }
            catch (Exception ex)
            {
                $"Failed to load {saveable.SaveKey} (Slot {slotId}): {ex.Message}\nStackTrace: {ex.StackTrace}".DebugError();
                return saveable;
            }
        }

        private static string ExtractDataField(string json)
        {
            try
            {
                int dataStartIndex = json.IndexOf("\"data\":", StringComparison.Ordinal) + "\"data\":".Length;
                if (dataStartIndex == -1) return null;
                int dataEndIndex = json.LastIndexOf("}}", StringComparison.Ordinal) + 1;
                return json.Substring(dataStartIndex, dataEndIndex - dataStartIndex).Trim();
            }
            catch (Exception ex)
            {
                $"Failed to extract data field: {ex.Message}".DebugError();
                return null;
            }
        }

        private static IEnumerable<Type> GetSaveableTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(ISaveable).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);
        }

        private static ISaveable CreateInstanceOfType(Type type)
        {
            try
            {
                return (ISaveable)Activator.CreateInstance(type);
            }
            catch
            {
                return null;
            }
        }

        private static string GetFilePath(string saveKey, int slotId)
        {
            return Path.Combine(SaveDirectory, $"{saveKey}_{slotId}{FileExtension}");
        }

        private static string GetCacheKey(string saveKey, int slotId)
        {
            return $"{saveKey}_{slotId}";
        }

        private static byte[] Compress(byte[] data)
        {
            try
            {
                using var compressedStream = new MemoryStream();
                using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    gzipStream.Write(data, 0, data.Length);
                    gzipStream.Flush();
                }

                return compressedStream.ToArray();
            }
            catch (Exception ex)
            {
                $"Compression failed: {ex.Message}\nStackTrace: {ex.StackTrace}".DebugError();
                return Array.Empty<byte>();
            }
        }

        private static byte[] Decompress(byte[] data)
        {
            try
            {
                using var compressedStream = new MemoryStream(data);
                using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
                using var decompressedStream = new MemoryStream();
                gzipStream.CopyTo(decompressedStream);
                return decompressedStream.ToArray();
            }
            catch (Exception ex)
            {
                $"Decompression failed: {ex.Message}\nStackTrace: {ex.StackTrace}".DebugError();
                return Array.Empty<byte>();
            }
        }

        private static byte[] Encrypt(byte[] data)
        {
            try
            {
                using Aes aes = Aes.Create();
                aes.Key = AesKey;
                aes.IV = AesIV;
                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                }

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                $"Encryption failed: {ex.Message}\nStackTrace: {ex.StackTrace}".DebugError();
                return Array.Empty<byte>();
            }
        }

        private static byte[] Decrypt(byte[] data)
        {
            try
            {
                using Aes aes = Aes.Create();
                aes.Key = AesKey;
                aes.IV = AesIV;
                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                }

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                $"Decryption failed: {ex.Message}\nStackTrace: {ex.StackTrace}".DebugError();
                return Array.Empty<byte>();
            }
        }

        [Serializable]
        private class VersionWrapper
        {
            public int version;
            public string data;
        }
    }
}