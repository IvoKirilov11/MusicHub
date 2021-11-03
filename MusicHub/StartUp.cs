namespace MusicHub
{
    using System;
    using System.Linq;
    using System.Text;
    using Data;
    using Initializer;

    public class StartUp
    {
        public static void Main(string[] args)
        {
            MusicHubDbContext context = 
                new MusicHubDbContext();

            DbInitializer.ResetDatabase(context);

            //Test your solutions here
        }

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {

            var albumsByProducer = context.Albums
                .Where(a => a.ProducerId == producerId)
                .Select(a => new
                {
                    AlbumName = a.Name,
                    AlbumReleaseDate = a.ReleaseDate,
                    ProducerName = a.Producer.Name,
                    AlbumSong = a.Songs
                        .Select(s => new
                        {
                            SongName = s.Name,
                            SongPrice = s.Price,
                            WriterName = s.Writer.Name
                        }),
                    AlbumPrice = a.Price
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var albums in albumsByProducer.OrderByDescending(a => a.AlbumPrice))
            {
                sb.Append($"-AlbumName: {albums.AlbumName}\r\n");
                sb.Append($"-ReleaseDate: {albums.AlbumReleaseDate:MM/dd/yyyy}\r\n");
                sb.Append($"-ProducerName: {albums.ProducerName}\r\n");
                sb.Append("-Songs:\r\n");

                int count = 1;

                foreach (var song in albums.AlbumSong
                                                    .OrderByDescending(s => s.SongName)
                                                    .ThenBy(s => s.WriterName))
                {
                    sb.Append($"---#{count}\r\n");
                    sb.Append($"---SongName: {song.SongName}\r\n");
                    sb.Append($"---Price: {song.SongPrice:f2}\r\n");
                    sb.Append($"---Writer: {song.WriterName}\r\n");

                    count++;
                }

                sb.Append($"-AlbumPrice: {albums.AlbumPrice:f2}\r\n");
            }

            return sb.ToString().TrimEnd();
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var AllSongs = context.Songs
                .Where(x => x.Duration.TotalSeconds > duration)
                .Select(x => new
                {
                    SongName = x.Name,
                    PerformerFullName = x.SongPerformers.Select(x => x.Performer.FirstName + " " + x.Performer.LastName).FirstOrDefault(),
                    WriterName = x.Writer.Name,
                    AlbumProducer = x.Album.Producer.Name,
                    Duration = x.Duration


                })
                .OrderBy(s => s.SongName)
                .ThenBy(s => s.WriterName)
                .ThenBy(s => s.PerformerFullName)
                .ToList();

            StringBuilder sb = new StringBuilder();
            int count = 1;

            foreach (var song in AllSongs)
            {

                sb.Append($"-Song #{count}\r\n");
                sb.Append($"---SongName: {song.SongName}\r\n");
                sb.Append($"---Writer: {song.WriterName}\r\n");
                sb.Append($"---Performer: {song.PerformerFullName}\r\n");
                sb.Append($"---AlbumProducer: {song.AlbumProducer}\r\n");
                sb.Append($"---Duration: {song.Duration:c}\r\n");

                count++;
            }

            return sb.ToString().TrimEnd();

        }
    }
}
