using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace ConsoleApplication1
{
    public class FileEncryptor
    {
        public enum CryptoMode
        {
            Encrypt,
            Decrypt
        }
        
        public static (string, string)  GenerateAesKeyAndIv(string keyFilePath)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.GenerateKey();
                aesAlg.GenerateIV();
                string key = Convert.ToBase64String(aesAlg.Key);
                string iv = Convert.ToBase64String(aesAlg.IV);
                File.WriteAllText(keyFilePath, key + Environment.NewLine + iv);
                return (key, iv);
            }
        }
        
        public static void ProcessAllFiles(string path, string key, string iv, CryptoMode mode)
        {
            var allFiles = GetAllFiles(path);
            foreach (var file in allFiles)
            {
                try
                {
                    ProcessFile(file, key, iv,mode);
                }
                catch (CryptographicException e)
                {
                    Console.Error.WriteLine($"Erreur cryptographique sur le fichier {file}: {e.Message}");
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine($"Erreur d'E/S sur le fichier {file}: {e.Message}");
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Erreur inattendue sur le fichier {file}: {e.Message}");
                }
            }
        }

        private static void ProcessFile(string path, string key,string iv, CryptoMode mode)
        {
            byte[] keyBytes = Convert.FromBase64String(key);
            byte[] ivBytes = Convert.FromBase64String(iv);
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                ICryptoTransform cryptoTransform =
                    mode == CryptoMode.Encrypt ? aes.CreateEncryptor() : aes.CreateDecryptor();
                byte[] fileBytes = File.ReadAllBytes(path);
                byte[] processedBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
                    {
                        cs.Write(fileBytes, 0, fileBytes.Length);
                    }

                    processedBytes = ms.ToArray();
                }

                File.WriteAllBytes(path, processedBytes);
            }
        }

        private static List<string> GetAllFiles(string rootPath)
        {
            List<string> fileList = new List<string>();
            Stack<string> dirs = new Stack<string>();

            if (!Directory.Exists(rootPath))
            {
                throw new ArgumentException();
            }

            dirs.Push(rootPath);
            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                string[] files;
                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.Error.WriteLine(e.Message);
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.Error.WriteLine(e.Message);
                    continue;
                }

                foreach (string file in files)
                {
                    try
                    {
                        // Ici, vous pouvez ajouter des opérations sur les fichiers si nécessaire
                        fileList.Add(file);
                    }
                    catch (FileNotFoundException e)
                    {
                        // Si le fichier a été supprimé par un autre programme
                        Console.Error.WriteLine(e.Message);
                    }
                }

                subDirs = Directory.GetDirectories(currentDir);
                foreach (string dir in subDirs)
                {
                    dirs.Push(dir);
                }
            }

            return fileList;
        }
    }
}