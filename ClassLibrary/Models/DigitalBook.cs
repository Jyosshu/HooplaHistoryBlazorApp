using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Models
{
    public class DigitalBook
    {
        public int Id { get; set; }
        public int TitleId { get; set; }
        public string Title { get; set; }
        public int KindId { get; set; }
        public string ArtistName { get; set; } // TODO: Refactor into List<Artists>?
        public string ArtKey { get; set; }

        public Image Image { get; set; }
        public Kind Kind { get; set; }
        
        public List<Borrow> Borrows { get; set; }
    }
}
