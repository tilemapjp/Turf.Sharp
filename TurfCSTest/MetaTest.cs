using System;
using System.Collections.Generic;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using NUnit.Framework;
using TurfCS;

namespace TurfCSTest
{
	[TestFixture()]
	public class MetaTest
	{
		Point pointGeometry = new Point(new GeographicPosition(0,0));
		LineString lineStringGeometry = new LineString(new List<IPosition>() {
			new GeographicPosition(0,0), new GeographicPosition(1,1)
		});
		Polygon polygonGeometry = new Polygon(new List<LineString>() {
			new LineString(new List<IPosition>() {
				new GeographicPosition(0,0), new GeographicPosition(1,1),
				new GeographicPosition(1,0), new GeographicPosition(0,0)
			})
		});
		MultiPolygon multiPolygonGeometry = new MultiPolygon(new List<Polygon>() {
			new Polygon(new List<LineString>() {
				new LineString(new List<IPosition>() {
					new GeographicPosition(0,0), new GeographicPosition(1,1),
					new GeographicPosition(1,0), new GeographicPosition(0,0)
				})
			})
		});
		GeometryCollection geometryCollection = new GeometryCollection(new List<IGeometryObject>() {
			new Point(new GeographicPosition(0,0)),
			new LineString(new List<IPosition>() {
				new GeographicPosition(0,0), new GeographicPosition(1,1)
			})
		});
		Feature pointFeature = new Feature(new Point(new GeographicPosition(0, 0)),
		                                   new Dictionary<string, object>() { { "a", 1 } });

		internal FeatureCollection Collection(Feature feature)
		{
			var featureCollect = new FeatureCollection(new List<Feature>() { 
				feature
			});

			return featureCollect;
		}

		internal Feature Feature(IGeometryObject geometry)
		{
			var feature = new Feature(geometry, new Dictionary<string, object>() { { "a", 1 } });

			return feature;
		}

		[Test()]
		public void CoordEach()
		{
			var expect = new List<List<double>>() {
				new List<double>() { 0, 0 }
			};
			var output = new List<List<double>>();
			Action<List<double>> callback = (List<double> obj) => {
				output.Add(obj);
			};
			Turf.CoordEach(pointFeature, callback);
			Assert.AreEqual(output, expect);

			var pointCollect = Collection(pointFeature);
			output.Clear();
			Turf.CoordEach(pointCollect, callback);
			Assert.AreEqual(output, expect);

			expect.Add(new List<double>() { 1, 1 });
			output.Clear();
			Turf.CoordEach(lineStringGeometry, callback);
			Assert.AreEqual(output, expect);

			var lineStringFeature = Feature(lineStringGeometry);
			output.Clear();
			Turf.CoordEach(lineStringFeature, callback);
			Assert.AreEqual(output, expect);

			var lineStringCollect = Collection(lineStringFeature);
			output.Clear();
			Turf.CoordEach(lineStringCollect, callback);
			Assert.AreEqual(output, expect);

			expect.Add(new List<double>() { 0, 1 });
			output.Clear();
			Turf.CoordEach(polygonGeometry, callback, true);
			Assert.AreEqual(output, expect);

			var polygonFeature = Feature(polygonGeometry);
			output.Clear();
			Turf.CoordEach(polygonFeature, callback, true);
			Assert.AreEqual(output, expect);

			var polygonCollect = Collection(polygonFeature);
			output.Clear();
			Turf.CoordEach(polygonCollect, callback, true);
			Assert.AreEqual(output, expect);

			expect.Add(new List<double>() { 0, 0 });
			output.Clear();
			Turf.CoordEach(polygonGeometry, callback);
			Assert.AreEqual(output, expect);

			output.Clear();
			Turf.CoordEach(polygonFeature, callback);
			Assert.AreEqual(output, expect);

			output.Clear();
			Turf.CoordEach(polygonCollect, callback);
			Assert.AreEqual(output, expect);

		}

		[Test()]
		public void PropEach()
		{
			Action<Dictionary<string, object>, int> callback = (Dictionary<string, object>obj, int i) =>
			{
				Assert.AreEqual(obj, new Dictionary<string, object>() { { "a", 1 } });
				Assert.AreEqual(i, 0);
			};
			Turf.PropEach(pointFeature, callback);

			var pointCollect = Collection(pointFeature);
			Turf.PropEach(pointCollect, callback);
		}
	}
}
