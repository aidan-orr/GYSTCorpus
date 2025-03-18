using GYSTCorpus;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GYSTCorpus;
public static class ParallelTaskExecutor
{
	/// <summary>
	/// Processes many items in parallel and uses a separate thread to save the results.
	/// </summary>
	/// <typeparam name="TItem"></typeparam>
	/// <typeparam name="TSave"></typeparam>
	/// <param name="items">The items to be processed.</param>
	/// <param name="processAction">The action to process each item. Atomic integer for keeping track of completed operations</param>
	/// <param name="saveAction">The action to use when saving the items. This will be called repeatedly anytime there are new items to save.</param>
	/// <param name="maxTasks">The maximum number of tasks that can be executing at once.</param>
	/// <param name="sleepDelay">The number of milliseconds to delay when waiting.</param>
	public static void ForEachWithSave<TItem, TSave>(IEnumerable<TItem> items, Action<TItem, AtomicInteger, ConcurrentQueue<TSave>, CancellationToken> processAction, Action<ConcurrentQueue<TSave>, CancellationToken> saveAction, int maxTasks = 64, int sleepDelay = 10, CancellationToken cancellation = default)
	{
		ConcurrentQueue<TSave> saveItems = [];

		bool finished = false;

		Thread saveThread = new Thread(() =>
		{
			while (!cancellation.IsCancellationRequested && (!finished || !saveItems.IsEmpty))
			{
				if (saveItems.IsEmpty)
				{
					Thread.Sleep(sleepDelay);
					continue;
				}

				saveAction(saveItems, cancellation);
			}
		})
		{
			Priority = ThreadPriority.AboveNormal,
			IsBackground = true
		};

		saveThread.Start();

		AtomicInteger processed = 0;

		ParallelOptions options = new ParallelOptions
		{
			CancellationToken = cancellation,
			MaxDegreeOfParallelism = maxTasks
		};

		Parallel.ForEach(items, options, (item, _, index) => processAction(item, processed, saveItems, cancellation));

		finished = true;
		saveThread.Join();
	}

	/// <summary>
	/// Processes many items in parallel and uses a separate thread to save the results.
	/// </summary>
	/// <typeparam name="TItem"></typeparam>
	/// <typeparam name="TSave"></typeparam>
	/// <param name="items">The items to be processed.</param>
	/// <param name="processAction">The action to process each item. Atomic integer for keeping track of completed operations</param>
	/// <param name="saveAction">The action to use when saving the items. This will be called repeatedly anytime there are new items to save.</param>
	/// <param name="maxTasks">The maximum number of tasks that can be executing at once.</param>
	/// <param name="sleepDelay">The number of milliseconds to delay when waiting.</param>
	public static void ForEachWithSave<TItem, TSave>(IEnumerable<TItem> items, Action<TItem, AtomicInteger, ConcurrentQueue<TSave>, long, CancellationToken> processAction, Action<ConcurrentQueue<TSave>, CancellationToken> saveAction, int maxTasks = 64, int sleepDelay = 10, CancellationToken cancellation = default)
	{
		ConcurrentQueue<TSave> saveItems = [];

		bool finished = false;

		Thread saveThread = new Thread(() =>
		{
			while (!cancellation.IsCancellationRequested && (!finished || !saveItems.IsEmpty))
			{
				if (saveItems.IsEmpty)
				{
					Thread.Sleep(sleepDelay);
					continue;
				}

				saveAction(saveItems, cancellation);
			}
		})
		{
			Priority = ThreadPriority.AboveNormal,
			IsBackground = true
		};

		saveThread.Start();

		AtomicInteger processed = 0;

		ParallelOptions options = new ParallelOptions
		{
			CancellationToken = cancellation,
			MaxDegreeOfParallelism = maxTasks
		};

		Parallel.ForEach(items, options, (item, _, index) => processAction(item, processed, saveItems, index, cancellation));

		finished = true;
		saveThread.Join();
	}
}
