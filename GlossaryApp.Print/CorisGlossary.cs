using System;

namespace GlossaryApp.Print
{
    [Serializable]
    public class CorisGlossary
    {
        public int TermID { get; set; }

        public string Term { get; set; }

        public bool? ItalicizeTerm { get; set; }

        public string Definition { get; set; }

        public string Image { get; set; }

        public string Alt_tag { get; set; }

        public string Caption { get; set; }

        public string Source { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? ModificationDate { get; set; }

        public string Editor { get; set; }

        public bool? PublishFlag { get; set; }

        public bool? DeleteFlag { get; set; }
    }
}
