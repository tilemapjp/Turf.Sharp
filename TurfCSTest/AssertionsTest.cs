using System;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using NUnit.Framework;
using TurfCS;

namespace TurfCSTest
{
	[TestFixture()]
	public class AssertionsTest
	{
		[Test()]
		public void Bbox()
		{
			var fc = JsonConvert.DeserializeObject<FeatureCollection>(Tools.GetResource("bbox.FeatureCollection.geojson"));
			var pt = JsonConvert.DeserializeObject<Feature>(Tools.GetResource("bbox.Point.geojson"));
			var line = JsonConvert.DeserializeObject<LineString>(Tools.GetResource("bbox.LineString.geojson"));
			var poly = JsonConvert.DeserializeObject<Feature>(Tools.GetResource("bbox.Polygon.geojson"));
			var multiLine = JsonConvert.DeserializeObject<MultiLineString>(Tools.GetResource("bbox.MultiLineString.geojson"));
			var multiPoly = JsonConvert.DeserializeObject<MultiPolygon>(Tools.GetResource("bbox.MultiPolygon.geojson"));

			// FeatureCollection
			var fcExtent = Turf.Bbox(fc);
			Assert.AreEqual(fcExtent[0], 20);
			Assert.AreEqual(fcExtent[1], -10);
			Assert.AreEqual(fcExtent[2], 130);
			Assert.AreEqual(fcExtent[3], 4);

			// Point
			var ptExtent = Turf.Bbox(pt);
			Assert.AreEqual(ptExtent[0], 102);
			Assert.AreEqual(ptExtent[1], 0.5);
			Assert.AreEqual(ptExtent[2], 102);
			Assert.AreEqual(ptExtent[3], 0.5);

			// Line
			var lineExtent = Turf.Bbox(line);
			Assert.AreEqual(lineExtent[0], 102);
			Assert.AreEqual(lineExtent[1], -10);
			Assert.AreEqual(lineExtent[2], 130);
			Assert.AreEqual(lineExtent[3], 4);

			// Polygon
			var polyExtent = Turf.Bbox(poly);
			Assert.AreEqual(polyExtent[0], 100);
			Assert.AreEqual(polyExtent[1], 0);
			Assert.AreEqual(polyExtent[2], 101);
			Assert.AreEqual(polyExtent[3], 1);

			// MultiLineString
			var multiLineExtent = Turf.Bbox(multiLine);
			Assert.AreEqual(multiLineExtent[0], 100);
			Assert.AreEqual(multiLineExtent[1], 0);
			Assert.AreEqual(multiLineExtent[2], 103);
			Assert.AreEqual(multiLineExtent[3], 3);

			// MultiPolygon
			var multiPolyExtent = Turf.Bbox(multiPoly);
			Assert.AreEqual(multiPolyExtent[0], 100);
			Assert.AreEqual(multiPolyExtent[1], 0);
			Assert.AreEqual(multiPolyExtent[2], 103);
			Assert.AreEqual(multiPolyExtent[3], 3);
		}

		[Test()]
		public void Circle()
		{
			var center = Turf.Point(new double[] { -75.343, 39.984 });
			double radius = 5;
			int steps = 10;

			var polygon = Turf.Circle(center, radius, steps, "kilometers");
			var point1 = Turf.Destination(center, radius - 1, 45, "kilometers");
			var point2 = Turf.Destination(center, radius + 1, 135, "kilometers");

			Assert.AreEqual(Turf.Inside(point1, polygon), true, "point is inside the polygon");
    		Assert.AreEqual(Turf.Inside(point2, polygon), false, "point is outside the polygon");
		}
	}
}
