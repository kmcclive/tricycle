namespace Tricycle.Models.Jobs
{
    public class TranscodedOutputStream<T> : OutputStream
    {
        public T Format { get; set; }
    }
}
