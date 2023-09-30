namespace Three_Core;

public class IndexedThreadPool
{
    public delegate void ThreadAction(int index);
    private readonly Thread[] workers;
    private readonly object _lock = new ();
    private readonly int taskCount;
    private int completedTasks;
    private readonly ThreadAction _action;
    
    public IndexedThreadPool(int taskCount, int workerCount, ThreadAction action)
    {
        _action = action;
        this.taskCount = taskCount;
        workers = new Thread[workerCount];
    }

    public void RunSync()
    {
        this.completedTasks = 0;
        // First create a separate thread for each worker
        for (int i = 0; i < workers.Length; i++)
        {
            workers[i] = new Thread(Work);
            workers[i].Start();
        }
        
        // Wait for all threads to finish
        foreach (var worker in workers)
        {
            worker.Join();
        }
    }
    
    private void Work()
    {
        while (true)
        {
            lock (_lock)
            {
                if (completedTasks == taskCount)
                {
                    return;
                }

                completedTasks++;
            }

            _action(completedTasks - 1);
        }
    }
}