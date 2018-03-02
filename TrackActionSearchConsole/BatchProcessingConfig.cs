namespace TrackActionSearchConsole
{
    public class BatchProcessingConfig
    {
        public int BatchSize { get; set; } = 50000;

        public int ContactActionsDatabaseSearchMaxDegreeOfParallelism { get; set; } = -1;

        public int AccountActionsDatabaseSearchMaxDegreeOfParallelism { get; set; } = -1;
    }
}