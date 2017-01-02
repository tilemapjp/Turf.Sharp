using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace TurfCS
{
	public partial class Turf
	{
		/**
		 * Takes two {@link Point|points} and finds the geographic bearing between them.
		 *
		 * @name bearing
		 * @param {Feature<Point>} start starting Point
		 * @param {Feature<Point>} end ending Point
		 * @returns {number} bearing in decimal degrees
		 * @example
		 * var point1 = {
		 *   "type": "Feature",
		 *   "properties": {
		 *     "marker-color": '#f00'
		 *   },
		 *   "geometry": {
		 *     "type": "Point",
		 *     "coordinates": [-75.343, 39.984]
		 *   }
		 * };
		 * var point2 = {
		 *   "type": "Feature",
		 *   "properties": {
		 *     "marker-color": '#0f0'
		 *   },
		 *   "geometry": {
		 *     "type": "Point",
		 *     "coordinates": [-75.534, 39.123]
		 *   }
		 * };
		 *
		 * var points = {
		 *   "type": "FeatureCollection",
		 *   "features": [point1, point2]
		 * };
		 *
		 * //=points
		 *
		 * var bearing = turf.bearing(point1, point2);
		 *
		 * //=bearing
		 */
		public static double Bearing(IPosition start, IPosition end)
		{
			return Bearing(Turf.GetCoord(start), Turf.GetCoord(end));
		}
		public static double Bearing(Feature start, Feature end)
		{
			return Bearing(Turf.GetCoord(start), Turf.GetCoord(end));
		}
		private static double Bearing(List<double> start, List<double> end)
		{
			var degrees2radians = Math.PI / 180;
			var radians2degrees = 180 / Math.PI;

			var lon1 = degrees2radians * start[0];
			var lon2 = degrees2radians * end[0];
			var lat1 = degrees2radians * start[1];
			var lat2 = degrees2radians * end[1];
			var a = Math.Sin(lon2 - lon1) * Math.Cos(lat2);
			var b = Math.Cos(lat1) * Math.Sin(lat2) -
				Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1);

			var bearing = radians2degrees * Math.Atan2(a, b);

			return bearing;
		}

		/**
		 * Takes a {@link Point} and calculates the location of a destination point given a distance in degrees, radians, miles, or kilometers; and bearing in degrees. This uses the [Haversine formula](http://en.wikipedia.org/wiki/Haversine_formula) to account for global curvature.
		 *
		 * @name destination
		 * @param {Feature<Point>} from starting point
		 * @param {number} distance distance from the starting point
		 * @param {number} bearing ranging from -180 to 180
		 * @param {string} [units=kilometers] miles, kilometers, degrees, or radians
		 * @returns {Feature<Point>} destination point
		 * @example
		 * var point = {
		 *   "type": "Feature",
		 *   "properties": {
		 *     "marker-color": "#0f0"
		 *   },
		 *   "geometry": {
		 *     "type": "Point",
		 *     "coordinates": [-75.343, 39.984]
		 *   }
		 * };
		 * var distance = 50;
		 * var bearing = 90;
		 * var units = 'miles';
		 *
		 * var destination = turf.destination(point, distance, bearing, units);
		 * destination.properties['marker-color'] = '#f00';
		 *
		 * var result = {
		 *   "type": "FeatureCollection",
		 *   "features": [point, destination]
		 * };
		 *
		 * //=result
		 */
		public static Feature Destination(IPosition from, double distance, double bearing, string units = "kilometers")
		{
			return Destination(Turf.GetCoord(from), distance, bearing, units);
		}
		public static Feature Destination(Feature from, double distance, double bearing, string units = "kilometers")
		{
			return Destination(Turf.GetCoord(from), distance, bearing, units);
		}
		private static Feature Destination(List<double> from, double distance, double bearing, string units = "kilometers")
		{
			var degrees2radians = Math.PI / 180;
			var radians2degrees = 180 / Math.PI;
			var coordinates1 = Turf.GetCoord(from);
			var longitude1 = degrees2radians * coordinates1[0];
			var latitude1 = degrees2radians * coordinates1[1];
			var bearing_rad = degrees2radians * bearing;

			var radians = Turf.DistanceToRadians(distance, units);

			var latitude2 = Math.Asin(Math.Sin(latitude1) * Math.Cos(radians) +
				Math.Cos(latitude1) * Math.Sin(radians) * Math.Cos(bearing_rad));
			var longitude2 = longitude1 + Math.Atan2(Math.Sin(bearing_rad) *
				Math.Sin(radians) * Math.Cos(latitude1),
				Math.Cos(radians) - Math.Sin(latitude1) * Math.Sin(latitude2));

			return Turf.Point(new double[] { radians2degrees * longitude2, radians2degrees * latitude2});
		}

		/**
		 * Calculates the distance between two {@link Point|points} in degrees, radians,
		 * miles, or kilometers. This uses the
		 * [Haversine formula](http://en.wikipedia.org/wiki/Haversine_formula)
		 * to account for global curvature.
		 *
		 * @name distance
		 * @param {Feature<Point>} from origin point
		 * @param {Feature<Point>} to destination point
		 * @param {string} [units=kilometers] can be degrees, radians, miles, or kilometers
		 * @return {number} distance between the two points
		 * @example
		 * var from = {
		 *   "type": "Feature",
		 *   "properties": {},
		 *   "geometry": {
		 *     "type": "Point",
		 *     "coordinates": [-75.343, 39.984]
		 *   }
		 * };
		 * var to = {
		 *   "type": "Feature",
		 *   "properties": {},
		 *   "geometry": {
		 *     "type": "Point",
		 *     "coordinates": [-75.534, 39.123]
		 *   }
		 * };
		 * var units = "miles";
		 *
		 * var points = {
		 *   "type": "FeatureCollection",
		 *   "features": [from, to]
		 * };
		 *
		 * //=points
		 *
		 * var distance = turf.distance(from, to, units);
		 *
		 * //=distance
		 */
		public static double Distance(IPosition from, IPosition to, string units = "kilometers")
		{
			return Distance(Turf.GetCoord(from), Turf.GetCoord(to), units);
		}
		public static double Distance(Feature from, Feature to, string units = "kilometers")
		{
			return Distance(Turf.GetCoord(from), Turf.GetCoord(to), units);
		}
		private static double Distance(List<double> from, List<double> to, string units = "kilometers")
		{
			var degrees2radians = Math.PI / 180;
			var dLat = degrees2radians * (to[1] - from[1]);
			var dLon = degrees2radians * (to[0] - from[0]);
			var lat1 = degrees2radians * from[1];
			var lat2 = degrees2radians * to[1];

			var a = Math.Pow(Math.Sin(dLat / 2), 2) +
				  Math.Pow(Math.Sin(dLon / 2), 2) * Math.Cos(lat1) * Math.Cos(lat2);

			return Turf.RadiansToDistance(2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)), units);
		}

		/**
		 * Takes a {@link LineString|line} and returns a {@link Point|point} at a specified distance along the line.
		 *
		 * @name along
		 * @param {Feature<LineString>} line input line
		 * @param {number} distance distance along the line
		 * @param {string} [units=kilometers] can be degrees, radians, miles, or kilometers
		 * @return {Feature<Point>} Point `distance` `units` along the line
		 * @example
		 * var line = {
		 *   "type": "Feature",
		 *   "properties": {},
		 *   "geometry": {
		 *     "type": "LineString",
		 *     "coordinates": [
		 *       [-77.031669, 38.878605],
		 *       [-77.029609, 38.881946],
		 *       [-77.020339, 38.884084],
		 *       [-77.025661, 38.885821],
		 *       [-77.021884, 38.889563],
		 *       [-77.019824, 38.892368]
		 *     ]
		 *   }
		 * };
		 *
		 * var along = turf.along(line, 1, 'miles');
		 *
		 * var result = {
		 *   "type": "FeatureCollection",
		 *   "features": [line, along]
		 * };
		 *
		 * //=result
		 */
		public static Feature Along(Feature line, double distance, string units = "kilometers")
		{
			if (line.Geometry.Type == GeoJSONObjectType.LineString)
			{
				return Along((LineString)line.Geometry, distance, units);
			}
			else {
				throw new Exception("input must be a LineString Feature or Geometry");
			}
		}
		public static Feature Along(LineString line, double distance, string units = "kilometers")
		{
			var coords = line.Coordinates;
			double travelled = 0;
			for (var i = 0; i < coords.Count; i++)
			{
				if (distance >= travelled && i == coords.Count - 1) break;
				else if (travelled >= distance)
				{
					var overshot = distance - travelled;
					if (Math.Abs(overshot) < double.Epsilon) return Turf.Point(coords[i]);
					else {
						var direction = Turf.Bearing(coords[i], coords[i - 1]) - 180;
						var interpolated = Turf.Destination(coords[i], overshot, direction, units);
						return interpolated;
					}
				}
				else {
					travelled += Turf.Distance(coords[i], coords[i + 1], units);
				}
			}
			return Turf.Point(coords[coords.Count - 1]);
		}

		/**
		 * Takes one or more features and calculates the centroid using
		 * the mean of all vertices.
		 * This lessens the effect of small islands and artifacts when calculating
		 * the centroid of a set of polygons.
		 *
		 * @name centroid
		 * @param {(Feature|FeatureCollection)} features input features
		 * @return {Feature<Point>} the centroid of the input features
		 * @example
		 * var poly = {
		 *   "type": "Feature",
		 *   "properties": {},
		 *   "geometry": {
		 *     "type": "Polygon",
		 *     "coordinates": [[
		 *       [105.818939,21.004714],
		 *       [105.818939,21.061754],
		 *       [105.890007,21.061754],
		 *       [105.890007,21.004714],
		 *       [105.818939,21.004714]
		 *     ]]
		 *   }
		 * };
		 *
		 * var centroidPt = turf.centroid(poly);
		 *
		 * var result = {
		 *   "type": "FeatureCollection",
		 *   "features": [poly, centroidPt]
		 * };
		 *
		 * //=result
		 */
		public static Feature Centroid(IGeoJSONObject features)
		{
			double xSum = 0;
			double ySum = 0;
			int len = 0;
			Turf.CoordEach(features, (List<double> coord) => {
				xSum += coord[0];
				ySum += coord[1];
				len++;
			}, true);
			return Turf.Point(new double[] { xSum / (double)len, ySum / (double)len});
		}

		/**
		 * Takes a bbox and returns an equivalent {@link Polygon|polygon}.
		 *
		 * @name bboxPolygon
		 * @param {Array<number>} bbox extent in [minX, minY, maxX, maxY] order
		 * @return {Feature<Polygon>} a Polygon representation of the bounding box
		 * @example
		 * var bbox = [0, 0, 10, 10];
		 *
		 * var poly = turf.bboxPolygon(bbox);
		 *
		 * //=poly
		 */
		public static Feature BboxPolygon(List<double>bbox)
		{
			var lowLeft = new GeographicPosition(bbox[1], bbox[0]);
			var topLeft = new GeographicPosition(bbox[3], bbox[0]);
			var topRight = new GeographicPosition(bbox[3], bbox[2]);
			var lowRight = new GeographicPosition(bbox[1], bbox[2]);

			var poly = new Polygon(new List<LineString> () {
				new LineString(new List<IPosition> {
					lowLeft,
					lowRight,
					topRight,
					topLeft,
					lowLeft
				})
			});

			return new Feature(poly);
		}

		/**
		 * Takes a {@link Feature} or {@link FeatureCollection} and returns the absolute center point of all features.
		 *
		 * @name center
		 * @param {(Feature|FeatureCollection)} layer input features
		 * @return {Feature<Point>} a Point feature at the absolute center point of all input features
		 * @example
		 * var features = {
		 *   "type": "FeatureCollection",
		 *   "features": [
		 *     {
		 *       "type": "Feature",
		 *       "properties": {},
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-97.522259, 35.4691]
		 *       }
		 *     }, {
		 *       "type": "Feature",
		 *       "properties": {},
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-97.502754, 35.463455]
		 *       }
		 *     }, {
		 *       "type": "Feature",
		 *       "properties": {},
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-97.508269, 35.463245]
		 *       }
		 *     }, {
		 *       "type": "Feature",
		 *       "properties": {},
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-97.516809, 35.465779]
		 *       }
		 *     }, {
		 *       "type": "Feature",
		 *       "properties": {},
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-97.515372, 35.467072]
		 *       }
		 *     }, {
		 *       "type": "Feature",
		 *       "properties": {},
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-97.509363, 35.463053]
		 *       }
		 *     }, {
		 *       "type": "Feature",
		 *       "properties": {},
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-97.511123, 35.466601]
		 *       }
		 *     }, {
		 *       "type": "Feature",
		 *       "properties": {},
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-97.518547, 35.469327]
		 *       }
		 *     }, {
		 *       "type": "Feature",
		 *       "properties": {},
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-97.519706, 35.469659]
		 *       }
		 *     }, {
		 *       "type": "Feature",
		 *       "properties": {},
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-97.517839, 35.466998]
		 *       }
		 *     }, {
		 *       "type": "Feature",
		 *       "properties": {},
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-97.508678, 35.464942]
		 *       }
		 *     }, {
		 *       "type": "Feature",
		 *       "properties": {},
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-97.514914, 35.463453]
		 *       }
		 *     }
		 *   ]
		 * };
		 *
		 * var centerPt = turf.center(features);
		 * centerPt.properties['marker-size'] = 'large';
		 * centerPt.properties['marker-color'] = '#000';
		 *
		 * var resultFeatures = features.features.concat(centerPt);
		 * var result = {
		 *   "type": "FeatureCollection",
		 *   "features": resultFeatures
		 * };
		 *
		 * //=result
		 */
		public static Feature Center(IGeoJSONObject layer)
		{
			var ext = Bbox(layer);
			var x = (ext[0] + ext[2]) / 2;
			var y = (ext[1] + ext[3]) / 2;
			return Turf.Point(new double[] { x, y });
		}

		/**
		 * Takes any number of features and returns a rectangular {@link Polygon} that encompasses all vertices.
		 *
		 * @name envelope
		 * @param {(Feature|FeatureCollection)} features input features
		 * @return {Feature<Polygon>} a rectangular Polygon feature that encompasses all vertices
		 * @example
		 * var fc = {
		 *   "type": "FeatureCollection",
		 *   "features": [
		 *     {
		 *       "type": "Feature",
		 *       "properties": {
		 *         "name": "Location A"
		 *       },
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-75.343, 39.984]
		 *       }
		 *     }, {
		 *       "type": "Feature",
		 *       "properties": {
		 *         "name": "Location B"
		 *       },
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-75.833, 39.284]
		 *       }
		 *     }, {
		 *       "type": "Feature",
		 *       "properties": {
		 *         "name": "Location C"
		 *       },
		 *       "geometry": {
		 *         "type": "Point",
		 *         "coordinates": [-75.534, 39.123]
		 *       }
		 *     }
		 *   ]
		 * };
		 *
		 * var enveloped = turf.envelope(fc);
		 *
		 * var resultFeatures = fc.features.concat(enveloped);
		 * var result = {
		 *   "type": "FeatureCollection",
		 *   "features": resultFeatures
		 * };
		 *
		 * //=result
		 */
		public static Feature Envelope(IGeoJSONObject features) 
		{
			return BboxPolygon(Bbox(features));
		}

		/**
		 * Takes a {@link LineString} or {@link Polygon} and measures its length in the specified units.
		 *
		 * @name lineDistance
		 * @param {Feature<(LineString|Polygon)>|FeatureCollection<(LineString|Polygon)>} line line to measure
		 * @param {string} [units=kilometers] can be degrees, radians, miles, or kilometers
		 * @return {number} length of the input line
		 * @example
		 * var line = {
		 *   "type": "Feature",
		 *   "properties": {},
		 *   "geometry": {
		 *     "type": "LineString",
		 *     "coordinates": [
		 *       [-77.031669, 38.878605],
		 *       [-77.029609, 38.881946],
		 *       [-77.020339, 38.884084],
		 *       [-77.025661, 38.885821],
		 *       [-77.021884, 38.889563],
		 *       [-77.019824, 38.892368]
		 *     ]
		 *   }
		 * };
		 *
		 * var length = turf.lineDistance(line, 'miles');
		 *
		 * //=line
		 *
		 * //=length
		 */
		public static double LineDistance(IGeoJSONObject line, string units = "kilometers") {
			if (line.Type == GeoJSONObjectType.FeatureCollection)
			{
				return ((FeatureCollection)line).Features.Aggregate(0, (double memo, Feature feature) => {
					return memo + LineDistance(feature);
				});
			}

			var geometry = line.Type == GeoJSONObjectType.Feature ? ((Feature)line).Geometry : (IGeometryObject)line;

			if (geometry.Type == GeoJSONObjectType.LineString)
			{
				return Length(((LineString)geometry).Coordinates, units);
			}
			else if (geometry.Type == GeoJSONObjectType.Polygon || geometry.Type == GeoJSONObjectType.MultiLineString)
			{
				double d = 0;
				var lines = geometry.Type == GeoJSONObjectType.Polygon ?
									 ((Polygon)geometry).Coordinates : ((MultiLineString)geometry).Coordinates;
				for (var i = 0; i < lines.Count; i++)
				{
					var points = ((LineString)lines[i]).Coordinates;
					d += Length(points, units);
				}
				return d;
			}
			else if (geometry.Type == GeoJSONObjectType.MultiPolygon)
			{
				double d = 0;
				var polygons = ((MultiPolygon)geometry).Coordinates;
				for (var i = 0; i < polygons.Count; i++)
				{
					var lines = ((Polygon)polygons[i]).Coordinates;
					for (var j = 0; j < lines.Count; j++)
					{
						var points = ((LineString)lines[j]).Coordinates;
						d += Length(points, units);
					}
				}
				return d;
			}
			else {
				throw new Exception("input must be a LineString, MultiLineString, " +
					"Polygon, or MultiPolygon Feature or Geometry (or a FeatureCollection " +
					"containing only those types)");
			}

		}

		private static double Length(List<IPosition>coords, string units)
		{
			double travelled = 0;
			var prevCoords = coords[0];
			var curCoords = coords[0];
			for (var i = 1; i < coords.Count; i++)
			{
				curCoords = coords[i];
				travelled += Distance(prevCoords, curCoords, units);
				prevCoords = curCoords;
			}
			return travelled;
		}


	}
}
