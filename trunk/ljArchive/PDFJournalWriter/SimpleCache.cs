using System;
using System.Collections;
using System.IO;

namespace EF.ljArchive.PDF
{
	class SimpleCache : IDisposable
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
}
