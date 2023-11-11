using System;
using Accord.Video.FFMPEG;
using AForge.Video.DirectShow;


namespace ConsoleApplication1
{
    internal class Program
    {
      
        public static void Main(string[] args)
        {
            string rootPath = @"D:\Cours\M2\programmation avance\test"; //   @"D:\Cours\M2\programmation avance\test"   -  "C:\\" pour Windows -  "/" pour Unix/Linux/Mac  -
            string keyPath = @"D:\Cours\M2\programmation avance\cle\macle.txt"; // @"D:\Cours\M2\programmation avance\cle\macle.txt"
            string fileVideoName = "test_video.avi"; // "test_video.avi"
          
            RunEncryptionProcess(rootPath,keyPath);
            try
            {
                string cameraMonikerString = ObtenirCameraMonikerString(); // Remplacez par la méthode pour obtenir le moniker de la caméra
                using (var cameraManager = CameraManager.Instance)
                {
                    cameraManager.Initialize(cameraMonikerString);
                    cameraManager.StartRecording(fileVideoName, 640, 480, 25, VideoCodec.MPEG4);
                    Console.WriteLine("Appuyez sur une touche pour arrêter l'enregistrement...");
                    Console.ReadKey();
                    cameraManager.StopRecording();
                }
                Console.WriteLine("Enregistrement terminé.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur: " + ex.Message);
            }
        }
        
        public static void RunEncryptionProcess(string rootPath, string keyPath)
        {
            Console.WriteLine("Voulez-vous (C)hiffrer ou (D)échiffrer le fichier?");
            var choice = Console.ReadLine().ToUpper();
            string key, iv;
            if (choice == "C") 
            {
                (key, iv)  = FileEncryptor.GenerateAesKeyAndIv(keyPath);
                Console.WriteLine("Chiffrement des fichiers ...");
                FileEncryptor.ProcessAllFiles(rootPath, key,iv,FileEncryptor.CryptoMode.Encrypt);
                Console.WriteLine("Chiffrement terminé.");
            }
            else if (choice == "D")
            {
                (key,iv) = GetUserEncryptionKey();
                Console.WriteLine("Déchiffrement des fichiers ...");
                FileEncryptor.ProcessAllFiles(rootPath, key,iv, FileEncryptor.CryptoMode.Decrypt);
                Console.WriteLine("Déchiffrement terminé.");
            }
            else
            {
                Console.WriteLine("Choix non valide.");
            }
        }
        
        private static (string,string) GetUserEncryptionKey()
        {
            Console.WriteLine("Veuillez entrer la clé secrète pour le déchiffrement:");
            string key = Console.ReadLine();
            Console.WriteLine("Veuillez entrer l'iv pour le déchiffrement:");
            string iv = Console.ReadLine();
            return (key, iv);
        }
        
        private static string ObtenirCameraMonikerString()
        {
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
                throw new Exception("Aucun périphérique vidéo trouvé.");
            Console.WriteLine("Sélectionnez le numéro de caméra:");
            for (int i = 0; i < videoDevices.Count; i++)
            {
                Console.WriteLine($"{i}: {videoDevices[i].Name}");
            }
            int deviceNumber = Convert.ToInt32(Console.ReadLine());

            if (deviceNumber >= 0 && deviceNumber < videoDevices.Count)
            {
                return videoDevices[deviceNumber].MonikerString;
            }
            else
            {
                throw new Exception("Numéro de caméra non valide.");
            }
        }
    }
}
