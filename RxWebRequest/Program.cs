// From https://bitbucket.org/interlude/reactive

using System;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

namespace Reactive
{
	/// <summary>
	///     The program.
	/// </summary>
	internal class Program
	{
		// Generates events with interval that alternates between 500ms and 1000ms every 5 events
		#region Constants and Fields

		private static readonly Action<string> PrintThreadId =
			(prefix) => Console.WriteLine("{0} on ThreadId: {1}", prefix, Thread.CurrentThread.ManagedThreadId);

		private static readonly Func<string, IObservable<Unit>> ProcessUrl =
			url =>
			Observable.Start(
				() =>
				{
					var threadId = Thread.CurrentThread.ManagedThreadId;
					Console.WriteLine(
						"Thread {0} is connecting to: {1}",
						threadId,
						url);

					var wr = WebRequest.Create(new Uri(url));
					Console.WriteLine(
						"Thread {0} got {1} \n",
						threadId,
						wr.GetResponse().ResponseUri);
				});

		#endregion

		#region Public Methods

		public static string CreateConnectionUrl(string url)
		{
			return url;
		}

		public static IObservable<string> GetUrls()
		{
			var urls = new[]
				{
					"http://wikipedia.org", "http://twitter.com", "http://ebay.com", "http://grooveshark.com", "http://google.com",
					"http://msdn.com"
				};

			var q = from x in urls
					select CreateConnectionUrl(x);

			return q.ToObservable(ThreadPoolScheduler.Instance);
		}

		#endregion

		#region Methods

		private static void Main(string[] args)
		{
			var threadCount = 0;
			var scheduler = ThreadPoolScheduler.Instance;

			var query = GetUrls().Select(x => ProcessUrl(x));
			var cancel = false;

			var tokensource = new CancellationTokenSource();
			var c = tokensource.Token;

			////var a = Observable.Interval(new TimeSpan(1, 0, 0, 0, 0));
			var obs = query.Merge()
						   .ObserveOn(scheduler)
						   .SubscribeOn(scheduler);
			obs.Subscribe(
				(unit) =>
				{
					cancel = true;
				},
				(ex) => Console.WriteLine("Exception on Thread {1}:  {0}", ex.Message, Thread.CurrentThread.ManagedThreadId),
				() => Console.WriteLine("Completed"),
				c);
			tokensource.Cancel();
			Console.ReadKey();
		}

		#endregion

		public class UrlObserver : IObserver<string>
		{
			#region Public Methods

			public void OnCompleted()
			{
				Console.WriteLine("Completed");
			}

			public void OnError(Exception error)
			{
				Console.WriteLine("Error occured");
			}

			public void OnNext(string value)
			{
				var threadId = Thread.CurrentThread.ManagedThreadId;
				Console.WriteLine("Thread {0} is connecting to: {1}", threadId, value);
				var wr = WebRequest.Create(new Uri(value));
				var stream = wr.GetResponse().GetResponseStream();
				if (stream == null)
				{
					return;
				}

				Console.WriteLine("Thread {0} got {1} \n", threadId, wr.GetResponse().ResponseUri);
			}

			#endregion
		}
	}
}