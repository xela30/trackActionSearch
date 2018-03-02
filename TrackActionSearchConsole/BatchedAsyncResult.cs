using System.Threading.Tasks;

namespace TrackActionSearchConsole
{
    public class BatchedAsyncResult<T>
    {
        public BatchedAsyncResult(Task<Task<T>[]> resultTasks)
        {
            Tasks = resultTasks;
        }

        public Task<Task<T>[]> Tasks { get; }
    }
}