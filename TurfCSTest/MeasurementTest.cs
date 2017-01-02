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

		[Test()]
		public void Center()
		{
			var boxFC = JsonConvert.DeserializeObject<FeatureCollection>(Tools.GetResource("center.box.geojson"));
			var blockFC = JsonConvert.DeserializeObject<FeatureCollection>(Tools.GetResource("center.block.geojson"));

			var boxFcCenter = Turf.Center(boxFC);
			boxFcCenter.Properties["marker-color"] = "#f0f";
			var boxFcCenterArray = Turf.GetCoord(boxFcCenter);
			Assert.AreEqual(boxFcCenterArray[0], 65.56640625, 0.00001);
			Assert.AreEqual(boxFcCenterArray[1], 43.59448261855401, 0.00001);

			var blockFcCenter = Turf.Center(blockFC.Features[0]);
			blockFcCenter.Properties["marker-color"] = "#f0f";
			var blockFcCenterArray = Turf.GetCoord(blockFcCenter);
			Assert.AreEqual(blockFcCenterArray[0], -114.02911397119072, 0.00001);
			Assert.AreEqual(blockFcCenterArray[1], 51.050271120392566, 0.00001);

			boxFC.Features.Add(boxFcCenter);
			blockFC.Features.Add(blockFcCenter);
			Console.WriteLine(JsonConvert.SerializeObject(boxFC));
			Console.WriteLine(JsonConvert.SerializeObject(blockFC));
		}

		[Test()]
		public void Envelope()
		{
			var fc = JsonConvert.DeserializeObject<FeatureCollection>(Tools.GetResource("envelope.fc.geojson"));
			var enveloped = Turf.Envelope(fc);
			Assert.AreEqual(enveloped.Geometry.Type, GeoJSONObjectType.Polygon);
			var exp = new List<List<double>>() {
				new List<double> () {20, -10},
				new List<double> () {130, -10},
				new List<double> () {130, 4},
				new List<double> () {20, 4},
				new List<double> () {20, -10}
			};
			var i = 0;
			Turf.CoordEach(enveloped, (List<double> act) => {
				Assert.AreEqual(act, exp[i], "positions are correct");
				i++;
			});
		}

		[Test()]
		public void LineDistance()
		{
			var route1 = JsonConvert.DeserializeObject<Feature>(Tools.GetResource("linedistance.route1.geojson"));
			var route2 = JsonConvert.DeserializeObject<Feature>(Tools.GetResource("linedistance.route2.geojson"));

			Assert.AreEqual(Math.Round(Turf.LineDistance((IGeoJSONObject)route1.Geometry, "miles")), 202);

			var point1 = Turf.Point(new double[] { -75.343, 39.984 });
			try
			{
				Turf.LineDistance(point1, "miles");
				Assert.Fail();
			}
			catch
			{
				Assert.Pass();
			}

			var multiPoint1 = new MultiPoint(new List<Point>() { 
				new Point(new GeographicPosition(39.984, -75.343)),
				new Point(new GeographicPosition(39.123, -75.534))          
			});
			try
			{
				Turf.LineDistance(multiPoint1, "miles");
				Assert.Fail();
			}
			catch
			{
				Assert.Pass();
			}

			Assert.AreEqual(Math.Round(Turf.LineDistance(route1, "miles")), 202);
			Assert.True((Turf.LineDistance(route2, "kilometers") - 742) < 1 && (Turf.LineDistance(route2, "kilometers") - 742) > (-1));

			var feat = JsonConvert.DeserializeObject<Feature>(Tools.GetResource("linedistance.polygon.geojson"));
			Assert.AreEqual(Math.Round(1000 * Turf.LineDistance(feat, "kilometers")), 5599);

			feat = JsonConvert.DeserializeObject<Feature>(Tools.GetResource("linedistance.multilinestring.geojson"));
			Assert.AreEqual(Math.Round(1000 * Turf.LineDistance(feat, "kilometers")), 4705);

			feat = JsonConvert.DeserializeObject<Feature>(Tools.GetResource("linedistance.multipolygon.geojson"));
			Assert.AreEqual(Math.Round(1000 * Turf.LineDistance(feat, "kilometers")), 8334);

			var fc = JsonConvert.DeserializeObject<FeatureCollection>(Tools.GetResource("linedistance.featurecollection.geojson"));
			Assert.AreEqual(Math.Round(1000 * Turf.LineDistance(fc, "kilometers")), 10304);
		}
	}
}
