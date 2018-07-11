using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadedDataRequester : MonoBehaviour {

    struct ThreadInfo
    {
        public readonly Action<object> callback;
        public readonly object parameter;

        public ThreadInfo(Action<object> callback, object parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

    static ThreadedDataRequester instance;
    Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

    void DataThread(Func<object> genData, Action<object> callback)
    {
        object data = genData();

        lock (dataQueue)
        {
            dataQueue.Enqueue(new ThreadInfo(callback, data));
        }
    }

    public static void RequestData(Func<object> genData, Action<object> callback)
    {
        ThreadStart threadStart = delegate
        {
            instance.DataThread(genData, callback);
        };

        new Thread(threadStart).Start();
    }

	void Awake () {
        instance = FindObjectOfType<ThreadedDataRequester>();
	}
	
	void Update () {
		if (dataQueue.Count > 0)
        {
            for (int i = 0; i < dataQueue.Count; i++)
            {
                ThreadInfo threadInfo = dataQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
	}
}
