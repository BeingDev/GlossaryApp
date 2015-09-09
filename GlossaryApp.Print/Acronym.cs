using System;

namespace GlossaryApp.Print
{
    [Serializable]
    public class Acronym
    {
        public int acronym_id { get; set; }

        public string acronym { get; set; }

        public string definition { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? ModificationDate { get; set; }

        public bool? PublishFlag { get; set; }

        public bool? DeleteFlag { get; set; }
    }
}
