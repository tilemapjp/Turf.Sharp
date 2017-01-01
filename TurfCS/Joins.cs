using System;
using System.Collections.Generic;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace TurfCS
{
	public partial class Turf
	{
		/**
		 * Takes a {@link Point} and a {@link Polygon} or {@link MultiPolygon} and determines if the point resides inside the polygon. The polygon can
		 * be convex or concave. The function accounts for holes.
		 *
		 * @name inside
		 * @param {Feature<Point>} point input point
		 * @param {Feature<(Polygon|MultiPolygon)>} polygon input polygon or multipolygon
		 * @return {boolean} `true` if the Point is inside the Polygon; `false` if the Point is not inside the Polygon
		 * @example
		 * var pt = point([-77, 44]);
		 * var poly = polygon([[
		 *   [-81, 41],
		 *   [-81, 47],
		 *   [-72, 47],
		 *   [-72, 41],
		 *   [-81, 41]
		 * ]]);
		 *
		 * var isInside = turf.inside(pt, poly);
		 *
		 * //=isInside
		 */
		static public bool Inside(Feature point, Feature poly)
		{
			var type = poly.Geometry.Type;
			if (type == GeoJSONObjectType.Polygon)
				return Inside_(Turf.GetCoord(point), new List<Polygon>() { (Polygon)poly.Geometry });
			else if (type == GeoJSONObjectType.MultiPolygon)
				return Inside_(Turf.GetCoord(point), ((MultiPolygon)poly.Geometry).Coordinates);
			else
				throw new Exception("2nd argument must be Polygon or MultiPolygon");
		}
		/*static public bool Inside(Feature point, Polygon poly)
		{
			return Inside_(Turf.GetCoord(point), new List<Polygon>() { poly });
		}
		static public bool Inside(Feature point, MultiPolygon mpoly)
		{
			return Inside_(Turf.GetCoord(point), mpoly.Coordinates);
		}
		static public bool Inside(Point point, Polygon poly)
		{
			return Inside_(Turf.GetCoord(point), new List<Polygon>() { poly });
		}
		static public bool Inside(Point point, MultiPolygon mpoly)
		{
			return Inside_(Turf.GetCoord(point), mpoly.Coordinates);
		}*/
		static private bool Inside_(List<double> pt, List<Polygon> polys)
		{
			bool insidePoly = false;
			for (int i = 0; i < polys.Count && !insidePoly; i++)
			{
				var poly = polys[i];
				var lines = poly.Coordinates;
				// check if it is in the outer ring first
				if (InRing(pt, lines[0]))
				{
					bool inHole = false;
					int k = 1;
					// check for the point in any of the holes
					while (k < lines.Count && !inHole)
					{
						if (InRing(pt, lines[k]))
						{
							inHole = true;
						}
						k++;
					}
					if (!inHole) insidePoly = true;
				}
			}
			return insidePoly;
		}

		// pt is [x,y] and ring is [[x,y], [x,y],..]
		static private bool InRing(List<double>pt, LineString ringOutline)
		{
			var isInside = false;
			var points = ringOutline.Coordinates;
			int i = 0;
			for (int j = points.Count - 1; i < points.Count; j = i++)
			{
				var pointi = (GeographicPosition)points[i];
				var pointj = (GeographicPosition)points[j];
				var xi = pointi.Longitude;
				var yi = pointi.Latitude;
				var xj = pointj.Longitude;
				var yj = pointj.Latitude;
				var intersect = ((yi > pt[1]) != (yj > pt[1])) &&
				(pt[0] < (xj - xi) * (pt[1] - yi) / (yj - yi) + xi);
				if (intersect) isInside = !isInside;
			}
			return isInside;
		}

		/**
		 * Takes a set of {@link Point|points} and a set of {@link Polygon|polygons} and performs a spatial join.
		 *
		 * @name tag
		 * @param {FeatureCollection<Point>} points input points
		 * @param {FeatureCollection<Polygon>} polygons input polygons
		 * @param {string} field property in `polygons` to add to joined {<Point>} features
		 * @param {string} outField property in `points` in which to store joined property from `polygons`
		 * @return {FeatureCollection<Point>} points with `containingPolyId` property containing values from `polyId`
		 * @example
		 * var pt1 = point([-77, 44]);
		 * var pt2 = point([-77, 38]);
		 * var poly1 = polygon([[
		 *   [-81, 41],
		 *   [-81, 47],
		 *   [-72, 47],
		 *   [-72, 41],
		 *   [-81, 41]
		 * ]], {pop: 3000});
		 * var poly2 = polygon([[
		 *   [-81, 35],
		 *   [-81, 41],
		 *   [-72, 41],
		 *   [-72, 35],
		 *   [-81, 35]
		 * ]], {pop: 1000});
		 *
		 * var points = featureCollection([pt1, pt2]);
		 * var polygons = featureCollection([poly1, poly2]);
		 *
		 * var tagged = turf.tag(points, polygons,
		 *                       'pop', 'population');
		 *
		 * //=tagged
		 */
		static public FeatureCollection Tag(FeatureCollection points, FeatureCollection polygons, string field, string outField)
		{
			foreach (var pt in points.Features) {
				if (!pt.Properties.ContainsKey(outField))
				{
					foreach (var poly in polygons.Features)
					{
						var isInside = Inside(pt, poly);
						if (isInside)
						{
							object val;
							poly.Properties.TryGetValue(field, out val);
							pt.Properties.Add(outField, val);
						}
					}
				}
			}
			return points;
		}
	}
}
