using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Models
{
    public class Borrow
    {
        public long Borrowed { get; set; }
        public long Returned { get; set; }

        private static DateTime StartDate
        {
            get => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
        public string BorrowedDate
        {
            get => StartDate.AddMilliseconds(Borrowed).ToLocalTime().ToString("MM/dd/yyyy");
        }

        public string ReturnedDate
        {
            get => StartDate.AddMilliseconds(Returned).ToLocalTime().ToString("MM/dd/yyyy");
        }
    }
}
