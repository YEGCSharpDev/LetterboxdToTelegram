﻿using System.Xml;
using System.Net;
using Microsoft.Data.Sqlite;
using HtmlAgilityPack;


namespace LetterboxdToCinephilesChannel
{
    internal class RSSReader
    {
        private static string databasePath = "entries.db";
        private static string tableName = "LatestEntries";
        ChannelCalls calls = new ChannelCalls();
        internal void Execute()
        {
            //SQLitePCL.Batteries.Init();

            InitializeDatabase();

            // Run the loop indefinitely
            while (true)
            {
                //string[] urls = Environment.GetEnvironmentVariable("RSS_URLS").Split(',');
                string[] urls = { "https://letterboxd.com/waffle1234/rss", "https://letterboxd.com/Azaghast/rss", "https://letterboxd.com/Azaghast/rss" };


                foreach (string url in urls)
                {
                    string xmlContent = DownloadXmlContent(url);

                    if (xmlContent != null)
                    {
                        TextMessage parsedData = ParseXml(xmlContent);

                        string textExtract = $"{parsedData.FilmTitle}{parsedData.FilmYear}{parsedData.MemberRating}{parsedData.TotalRating}{parsedData.ImgSrc}{parsedData.Review}";

                        if (!EntryExists(textExtract) && !string.IsNullOrWhiteSpace(textExtract))
                        {
                            InsertEntry(textExtract);
                            Console.WriteLine(textExtract);
                            //calls.SendMessageAsync(textExtract);
                            calls.SendPhotoAsync(parsedData);
                        }
                        
                    }
                    else
                    {
                        Console.WriteLine($"Failed to download XML from {url}");
                    }
                }

                // Sleep for 10 minutes before the next iteration
                System.Threading.Thread.Sleep(10 * 60 * 1000); // Sleep for 10 minutes
            }
        }

        static void InitializeDatabase()
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                connection.Open();

                // Create the table if it doesn't exist
                using (SqliteCommand command = new SqliteCommand($"CREATE TABLE IF NOT EXISTS {tableName} (EntryContent TEXT PRIMARY KEY);", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        static void InsertEntry(string entryContent)
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                connection.Open();

                // Insert the entry into the table
                using (SqliteCommand command = new SqliteCommand($"INSERT INTO {tableName} (EntryContent) VALUES (@entryContent);", connection))
                {
                    command.Parameters.AddWithValue("@entryContent", entryContent);
                    command.ExecuteNonQuery();
                }
            }
        }

        static bool EntryExists(string entryContent)
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                connection.Open();

                // Check if the entry exists in the table
                using (SqliteCommand command = new SqliteCommand($"SELECT COUNT(*) FROM {tableName} WHERE EntryContent = @entryContent;", connection))
                {
                    command.Parameters.AddWithValue("@entryContent", entryContent);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }



        static string DownloadXmlContent(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    return client.DownloadString(url);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading XML from {url}: {ex.Message}");
                return null;
            }
        }

        static TextMessage ParseXml(string xmlContent)
        {
            TextMessage message = new TextMessage();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("letterboxd", "https://letterboxd.com");

            // Add other namespaces as needed
            nsManager.AddNamespace("tmdb", "https://themoviedb.org");

            XmlNode latestItem = xmlDoc.SelectSingleNode("/rss/channel/item[1]"); // Select the first item

            if (latestItem != null)
            {
                message.FilmTitle = latestItem.SelectSingleNode(".//letterboxd:filmTitle", nsManager)?.InnerText;
                message.FilmYear = latestItem.SelectSingleNode(".//letterboxd:filmYear", nsManager)?.InnerText;
                message.MemberRating = latestItem.SelectSingleNode(".//letterboxd:memberRating", nsManager)?.InnerText;
                message.TotalRating = "5";

                string description = latestItem.SelectSingleNode(".//description", nsManager)?.InnerText.Trim();
                string descriptionhtml = WebUtility.HtmlDecode(description);

                ExtractImgUrlAndComments(descriptionhtml,message);
            }

            return message;

        }

        static void ExtractImgUrlAndComments(string descriptionHtml,TextMessage message)
        {
            if ((descriptionHtml != null) && !string.IsNullOrWhiteSpace(descriptionHtml))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(descriptionHtml);

                // Extract img src
                message.ImgSrc = doc.DocumentNode.SelectSingleNode("//img")?.Attributes["src"]?.Value;

                // Extract comments
                message.Review = doc.DocumentNode.SelectSingleNode("//p[last()]")?.InnerText;

            }

        }

    }
}
