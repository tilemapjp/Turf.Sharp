using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TurfCSTest
{
	public class Tools
	{
		static internal string[] Names { get; set; }
		static internal Assembly Assembly { get; set; }

		public static string GetResource(string resourceName)
		{
			if (Assembly == null)
			{
				Assembly = typeof(Tools).GetTypeInfo().Assembly;
				Names = Assembly.GetManifestResourceNames();
			}
			try
			{
				string path = Names.First(x => x.EndsWith(resourceName, StringComparison.CurrentCultureIgnoreCase));
				var stream = Assembly.GetManifestResourceStream(path);
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
			catch
			{
				return null;
			}
		}
	}
}
