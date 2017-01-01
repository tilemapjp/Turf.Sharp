using System;
using System.Collections.Generic;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using NUnit.Framework;
using TurfCS;

namespace TurfCSTest
{
	[TestFixture()]
	public class MeasurementTest
	{
		[Test()]
		public void Bearing()
		{
			var pt1 = Turf.Point(new double[] { -75.4, 39.4 });
			var pt2 = Turf.Point(new double[] { -75.534, 39.123 });

			var bear = Turf.Bearing(pt1, pt2);
			Assert.AreEqual(bear, -159.42, 0.01, "should return the correct bearing");
		}

		[Test()]
		public void Destination()
		{
			var pt1 = Turf.Point(new double[] { -75.0, 39.0 });
			double dist = 100;
			double bear = 180;

			var pt2 = Turf.Destination(pt1, dist, bear, "kilometers");
			var ptgeom = (Point)pt2.Geometry;
			var prcoord = (GeographicPosition)ptgeom.Coordinates;

			Assert.AreEqual(prcoord.Longitude, -75, 0.001, "returns the correct point");
			Assert.AreEqual(prcoord.Latitude, 38.10096062273525, 0.001, "returns the correct point");
			Assert.AreEqual(ptgeom.Type, GeoJSONObjectType.Point, "returns the correct point");
		}

		[Test()]
		public void Distance()
		{
			var pt1 = Turf.Point(new double[] { -75.343, 39.984 });
			var pt2 = Turf.Point(new double[] { -75.534, 39.123 });

			Assert.AreEqual(Turf.Distance(pt1, pt2, "miles"), 60.37218405837491, 0.00001, "miles");
			Assert.AreEqual(Turf.Distance(pt1, pt2, "nauticalmiles"), 52.461979624130436, 0.00001, "miles");
			Assert.AreEqual(Turf.Distance(pt1, pt2, "kilometers"), 97.15957803131901, 0.00001, "kilometers");
			Assert.AreEqual(Turf.Distance(pt1, pt2, "kilometres"), 97.15957803131901, 0.00001, "kilometres");
			Assert.AreEqual(Turf.Distance(pt1, pt2, "radians"), 0.015245501024842149, 0.00001, "radians");
			Assert.AreEqual(Turf.Distance(pt1, pt2, "degrees"), 0.8735028650863799, 0.00001, "degrees");
			Assert.AreEqual(Turf.Distance(pt1, pt2), 97.15957803131901, 0.00001, "default=kilometers");		
		}

		[Test()]
		public void Along()
		{
			var line = JsonConvert.DeserializeObject<Feature>(Tools.GetResource("fixtures_dc-line.geojson"));

			var pt1 = Turf.Along(line, 1, "miles");
			var pt2 = Turf.Along((LineString)line.Geometry, 1.2, "miles");
			var pt3 = Turf.Along(line, 1.4, "miles");
			var pt4 = Turf.Along((LineString)line.Geometry, 1.6, "miles");
			var pt5 = Turf.Along(line, 1.8, "miles");
			var pt6 = Turf.Along((LineString)line.Geometry, 2, "miles");
			var pt7 = Turf.Along(line, 100, "miles");
			var pt8 = Turf.Along((LineString)line.Geometry, 0, "miles");
			var fc = new FeatureCollection(new List<Feature>() { pt1, pt2, pt3, pt4, pt5, pt6, pt7, pt8 });

			foreach (var f in fc.Features) {
				Assert.AreEqual(f.Type, GeoJSONObjectType.Feature);
				Assert.AreEqual(f.Geometry.Type, GeoJSONObjectType.Point);
			}
			Assert.AreEqual(fc.Features.Count, 8);
			var exp = (GeographicPosition)((Point)fc.Features[7].Geometry).Coordinates;
			var act = (GeographicPosition)((Point)pt8.Geometry).Coordinates;
			Assert.AreEqual(exp.Longitude, act.Longitude);
			Assert.AreEqual(exp.Latitude, act.Latitude);
		}

		[Test()]
		public void BboxPolygon()
		{
			var poly = Turf.BboxPolygon(new List<double>() { 0, 0, 10, 10 });

			Assert.AreEqual(poly.Geometry.Type, GeoJSONObjectType.Polygon, "should be a Polygon geometry type");

			var coords = ((Polygon)poly.Geometry).Coordinates[0].Coordinates;
			Assert.AreEqual(coords.Count, 5);
			Assert.AreEqual(((GeographicPosition)coords[0]).Latitude, ((GeographicPosition)coords[coords.Count - 1]).Latitude);
			Assert.AreEqual(((GeographicPosition)coords[0]).Longitude, ((GeographicPosition)coords[coords.Count - 1]).Longitude);
		}
	}
}
