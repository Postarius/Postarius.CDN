using System;

namespace Domain
{
    public class Media : EntityBase
    {
        public string RawPath { get; set; }
        public MediaState State { get; set; }
    }

    public enum MediaState
    {
        Unprocessed,
        Processed
    }
}