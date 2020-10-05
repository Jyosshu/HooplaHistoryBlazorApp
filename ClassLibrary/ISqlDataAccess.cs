using ClassLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public interface ISqlDataAccess
    {
        Task<DigitalBook> GetBookById(int bookId);
        Task<List<DigitalBook>> GetBooks();
        Task<List<DigitalBook>> GetBooksByAuthor(string authorName);
    }
}