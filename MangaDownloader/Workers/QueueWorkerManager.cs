﻿using MangaDownloader.Utils;
using MangaDownloader.Workers.Data;
using System;
using System.Collections.Generic;
using System.Timers;

namespace MangaDownloader.Workers
{
    class QueueWorkerManager
    {
        public event Action AllWorkersStopped;
        public bool IsBusy = false;

        const int MANUAL_WORKER_LIMIT = 10;
        static QueueWorkerManager instance;
        Timer taskTimer;
        bool IsStopping = false;
        int workerLimit = 3;
        List<Task> taskList = new List<Task>();
        List<IWorker> workerList = new List<IWorker>();
        List<IWorker> manualWorkerList = new List<IWorker>();

        private QueueWorkerManager()
        {
            taskTimer = new Timer(1000);
            taskTimer.AutoReset = false;
            taskTimer.Elapsed += taskTimer_Elapsed;
        }

        public static QueueWorkerManager GetInstance()
        {
            if (instance == null)
                instance = new QueueWorkerManager();
            return instance;
        }

        public List<Task> GetTaskList()
        {
            return taskList;
        }

        public void StartQueue(int workerLimit, WorkerHandlers handlers)
        {
            if (!IsBusy)
            {
                this.workerLimit = workerLimit;

                if (workerList.Count < workerLimit)
                    for (int i = workerList.Count + 1; i <= workerLimit; i++)
                        workerList.Add(WorkerFactory.CreateWorker(handlers));

                IsBusy = true;
                IsStopping = false;
                taskTimer.Start();
            }
        }

        public void StopQueue()
        {
            if (IsBusy)
            {
                for (int i = workerList.Count - 1; i >= 0; i--)
                    workerList[i].Stop();

                IsStopping = true;
            }
        }

        public void Download(Task task, WorkerHandlers handlers)
        {
            int totalRunningManualWorkers = CountRunningManualWorkers();
            if (totalRunningManualWorkers < MANUAL_WORKER_LIMIT && SomeRules.CanDownloadTask(task.Status))
            {
                IWorker worker = GetFreeManualWorker();
                if (worker == null)
                {
                    worker = WorkerFactory.CreateWorker(handlers);
                    manualWorkerList.Add(worker);
                }
                worker.Start(task);
            }
        }

        public void Stop(Task task)
        {
            for (int i = workerList.Count - 1; i >= 0; i--)
            {
                IWorker w = workerList[i];
                if (w.GetTask() != null && w.GetTask().Equals(task))
                    w.Stop();
            }

            for (int i = manualWorkerList.Count - 1; i >= 0; i--)
            {
                IWorker mw = manualWorkerList[i];
                if (mw.GetTask() != null && mw.GetTask().Equals(task))
                    mw.Stop();
            }
        }

        void taskTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int totalRunningWorkers = CountRunningWorkers();
            Task task = taskList.Find(x => x.Status == Enums.TaskStatus.QUEUED);
            bool canStop = IsStopping && totalRunningWorkers == 0;
            bool noTask = task == null && totalRunningWorkers == 0;

            if (noTask || canStop)
            {
                IsBusy = false;
                IsStopping = false;
                CallEventSafely(AllWorkersStopped);
                return;
            }
            else if (!IsStopping)
            {
                while (task != null && totalRunningWorkers < workerLimit)
                {
                    IWorker worker = GetFreeWorker();
                    if (worker != null) worker.Start(task);
                    totalRunningWorkers = CountRunningWorkers();
                    task = taskList.Find(x => x.Status == Enums.TaskStatus.QUEUED);
                }
            }

            taskTimer.Start();
        }

        private int CountRunningWorkers()
        {
            int count = 0;
            for (int i = workerList.Count - 1; i >= 0; i--)
                if (workerList[i].IsBusy())
                    count++;

            return count;
        }

        private int CountRunningManualWorkers()
        {
            int count = 0;
            for (int i = manualWorkerList.Count - 1; i >= 0; i--)
                if (manualWorkerList[i].IsBusy())
                    count++;

            return count;
        }

        private IWorker GetFreeWorker()
        {
            for (int i = workerList.Count - 1; i >= 0; i--)
                if (!workerList[i].IsBusy())
                    return workerList[i];

            return null;
        }

        private IWorker GetFreeManualWorker()
        {
            for (int i = manualWorkerList.Count - 1; i >= 0; i--)
                if (!manualWorkerList[i].IsBusy())
                    return manualWorkerList[i];

            return null;
        }

        private void CallEventSafely(Action eventAction)
        {
            if (eventAction != null)
                eventAction.Invoke();
        }
    }
}
