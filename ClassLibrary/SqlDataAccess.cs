using ClassLibrary.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.IO;

namespace ClassLibrary
{
    public class SqlDataAccess : ISqlDataAccess
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SqlDataAccess> _log;
        public string ConnectionStringName { get; set; } = "DigitalBooks_Docker";

        public SqlDataAccess(IConfiguration config, ILogger<SqlDataAccess> log)
        {
            _config = config;
            _log = log;
        }

        public async Task<List<Artist>> GetArtists()
        {
            string connectionString = _config.GetConnectionString(ConnectionStringName);
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var data = await connection.QueryAsync<Artist>("SELECT * FROM Artist");
                return data.ToList();
            }
        }

        public async Task<List<DigitalBook>> GetBooks()
        {
            string connectionString = _config.GetConnectionString(ConnectionStringName);
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var data = await connection.QueryAsync<DigitalBook, Image, Kind, DigitalBook>(getBooksQuery + " ORDER BY di.Id",
                    map: (digitalBook, image, kind) =>
                    {
                        digitalBook.Image = image;
                        digitalBook.Kind = kind;
                        return digitalBook;
                    });

                return data.ToList();
            }
        }

        public async Task<DigitalBook> GetBookById(int bookId)
        {
            try
            {
                string remoteUrl = _config["RemoteUrl"];
                string connectionString = _config.GetConnectionString(ConnectionStringName);
                using (IDbConnection connection = new SqlConnection(connectionString))
                {
                    var data = await connection.QueryAsync<DigitalBook, Image, Kind, DigitalBook>(getBooksQuery + " WHERE di.Id = @BookId",
                        map: (digitalBook, image, kind) =>
                        {
                            digitalBook.RemoteURL = Path.Combine(remoteUrl, digitalBook.TitleId.ToString());
                            digitalBook.Image = image;
                            digitalBook.Kind = kind;
                            return digitalBook;
                        }, new { BookId = bookId });

                    foreach (var book in data)
                    {
                        book.Borrows = await GetBorrowsByTitleId(book.TitleId);
                    }
                    
                    return data.FirstOrDefault();
                }
            }
            catch (DbException de)
            {
                _log.LogError(de, de.Message);
                return null;
            }
        }

        public async Task<List<DigitalBook>> GetBooksByAuthor(string authorName)
        {
            string connectionString = _config.GetConnectionString(ConnectionStringName);
            // TODO: Sanitize user input string
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var data = await connection.QueryAsync<DigitalBook, Image, Kind, DigitalBook>(getBooksQuery + " WHERE LEFT(di.ArtistName, @InputLength) = @ArtistName ORDER BY di.id",
                    map: (digitalBook, image, kind) =>
                    {
                        digitalBook.Image = image;
                        digitalBook.Kind = kind;
                        return digitalBook;
                    }, new { artistName = authorName, InputLength = authorName.Length });

                return data.ToList();
            }
        }

        private async Task<List<Borrow>> GetBorrowsByTitleId(long titleId)
        {
            string connectionString = _config.GetConnectionString(ConnectionStringName);
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var data = await connection.QueryAsync<Borrow>("SELECT Borrowed, Returned FROM Borrows WHERE TitleId = @TitleId", new { TitleId = titleId });
                return data.ToList();
            }
        }

        private static readonly string getBooksQuery = @"SELECT di.Id
, di.TitleId
, di.Title
, di.KindId
, di.ArtistName
, di.ArtKey
, i.Id
, i.AltText
, i.ArtKey
, i.RemoteUrl
, ki.Id
, ki.Name
, ki.Singular
, ki.Plural
FROM DigitalItem di
INNER JOIN Images i ON i.ArtKey = di.ArtKey
INNER JOIN Kind ki ON ki.Id = di.KindId";
    }
}
