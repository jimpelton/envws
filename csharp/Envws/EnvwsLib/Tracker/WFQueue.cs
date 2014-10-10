using System.Collections.Concurrent;

namespace EnvwsLib.Tracker
{
    /// <summary>
    /// regular old producer-consumer style queue.
    /// Keeps track of finished jobs.
    /// </summary>
    public class WFQueue<T>
    {
        private BlockingCollection<T>
            m_waiting = new BlockingCollection<T>(new ConcurrentQueue<T>());

        private BlockingCollection<T>
            m_finished = new BlockingCollection<T>(new ConcurrentQueue<T>());


        public void EnqueueDone(T j)
        {
             m_finished.Add(j);
        }

        public T DequeueDone()
        {
            return m_finished.Take();
        }

        public void EnqueueWaiting(T j)
        {
            m_waiting.Add(j);
        }

        public T DequeueWaiting()
        {
            return m_waiting.Take();
        }

        public int SizeWaiting()
        {
            return m_waiting.Count;
        }

        public int SizeFinished()
        {
            return m_finished.Count;
        }
    }
}
