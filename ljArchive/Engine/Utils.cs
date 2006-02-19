using System;
using System.Resources;
using System.Drawing;
using System.Reflection;
using CookComputing.XmlRpc;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace EF.ljArchive.Engine
{
	/// <summary>
	/// Factory for creating <see cref="ILJServer"/> classes.
	/// </summary>
	internal class LJServerFactory
	{
		/// <summary>
		/// This is a static class.
		/// </summary>
		private LJServerFactory() {}

		internal static ILJServer Create(string url)
		{
			ILJServer iLJ = (ILJServer) XmlRpcProxyGen.Create(typeof(ILJServer));
			XmlRpcClientProtocol xpc = (CookComputing.XmlRpc.XmlRpcClientProtocol) iLJ;
			xpc.UserAgent = _useragent;
			Uri baseUri = new Uri(url), uri = new Uri(baseUri, _xmlrpcpath);
			
			xpc.Url =  uri.AbsoluteUri;
			xpc.ProtocolVersion = new Version(1, 0);
			xpc.KeepAlive = false;
			xpc.RequestEncoding = new System.Text.UTF8Encoding();
			xpc.XmlEncoding = new System.Text.UTF8Encoding();
			return iLJ;
		}

		static private readonly string _xmlrpcpath = ConstReader.GetString("_xmlrpcpath");
		static private readonly string _useragent = ConstReader.GetString("_useragent");
	}

	/// <summary>
	/// Factory for creating <see cref="HttpWebRequest"/> classes.
	/// </summary>
	internal class HttpWebRequestFactory
	{
		/// <summary>
		/// This is a static class.
		/// </summary>
		private HttpWebRequestFactory() {}

		internal static HttpWebRequest Create(string url, string ljsession)
		{
			Uri uri = new Uri(url);
			HttpWebRequest wr = (HttpWebRequest) WebRequest.Create(uri.AbsoluteUri);
			wr.ProtocolVersion = new Version(1, 0);
			wr.KeepAlive = false;
			wr.UserAgent = _useragent;
			if (ljsession != null && ljsession.Length > 0)
			{
				wr.CookieContainer = new CookieContainer();
				// Old-style session cookies, just in case.
				wr.CookieContainer.Add(new Cookie("ljsession", ljsession, "/", uri.Host));
				// New-style session cookies.
				wr.CookieContainer.Add(new Cookie("ljmastersession", ljsession, "/", uri.Host));
				wr.CookieContainer.Add(new Cookie("ljloggedin", ljsession.Substring(ljsession.IndexOf(":") + 1, ljsession.LastIndexOf(":") - ljsession.IndexOf(":") - 1), "/", uri.Host));
			}
			return wr;
		}
		static private readonly string _useragent = ConstReader.GetString("_useragent");
	}

	/// <summary>
	/// Reads internal global consts.
	/// </summary>
	internal class ConstReader
	{
		/// <summary>
		/// This is a static class.
		/// </summary>
		private ConstReader() {}

		static ConstReader()
		{
			rm = new ResourceManager(_constsresource, Assembly.GetExecutingAssembly());
		}

		internal static object GetObject(string name)
		{
			return rm.GetObject(name);
		}

		internal static int GetInt(string name)
		{
			return (int) GetObject(name);
		}

		internal static string GetString(string name)
		{
			return (string) GetObject(name);
		}

		static private ResourceManager rm;
		private const string _constsresource = "EF.ljArchive.Engine.res.Consts";
	}

	internal class MD5Hasher
	{
		/// <summary>
		/// This is a static class.
		/// </summary>
		private MD5Hasher() {}

		static MD5Hasher()
		{
			md5 = new MD5CryptoServiceProvider();
		}

		static public string Compute(string plainText)
		{
			byte[] plainTextBytes = Encoding.ASCII.GetBytes(plainText);
			byte[] hashBytes = md5.ComputeHash(plainTextBytes);
			StringBuilder sb = new StringBuilder();
			foreach (byte hashByte in hashBytes)
				sb.Append(Convert.ToString(hashByte, 16).PadLeft(2, '0'));
			return sb.ToString();
		}

		static private MD5CryptoServiceProvider md5;
	}
}
