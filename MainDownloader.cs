using CSCore;
using CSCore.MediaFoundation;
using System.Linq;
using System;
using System.Collections;
using System.Net;
using VideoLibrary;
//Dont need system Diagn testing only
using System.Diagnostics;
using System.IO;



namespace YoutubePlaylistDownloader
{
    class MainDownloader
    {
        /// <summary>
        /// READ ME -------------------------------------------------
        /// YouTube Playlist Downloader. Automatically Downloads and converts to .mp3 format.
        /// PLaylist Links Can only be in this URL Format https://www.youtube.com/playlist?list=_PlaylistID_ for Now.
        /// </summary>
        /*TODO LIST
         * 1) Find A Way to Download Youtube Video (Done - VideoLibrary)
         * 2) Find A Way to Have Final Format be .MP3 ( Done - FFMpeg)
         *      A) Find a Faster Converter ( Done - CSCore)
         * 3) Find A Way to Extract All Seprate Videos ID From Playlist Link (Done - .Net)
         *      A) Find a Way to get past the 100 Video limit. ( Done - Method PlayListIndexGenerator (New Limit 200))
         * 4) Find A Way to Organize songs into Differn't Playlist Folders according to what Playlist they come From
         * 5) Create Gui
         * 6) Get Rid of Hard Coded Varaibles like PATH,PATHSONGLIST.
         * 7) Option To Check Playlist every 5 Mins to Download new songs added.
         * 8)
         */
        //static string PATH = @"C:\Users\Sam\Music\Music\";
        static string PATH = System.IO.Directory.GetCurrentDirectory() + @"\Music\";
        static string PATHSONGLIST = PATH + @"11DownloadedSongList.txt";
        //static string FFMPEGPATH;

        static void Main(string[] args)
        {
            //FFMPEGPATH = System.IO.Directory.GetCurrentDirectory().Replace(@"\Debug", @"\ffmpeg\bin\ffmpeg.exe");
            Stopwatch mywatch = new Stopwatch();
            string[] PlaylistUrls = new string[2];
            PlaylistUrls[0] = "https://www.youtube.com/playlist?list=PLtlQWFxwdDxWElh5JDDNSAV4h1h3kcA73";
            PlaylistUrls[1] = "https://www.youtube.com/playlist?list=PLtlQWFxwdDxWt0zz33Ge-3XV60MZ2TYTv";
            Console.WriteLine("1) Download a Song\n" +
                "2) Download a PlayList" +
                "3) Drive Music Playlist Download" +
                "4) Best Trap City Music Playlist Download" +
                "9) Exit");
            string input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    Console.WriteLine("Put In Your Youtube Song with this link style link(https://www.youtube.com/watch?v=_VideoID)");
                    input = Console.ReadLine();
                    DownloadYoutubeVideo(input);
                    break;
                case "2":
                    Console.WriteLine("Put In Your Youtube Playlist with this link style link(https://www.youtube.com/playlist?list=)");
                    input = Console.ReadLine();
                    mywatch.Start();
                    PlaylistVideoLinkExtractor(PlayListIndexGenerator(input), PlaylistIdExtractor(input), true);
                    break;
                case "3":
                    mywatch.Start();
                    PlaylistVideoLinkExtractor(PlayListIndexGenerator(PlaylistUrls[0]), PlaylistIdExtractor(PlaylistUrls[0]), true);
                    break;
                case "4":
                    mywatch.Start();
                    PlaylistVideoLinkExtractor(PlayListIndexGenerator(PlaylistUrls[1]), PlaylistIdExtractor(PlaylistUrls[1]), true);
                    break;
                case "9":
                    Console.WriteLine("Exiting");
                    break;
                default:
                    Console.WriteLine("Exiting");
                    break;
            }

            mywatch.Stop();
            Console.WriteLine("All Done in = " + mywatch.Elapsed);
            Console.ReadLine();
        }
        /// <summary>
        /// Method Makes Sure Essiatial Variables are Set.
        /// </summary>
        private static void Intialization()
        {
            if (PATH == null)
                throw new Exception("Intialization Failed. PATH is Null");
            else if (!File.Exists(PATHSONGLIST))
            {
                System.IO.Directory.CreateDirectory(PATH);
                File.Create(PATHSONGLIST);
            }
        }
        /// <summary>
        /// Method to 2Grab Every Youtube Videos Unique ID From Playlist.
        /// </summary>
        ///<param name="Url">
        ///Takes https://www.youtube.com/watch?v=_VideoID_&amp;index=1&amp;list=_PlaylistID 
        ///</param>
        ///<param name="PlaylistId">
        ///Takes String PlayListID From <see cref="PlaylistIdExtractor(string)"/>
        ///</param>
        private static void PlaylistVideoLinkExtractor(string Url, string PlaylistId, bool Download = false)
        {
            Intialization();
            ArrayList Playlist = new ArrayList();
            foreach (string x in HttpPageRequest(Url))
            {
                if (x.Contains("playlist-video") & x.Contains(PlaylistId))
                {
                    string VideoId = VideoIdExtractor(x);
                    if (SongListChecker(VideoId) == false)
                    {
                        Playlist.Add(VideoId);
                        Console.WriteLine(VideoId);
                    }
                    else
                    {
                        Console.WriteLine("Was already downloaded");
                    }
                }
            }
            if (Download == true)
            {
                int i = 0;
                //DownloadYoutubeVideos(ref Playlist);
                Playlist.Reverse();
                using (var tw = new StreamWriter(PATHSONGLIST, true))
                {
                    foreach (string VideoId in Playlist)
                    {
                        if (DownloadYoutubeVideo("www.youtube.com" + VideoId) == true)
                        {
                            i++;
                            tw.WriteLine(VideoId);
                        }
                    }
                }
                Console.WriteLine("Downloaded (" + i + ") Songs");
            }
        }
        /// <summary> 
        /// Method to Grab a Youtube Playlist Id From Youtube Playlist Link. Returns String PlaylistId
        /// </summary>
        ///<param name="Url">
        ///<para>Can take https://www.youtube.com/playlist?list=_PlaylistID_ 
        ///or https://www.youtube.com/watch?v=_VideoID_&amp;index=1&amp;list=_PlaylistID_
        ///or https://www.youtube.com/watch?v=_VideoID_&amp;list=_PlaylistID_&amp;index=1</para> 
        /// </param>
        private static string PlaylistIdExtractor(string Url)
        {
            try
            {
                if (Url == null)
                    throw new Exception("PlaylistIdExtract Method Was Feed A Null URL");
                else if (Url.Contains("list="))
                {
                    Url = Url.Remove(0, Url.IndexOf("list=") + 5);
                    if (Url.Contains("&index="))
                        return Url.Remove(Url.IndexOf("&index="), Url.Length);
                    return Url;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
        /// <summary> 
        /// Method to Generate a Youtube Playlist Url With a Index.
        /// Returns String Url = https://www.youtube.com/watch?v=_VideoID_&amp;index=1&amp;list=_PlaylistID_.
        /// </summary>
        /// <param name="Url">Can only take URL https://www.youtube.com/playlist?list=_PlaylistID_</param>
        private static string PlayListIndexGenerator(string Url)
        {
            foreach (string x in HttpPageRequest(Url))
            {
                if (x.Contains("pl-video-title-link"))
                {
                    return ("https://www.youtube.com" +
                        VideoIdExtractor(x) +
                        "&index=1" +
                        "&list=" +
                        PlaylistIdExtractor(Url));
                }
            }
            return null;
        }
        /// <summary>
        /// Mathod Checks PATHSONGLIST to See if videoID was Already Downloaded
        /// </summary>
        /// <param name="VideoID">Takes a watch?v=_VideoID_</param>
        /// <returns></returns>
        private static bool SongListChecker(string VideoID)
        {
            using (var tr = new StreamReader(PATHSONGLIST))
            {
                string text;
                while ((text = tr.ReadLine()) != null)
                {
                    if (text.Contains(VideoID))
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Method Takes out VideoID from <see cref="HttpPageRequest(string)"/>. Return String VideoID.
        /// </summary>
        /// <param name="ContainingVideoId">A String containing a Youtube VideoID</param>
        /// <returns></returns>
        private static string VideoIdExtractor(string ContainingVideoID)
        {
            return ContainingVideoID.Substring(ContainingVideoID.IndexOf("href=\"") + 6, 20);
        }
        /// <summary>
        /// Method Downloads a Youtube Video from The Video Url.
        /// </summary>
        /// <param name="Url">Takes a https://www.youtube.com/watch?v=_VideoID_ </param>
        private static bool DownloadYoutubeVideo(string Url)
        {
            string videoFullName = "foo";
            try
            {
                var youtube = YouTube.Default;
                var video = youtube.GetVideo(Url);
                videoFullName = video.FullName.Replace(" - YouTube.mp4", "");
                Console.WriteLine("Downloading " + videoFullName);
                //File.WriteAllBytes(PATH + videoFullName + ".mp4", video.GetBytes());

                IWaveSource videoSource = CSCore.Codecs.CodecFactory.Instance.GetCodec(new Uri(video.Uri));
                Tuple<IWaveSource, String> package = Tuple.Create(videoSource, videoFullName);
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ThreadedConvertToMp3), package);
            }
            catch (Exception e)
            {
                if (File.Exists(PATH + videoFullName + ".mp3"))
                    File.Delete(PATH + videoFullName + ".mp3");
                Console.WriteLine(e);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Method is For Threadpooling the ConvertToMp3 Method.
        /// </summary>
        /// <param name="callback">Requires A Tuple of source video.URI and FileName</param>
        private static void ThreadedConvertToMp3(object callback)
        {
            IWaveSource source = ((Tuple<IWaveSource, String>)callback).Item1;
            String videoTitle = ((Tuple<IWaveSource, String>)callback).Item2;
            Console.WriteLine("Proccessing " + videoTitle);
            ConvertToMp3(source, videoTitle);
        }
        /// <summary>
        /// Method Converts IWaveSource .Mp4 to .Mp3 with 192kbs Sample Rate and Saves it to Path Using the videoTitle
        /// </summary>
        /// <param name="source">Takes a IWaveSource videoSource = CSCore.Codecs.CodecFactory.Instance.GetCodec(new Uri(video.Uri))</param>
        /// <param name="videoTitle"> Takes the Video Title</param>
        /// <returns></returns>
        private static bool ConvertToMp3(IWaveSource source, string videoTitle)
        {
            var supportedFormats = MediaFoundationEncoder.GetEncoderMediaTypes(AudioSubTypes.MpegLayer3);
            if (!supportedFormats.Any())
            {
                Console.WriteLine("The current platform does not support mp3 encoding.");
                return true;
            }

            if (supportedFormats.All(
                    x => x.SampleRate != source.WaveFormat.SampleRate && x.Channels == source.WaveFormat.Channels))
            {
                int sampleRate =
                    supportedFormats.OrderBy(x => Math.Abs(source.WaveFormat.SampleRate - x.SampleRate))
                        .First(x => x.Channels == source.WaveFormat.Channels)
                        .SampleRate;

                Console.WriteLine("Samplerate {0} -> {1}", source.WaveFormat.SampleRate, sampleRate);
                Console.WriteLine("Channels {0} -> {1}", source.WaveFormat.Channels, 2);
                source = source.ChangeSampleRate(sampleRate);
            }
            using (source)
            {
                using (var encoder = MediaFoundationEncoder.CreateMP3Encoder(source.WaveFormat, PATH + videoTitle + ".mp3"))
                {
                    byte[] buffer = new byte[source.WaveFormat.BytesPerSecond];
                    int read;
                    while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        encoder.Write(buffer, 0, read);

                        //Console.CursorLeft = 0;
                        //Console.Write("{0:P}/{1:P}", (double)source.Position / source.Length, 1);
                    }
                }
            }
            File.Delete(PATH + videoTitle + ".mp4");
            return false;
        }
        /// <summary> 
        /// Method to Grab HttpPageRequest. Returns a String Array Split by \n.
        /// </summary> 
        /// <param name="Url">Takes Any Url</param>
        private static string[] HttpPageRequest(string Url)
        {
            HttpWebRequest wb = (HttpWebRequest)WebRequest.Create(Url);
            wb.Method = "GET";
            wb.KeepAlive = true;
            wb.Proxy = null;
            wb.UserAgent = "Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 68.0.3440.106 Safari / 537.36";

            return new StreamReader(wb.GetResponse().GetResponseStream()).ReadToEnd().Split('\n');
        }
        /* Not Used Methods --------------------
        /// <summary>
        /// Method Ment for ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadedFfmpeg), videoFullName)
        /// </summary>
        /// <param name="callback">Takes string videoFullName</param>
        private static void DownloadYoutubeVideos(ref ArrayList Playlist)
        {
            using (var cli = new VideoClient())
            {
                var youtube = YouTube.Default;
                Video video;
                string videoFullName = "foo";
                using (var tw = new StreamWriter(PATHSONGLIST, true))
                {
                    foreach (string link in Playlist)
                    {
                        try
                        {
                            video = youtube.GetVideo("https://www.youtube.com" + link);
                            videoFullName = video.FullName.Replace(" - YouTube.mp4", "");
                            //File.WriteAllBytes(PATH + videoFullName + ".mp4", cli.GetBytes(video));

                            IWaveSource videoSource = CSCore.Codecs.CodecFactory.Instance.GetCodec(new Uri(video.Uri));
                            Tuple<IWaveSource, String> package = Tuple.Create(videoSource, videoFullName);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadedConvertToMp3), package);
                            tw.WriteLine(link);
                        }
                        catch (Exception e)
                        {
                            if (File.Exists(PATH + videoFullName + ".mp3"))
                                File.Delete(PATH + videoFullName + ".mp3");
                            Console.WriteLine(e);
                            Playlist.Remove(link);
                        }
                    }
                }
            }
        }
        private static void ThreadedFfmpeg(object callback)
        {
            string t = (string)callback;
            Console.WriteLine("Proccessing " + t);
            Ffmpeg(t);
            //ConvertToMp3(t);
        }
        /// <summary>
        /// Method Converts .Mp4 to .Mp3 and Deletes .Mp4
        /// </summary>
        /// <param name="videoTitle">Just Video Name without .mp4 or PATH</param>
        private static void Ffmpeg(string videoTitle)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //process.StartInfo.FileName = "ffmpeg.exe";
            process.StartInfo.FileName = FFMPEGPATH;
            process.StartInfo.Arguments = "-i \"" + PATH + videoTitle + ".mp4\"" +
                " -qscale 0 \"" + PATH + videoTitle + ".mp3\"";
            process.Start();
            process.WaitForExit();
            File.Delete(PATH + videoTitle + ".mp4");
        }
        private static void ThreadedDownloadYoutubeVideo(object callback)
        {

            string Url = "www.youtube.com" + (string)callback;
            Console.WriteLine(Url);
            string videoFullName = "foo";
            try
            {
                var youtube = YouTube.Default;
                var video = youtube.GetVideo(Url);
                videoFullName = video.FullName.Replace(" - YouTube.mp4", "");
                Console.WriteLine("Downloading " + videoFullName);
                //File.WriteAllBytes(PATH + videoFullName + ".mp4", video.GetBytes());

                IWaveSource videoSource = CSCore.Codecs.CodecFactory.Instance.GetCodec(new Uri(video.Uri));
                ConvertToMp3(videoSource, videoFullName);
                //Tuple<IWaveSource, String> package = Tuple.Create(videoSource, videoFullName);
                //ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadedConvertToMp3), package);
            }
            catch (Exception e)
            {
                if (File.Exists(PATH + videoFullName + ".mp3"))
                    File.Delete(PATH + videoFullName + ".mp3");
                Console.WriteLine(e);
            }
        }
        */
    }
}
