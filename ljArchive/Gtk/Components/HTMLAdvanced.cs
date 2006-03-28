using System;
using System.IO;
using System.Collections;
using System.Threading;
using Gtk;
using Glade;

namespace EF.ljArchive.Gtk.Components
{
	public class HTMLAdvanced : HTML
	{
		private ArrayList threads;
		
		public HTMLAdvanced() : base()
		{
			threads = new ArrayList();
		}

		override protected void OnUrlRequested(string url, HTMLStream handle)
		{
			// gobble up UrlRequested event by not calling base
			UrlDownloader ud = new UrlDownloader(url, handle);
			Thread t = new Thread(new ThreadStart(ud.Go));
			threads.Add(t);
			t.Start();
		}
		
		override public void Dispose()
		{
			foreach(Thread t in threads)
				if (t.IsAlive)
					t.Abort();
			base.Dispose();
		}
		
		#region UrlDownloader
		private class UrlDownloader
		{
			public UrlDownloader(string url, HTMLStream handle)
			{
				this.url = url;
				this.handle = handle;
			}
			
			public void Go()
			{
				int n;
				byte [] buffer = new byte [8192];

				try
				{
					using (Stream s = SimpleCache.Cache.GetStream(url))
						while ((n = s.Read(buffer, 0, 8192)) != 0)
							handle.Write(buffer, n);
				}
				catch (System.Net.WebException) {} // don't care about 404's and such
				catch (System.Threading.ThreadAbortException) {} // stream is gone
				// handle.Close(HTMLStreamStatus.Ok);   why is this breaking gtkhtml?
			}
			
			public string url;
			public HTMLStream handle;
		}
		#endregion

		#region SimpleCache
		private class SimpleCache : IDisposable
		{
		    // Explicit static constructor to tell C# compiler
	    	// not to mark type as beforefieldinit
		    static SimpleCache()
		    {
		    }

	    	public SimpleCache()
		    {
		    	Directory.CreateDirectory(_tmpdir);
		    }
		    
		    ~SimpleCache()
		    {
		    	Dispose();
		    }
		    
		    public Stream GetStream(string url)
		    {
		    	Uri uri = new Uri(url);

		    	if (uri.Scheme == "http")
		    	{
		    		lock (urimap)
		    		{
			    		if (!urimap.ContainsKey(uri.AbsoluteUri))
			    		{
			    			string filename = System.IO.Path.Combine(_tmpdir, urimap.Count.ToString());
			    			int n;
							byte [] buffer = new byte [8192];
							using (Stream sin = System.Net.WebRequest.Create(uri).GetResponse().GetResponseStream())
								using (Stream sout = new FileStream(filename, FileMode.Create))
				    				while ((n = sin.Read (buffer, 0, 8192)) != 0)
				    					sout.Write(buffer, 0, n);
			    			urimap.Add(uri.AbsoluteUri, filename);
		    			}
	    			}
		    		return new FileStream((string) urimap[uri.AbsoluteUri], FileMode.Open, FileAccess.Read);
		    	}
		    	return System.Net.WebRequest.CreateDefault(uri).GetResponse().GetResponseStream();
		    }
		    
		    public void Dispose()
		    {
		    	Directory.Delete(_tmpdir, true);
		    }

		    static public SimpleCache Cache
	    	{
	    	    get { return instance; }
	    	}
	    	
	    	private Hashtable urimap = new Hashtable();
	    	static private readonly string _tmpdir = "tmp";
	    	static readonly SimpleCache instance = new SimpleCache();
		}
		#endregion
	}
}