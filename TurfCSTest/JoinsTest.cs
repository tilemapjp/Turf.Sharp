using System;
using NUnit.Framework;
using GeoJSON.Net.Geometry;
using System.Collections.Generic;
using Newtonsoft.Json;
using GeoJSON.Net.Feature;
using TurfCS;
using System.Linq;

namespace TurfCSTest
{
	[TestFixture()]
	public class JoinsTest
	{
		[Test()]
		public void Inside_FeatureCollection()
		{
			// test for a simple polygon
			var poly = new Feature(new Polygon(new List<LineString>() { 
				new LineString(new List<Position>() {
					new GeographicPosition(0,0),
					new GeographicPosition(100,0),
					new GeographicPosition(100,100),
					new GeographicPosition(0,100),
					new GeographicPosition(0,0)
				})			
			}));
			var ptIn = Turf.Point(new double[] { 50, 50 });
			var ptOut = Turf.Point(new double[] { 140, 150 });

			Assert.True(Turf.Inside(ptIn, poly), "point inside simple polygon");
			Assert.False(Turf.Inside(ptOut,poly), "point outside simple polygon");

			// test for a concave polygon
			var concavePoly = new Feature(new Polygon(new List<LineString>() { 
				new LineString(new List<Position>() {
					new GeographicPosition(0,0),
					new GeographicPosition(50,50),
					new GeographicPosition(100,50),
					new GeographicPosition(100,100),
					new GeographicPosition(0,100),
					new GeographicPosition(0,0)
				})
			}));
			var ptConcaveIn = Turf.Point(new double[] { 75, 75 });
			var ptConcaveOut = Turf.Point(new double[] { 25, 50 });

			Assert.True(Turf.Inside(ptConcaveIn, concavePoly), "point inside concave polygon");
			Assert.False(Turf.Inside(ptConcaveOut, concavePoly), "point outside concave polygon");
		}

		[Test()]
		public void Inside_PolyWithHole()
		{
			var ptInHole = Turf.Point(new double[] { -86.69208526611328, 36.20373274711739 });
			var ptInPoly = Turf.Point(new double[] { -86.72229766845702, 36.20258997094334 });
			var ptOutsidePoly = Turf.Point(new double[] { -86.75079345703125, 36.18527313913089 });
			var polyHole = JsonConvert.DeserializeObject<Feature>(Tools.GetResource("poly-with-hole.geojson"));

			Assert.False(Turf.Inside(ptInHole, polyHole));
			Assert.True(Turf.Inside(ptInPoly, polyHole));
			Assert.False(Turf.Inside(ptOutsidePoly, polyHole));
		}

		[Test()]
		public void Inside_MultiPolyWithHole()
		{
			var ptInHole = Turf.Point(new double[] { -86.69208526611328, 36.20373274711739 });
			var ptInPoly = Turf.Point(new double[] { -86.72229766845702, 36.20258997094334 });
			var ptInPoly2 = Turf.Point(new double[] { -86.75079345703125, 36.18527313913089 });
			var ptOutsidePoly = Turf.Point(new double[] { -86.75302505493164, 36.23015046460186 });
			var multiPolyHole = JsonConvert.DeserializeObject<Feature>(Tools.GetResource("multipoly-with-hole.geojson"));

			Assert.False(Turf.Inside(ptInHole, multiPolyHole));
			Assert.True(Turf.Inside(ptInPoly, multiPolyHole));
			Assert.True(Turf.Inside(ptInPoly2, multiPolyHole));
			Assert.True(Turf.Inside(ptInPoly, multiPolyHole));
			Assert.False(Turf.Inside(ptOutsidePoly, multiPolyHole));
		}

		[Test()]
		public void Tag()
		{
			var points = JsonConvert.DeserializeObject<FeatureCollection>(Tools.GetResource("tagPoints.geojson"));
			var polygons = JsonConvert.DeserializeObject<FeatureCollection>(Tools.GetResource("tagPolygons.geojson"));

			var taggedPoints = Turf.Tag(points, polygons, "polyID", "containingPolyID");

			Assert.AreEqual(taggedPoints.Features.Count, points.Features.Count,
			  "tagged points should have the same length as the input points");

			var count = taggedPoints.Features.Where(x =>
			{
				object val;
				x.Properties.TryGetValue("containingPolyID", out val);
				return val != null && int.Parse(val.ToString()) == 4;
			}).Count();
			Assert.AreEqual(count, 6, "polygon 4 should have tagged 6 points");
		}	
	
	}
}
