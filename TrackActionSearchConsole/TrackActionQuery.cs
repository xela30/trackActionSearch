namespace TrackActionSearchConsole
{
    public class TrackActionQuery
    {
        public AccountQuery AccountQuery { get; set; }
        public ContactQuery ContactQuery { get; set; }
        public ActionQuery ActionQuery { get; set; } = new ActionQuery();
    }
}