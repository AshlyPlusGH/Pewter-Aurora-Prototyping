using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ash {
    /// <summary>
    /// A persistent global coroutine runner that provides a queued execution system.
    /// Ensures that coroutines run sequentially without overlapping.
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents); }}

        [Space(10)]
        
        private static CoroutineRunner instance;

        /// <summary>
        /// Queue of coroutines waiting to be executed.
        /// </summary>
        private static readonly Queue<IEnumerator> routineQueue = new Queue<IEnumerator>();

        /// <summary>
        /// Indicates whether the queue processor is currently running.
        /// </summary>
        private static bool isProcessingQueue = false;

        // ---------------------------------------------------------
        // INITIALIZATION
        // ---------------------------------------------------------

        private void Awake()
        {
            // Ensure only one instance exists and persists across scenes.
            if (instance == null)
            {
                instance = this;
                gameObject.transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // ---------------------------------------------------------
        // PUBLIC API
        // ---------------------------------------------------------

        /// <summary>
        /// Adds a coroutine to the queue.  
        /// If the queue is idle, processing begins immediately.
        /// </summary>
        public static void QueueRoutine(IEnumerator routine)
        {
            routineQueue.Enqueue(routine);

            if (!isProcessingQueue)
                instance.StartCoroutine(ProcessQueue());
        }

        /// <summary>
        /// Starts a coroutine immediately without queueing.
        /// Useful for fire‑and‑forget routines.
        /// </summary>
        public static void Run(IEnumerator routine)
        {
            instance.StartCoroutine(routine);
        }

        // ---------------------------------------------------------
        // INTERNAL QUEUE PROCESSOR
        // ---------------------------------------------------------

        /// <summary>
        /// Processes queued coroutines sequentially.
        /// Ensures only one coroutine runs at a time.
        /// </summary>
        private static IEnumerator ProcessQueue()
        {
            isProcessingQueue = true;

            while (routineQueue.Count > 0)
            {
                IEnumerator next = routineQueue.Dequeue();
                yield return instance.StartCoroutine(next);
            }

            isProcessingQueue = false;
        }
    }
}