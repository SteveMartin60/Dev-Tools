using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;

namespace reddit_fetch
{
    /// <summary>
    /// Provides helper methods for working with the image hash database.
    /// Handles storage, lookup, and cleanup of hashes.
    /// </summary>
    public static class HashDatabaseHelper
    {
        public static AppConfig Config { get; set; } = null!;

        private static readonly AverageHash HashAlgorithm = new AverageHash();

        /// <summary>
        /// Ensures the database exists and has the proper schema.
        /// </summary>
        public static void EnsureDatabase()
        {
            var dbPath = Config.DatabasePath;

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                """
                CREATE TABLE IF NOT EXISTS ImageHashes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FilePath TEXT NOT NULL UNIQUE,
                    Hash TEXT NOT NULL,
                    LastSeenUtc TEXT NOT NULL
                );
                """;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Adds or updates an image hash in the database.
        /// </summary>
        public static void UpsertHash(string filePath, string hash)
        {
            EnsureDatabase();

            using var connection = new SqliteConnection($"Data Source={Config.DatabasePath}");
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                """
                INSERT INTO ImageHashes (FilePath, Hash, LastSeenUtc)
                VALUES ($filePath, $hash, $lastSeen)
                ON CONFLICT(FilePath) DO UPDATE SET
                    Hash = excluded.Hash,
                    LastSeenUtc = excluded.LastSeenUtc;
                """;

            cmd.Parameters.AddWithValue("$filePath", filePath);
            cmd.Parameters.AddWithValue("$hash", hash);
            cmd.Parameters.AddWithValue("$lastSeen", DateTime.UtcNow.ToString("u"));

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Returns true if the image is a near-duplicate of one already stored.
        /// </summary>
        public static bool IsNearDuplicate(string filePath, string hash, int maxDistance = 5)
        {
            EnsureDatabase();

            using var connection = new SqliteConnection($"Data Source={Config.DatabasePath}");
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT FilePath, Hash FROM ImageHashes;";
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var existingHash = reader.GetString(1);
                var distance = CompareHashes(existingHash, hash);
                if (distance <= maxDistance)
                {
                    Logger.LogInfo($"Near-duplicate found. Existing: {reader.GetString(0)} | New: {filePath} | Distance: {distance}");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Refreshes existence of files in DB, removing entries for missing files.
        /// </summary>
        public static void RefreshFileExistence()
        {
            EnsureDatabase();

            using var connection = new SqliteConnection($"Data Source={Config.DatabasePath}");
            connection.Open();

            var toDelete = new List<long>();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, FilePath FROM ImageHashes;";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetInt64(0);
                    var path = reader.GetString(1);
                    if (!File.Exists(path))
                        toDelete.Add(id);
                }
            }

            foreach (var id in toDelete)
            {
                using var del = connection.CreateCommand();
                del.CommandText = "DELETE FROM ImageHashes WHERE Id = $id;";
                del.Parameters.AddWithValue("$id", id);
                del.ExecuteNonQuery();
                Logger.LogInfo($"Removed missing hash entry Id={id}");
            }
        }

        private static int CompareHashes(string a, string b)
        {
            try
            {
                ulong ha = ulong.Parse(a);
                ulong hb = ulong.Parse(b);

                ulong diff = ha ^ hb;        // XOR highlights differing bits
                int distance = 0;

                // Count set bits (Hamming distance)
                while (diff != 0)
                {
                    diff &= (diff - 1);     // remove lowest set bit
                    distance++;
                }

                return distance;
            }
            catch
            {
                return int.MaxValue;
            }
        }
    }
}
