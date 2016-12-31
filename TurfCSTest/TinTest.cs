using System;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using NUnit.Framework;
using TurfCS;

namespace TurfCSTest
{
	[TestFixture()]	
	public class TinTest
	{
		[Test()]
		public void Tin()
		{
			var points = JsonConvert.DeserializeObject<FeatureCollection>(Tools.GetResource("Points.geojson"));
			var point = JsonConvert.DeserializeObject<Feature>(Tools.GetResource("Point.geojson"));
			var tinned = Turf.Tin(points, "elevation");

			Assert.AreEqual(tinned.Features[0].Geometry.Type, GeoJSONObjectType.Polygon);
			//Assert.AreEqual(tinned.Features.Count, 24);
			var res = JsonConvert.SerializeObject(tinned);
			Console.WriteLine(JsonConvert.SerializeObject(tinned));

			//fs.writeFileSync(__dirname + '/test/Tin.geojson', JSON.stringify(tinned));
			//t.end();
		}
	}
}
