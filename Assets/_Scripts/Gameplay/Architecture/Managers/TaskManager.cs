using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Managers{

    /// TaskManager.cs
    /// https://raw.githubusercontent.com/krockot/Unity-TaskManager/master/TaskManager.cs
    /// This is a convenient coroutine API for Unity.
    ///
    /// Example usage:
    ///   IEnumerator MyAwesomeTask()
    ///   {
    ///       while(true) {
    ///           // ...
    ///           yield return null;
    ////      }
    ///   }
    ///
    ///   IEnumerator TaskKiller(float delay, Task t)
    ///   {
    ///       yield return new WaitForSeconds(delay);
    ///       t.Stop();
    ///   }
    ///
    ///   // From anywhere
    ///   Task my_task = new Task(MyAwesomeTask());
    ///   new Task(TaskKiller(5, my_task));
    ///
    /// The code above will schedule MyAwesomeTask() and keep it running
    /// concurrently until either it terminates on its own, or 5 seconds elapses
    /// and triggers the TaskKiller Task that was created.

    /// A Task object represents a coroutine.  Tasks can be started, paused, and stopped.
    /// It is an error to attempt to start a task that has been stopped or which has
    /// naturally terminated.
    public class Task
    {
        TaskState _task;
        /// Returns true if and only if the coroutine is running.  Paused tasks
        /// are considered to be running.
        public bool Running
        {
            get
            {
                return _task.Running;
            }
        }

        /// Returns true if and only if the coroutine is currently paused.
        public bool Paused
        {
            get
            {
                return _task.Paused;
            }
        }

        /// Delegate for termination subscribers.  manual is true if and only if
        /// the coroutine was stopped with an explicit call to Stop().
        public delegate void FinishedHandler(bool manual);

        /// Termination event.  Triggered when the coroutine completes execution.
        public event FinishedHandler Finished;

        /// Creates a new Task object for the given coroutine.
        ///
        /// If autoStart is true (default) the task is automatically started
        /// upon construction.
        public Task(IEnumerator c, bool autoStart = true)
        {
            _task = TaskManager.CreateTask(c);
            _task.Finished += TaskFinished;
            if (autoStart)
                Start();
        }

        /// Begins execution of the coroutine
        public void Start()
        {
            _task.Start();
        }

        /// Discontinues execution of the coroutine at its next yield.
        public void Stop()
        {
            _task.Stop();
        }

        public void Pause()
        {
            _task.Pause();
        }

        public void Unpause()
        {
            _task.Unpause();
        }

        void TaskFinished(bool manual)
        {
            FinishedHandler handler = Finished;
            if (handler != null)
                handler(manual);
        }

    }

    public class TaskState
    {
        IEnumerator _coroutine;
        bool _running;
        bool _paused;
        bool _stopped;


        public bool Running
        {
            get
            {
                return _running;
            }
        }

        public bool Paused
        {
            get
            {
                return _paused;
            }
        }

        public delegate void FinishedHandler(bool manual);
        public event FinishedHandler Finished;

        public TaskState(IEnumerator c)
        {
            _coroutine = c;
        }

        public void Pause()
        {
            _paused = true;
        }

        public void Unpause()
        {
            _paused = false;
        }

        public void Start()
        {
            if (_running)
            {
                return;
            }
            _running = true;
            TaskManager.Instance.StartCoroutine(CallWrapper());
        }

        public void Stop()
        {
            _stopped = true;
            _running = false;
        }

        IEnumerator CallWrapper()
        {
            yield return null;
            IEnumerator e = _coroutine;
            while (_running)
            {
                if (_paused)
                    yield return null;
                else
                {
                    if (e != null && e.MoveNext())
                    {
                        yield return e.Current;
                    }
                    else
                    {
                        _running = false;
                    }
                }
            }

            FinishedHandler handler = Finished;
            if (handler != null)
                handler(_stopped);
        }
    }

    public class TaskManager : GameManager<TaskManager>, IManager
    {
        private WaitForSecondsPool _waitForSecondsPool;

        public WaitForSecondsPool WaitForSecondsPool { get { return _waitForSecondsPool; } }

        public static TaskState CreateTask(IEnumerator coroutine)
        {
            return new TaskState(coroutine);
        }

        public void ManagedPreInGameLoad()
        {
            base.Awake();

            _waitForSecondsPool = new WaitForSecondsPool();
        }

        public void ManagedPostInGameLoad()
        {
        }

        public void ManagedPreMainMenuLoad()
        {
        }

        public void ManagedPostMainMenuLoad()
        {
        }

        public static explicit operator TaskManager(GameObject v)
        {
            throw new NotImplementedException();
        }
    }

}
