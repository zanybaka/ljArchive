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

	/// <summary>
	/// Miscellaneous functions for talking to an LJ server.
	/// </summary>
	public class Server
	{
		/// <summary>
		/// Get journals a user has community access to.
		/// </summary>
		/// <param name="username">The user's username.</param>
		/// <param name="password">The user's password.</param>
		/// <param name="serverURL">The server's URL.</param>
		/// <returns></returns>
		static public string[] GetUseJournals(string username, string password, string serverURL)
		{
			ILJServer iLJ;
			XMLStructs.GetChallengeResponse gcr;
			string auth_response;
			XMLStructs.LoginParams lp;
			XMLStructs.LoginResponse lr;

			try
			{
				iLJ = LJServerFactory.Create(serverURL);
				gcr = iLJ.GetChallenge();
				auth_response = MD5Hasher.Compute(gcr.challenge + MD5Hasher.Compute(password));
				lp = new XMLStructs.LoginParams(username, "challenge", gcr.challenge, auth_response, 1, clientVersion,
				0, 0, 1, 1);
				lr = iLJ.Login(lp);
			}
			catch (CookComputing.XmlRpc.XmlRpcFaultException xfe)
			{
				throw new ExpectedSyncException(ExpectedError.InvalidPassword, xfe);
			}
			catch (System.Net.WebException we)
			{
				throw new ExpectedSyncException(ExpectedError.ServerNotResponding, we);
			}
			catch (CookComputing.XmlRpc.XmlRpcServerException xse)
			{
				throw new ExpectedSyncException(ExpectedError.ServerNotResponding, xse);
			}
			return lr.usejournals;
		}
		
		static public string GetDefaultPicURL(string journalName, string serverURL, bool community)
		{
			Uri uri = new Uri(new Uri(serverURL), string.Format(_foafpath,
			                                                       (community ? "community" : "users"),
			                                                       journalName));
			HttpWebRequest w = HttpWebRequestFactory.Create(uri.AbsoluteUri, null);
			string picURL = null;
			using (System.IO.Stream s = w.GetResponse().GetResponseStream())
			{
				System.Xml.XmlTextReader xr = new System.Xml.XmlTextReader(s);
				while (xr.Read())
				{
					if (xr.NodeType == System.Xml.XmlNodeType.Element && xr.Name == "foaf:img")
					{
						picURL = xr.GetAttribute("rdf:resource");
						break;
					}
				}
				xr.Close();
			}
			return picURL;
		}
		
		static private readonly string clientVersion = Environment.OSVersion.Platform.ToString() + "-.NET/" +
			System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(2);
		static private readonly string _foafpath = ConstReader.GetString("_foafpath");
	}
}
