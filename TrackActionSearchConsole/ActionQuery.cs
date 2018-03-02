using System;

namespace TrackActionSearchConsole
{
    public class ActionQuery
    {
        public int CompanyId { get; set; }
        public int? Top { get; set; }

        public DateTime? Min;
        public DateTime? Max;

        public bool Accepted;

        public int[] InternalParticipants;

        public ActionTeamQuery TeamQuery { get; set; }
    }
}