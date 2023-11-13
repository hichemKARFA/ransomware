using System;
using AForge.Video;
using AForge.Video.DirectShow;
using Accord.Video.FFMPEG;

namespace ConsoleApplication1
{
    public class CameraManager : IDisposable

    {
    private static CameraManager instance;
    private VideoCaptureDevice videoSource;
    private VideoFileWriter videoWriter;
    private bool isRecording;
    private bool isInitialized;


    // Constructeur privé pour empêcher l'instanciation directe
    private CameraManager()
    {
    }

    // Propriété statique pour accéder à l'instance
    public static CameraManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CameraManager();
            }

            return instance;
        }
    }

    public void Initialize(string cameraMonikerString)
    {
        videoSource = new VideoCaptureDevice(cameraMonikerString);
        videoSource.NewFrame += new NewFrameEventHandler(OnNewFrame);
        isInitialized = true;
    }

    public void StartRecording(string fileName, int width, int height, int frameRate, VideoCodec codec)
    {
        if (!isInitialized)
        {
            throw new InvalidOperationException("La caméra n'est pas initialisée.");
        }

        if (isRecording)
        {
            throw new InvalidOperationException("L'enregistrement est déjà en cours.");
        }

        try
        {
            videoWriter = new VideoFileWriter();
            videoWriter.Open(fileName, width, height, frameRate, codec);
            videoSource.Start();
            isRecording = true;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erreur lors de l'enregistrement.", ex);
        }
        finally
        {
            // Nettoyage des ressources
            if (!isRecording)
            {
                videoWriter?.Close();
                videoSource?.SignalToStop();
            }
        }
    }

    private void OnNewFrame(object sender, NewFrameEventArgs eventArgs)
    {
        if (isRecording)
        {
            videoWriter.WriteVideoFrame(eventArgs.Frame);
        }
    }


    public void StopRecording()
    {
        if (!isRecording)
        {
            return;
        }

        try
        {
            if (videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }

            if (videoWriter.IsOpen)
            {
                videoWriter.Close();
            }

            isRecording = false;
        }

        catch (Exception ex)
        {
            throw new ApplicationException("Erreur lors de l'arrêt de l'enregistrement.", ex);
        }
    }

    public void Dispose()
    {
        // Libérez les ressources de VideoFileWriter
        if (videoWriter != null && videoWriter.IsOpen)
        {
            videoWriter.Close();
            videoWriter.Dispose();
        }
        
        // Arrêtez et libérez le VideoCaptureDevice
        if (videoSource != null && videoSource.IsRunning)
        {
            videoSource.SignalToStop();
            videoSource.WaitForStop();
            videoSource.NewFrame -= OnNewFrame;
            videoSource = null;
        }
        
        GC.SuppressFinalize(this);
    }
    
    // Destructeur
    ~CameraManager()
    {
        Dispose();
    }
    
    }
}