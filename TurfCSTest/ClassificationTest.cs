using System;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using NUnit.Framework;
using TurfCS;

namespace TurfCSTest
{
	[TestFixture()]
	public class ClassificationTest
	{
		[Test()]
		public void Nearest()
		{
			var pt = JsonConvert.DeserializeObject<Feature>(Tools.GetResource("nearest.pt.geojson"));
			var pts = JsonConvert.DeserializeObject<FeatureCollection>(Tools.GetResource("nearest.pts.geojson"));

			var closestPt = Turf.Nearest(pt, pts);

			Assert.AreEqual(closestPt.Geometry.Type, GeoJSONObjectType.Point, "should be a point");
			Assert.AreEqual(((GeographicPosition)((Point)closestPt.Geometry).Coordinates).Longitude, -75.33, "lon -75.33");
			Assert.AreEqual(((GeographicPosition)((Point)closestPt.Geometry).Coordinates).Latitude, 39.44, "lat 39.44");
		}
	}
}
