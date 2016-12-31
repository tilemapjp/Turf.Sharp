using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace TurfCS
{
	public partial class Turf
	{
		internal class Vertix {
			internal double X { get; }
			internal double Y { get; }
			internal object Z { get; }
			internal bool __S { get; }
			internal Vertix(double x, double y, object z = null, bool __s = false) {
				this.X = x;
				this.Y = y;
				this.Z = z;
				this.__S = __s;
			}
		}

		internal class Triangle
		{
			internal Vertix A { get; }
			internal Vertix B { get; }
			internal Vertix C { get; }
			internal double X { get; }
			internal double Y { get; }
			internal double R { get; }

			internal Triangle(Vertix a, Vertix b, Vertix c)
			{
				this.A = a;
				this.B = b;
				this.C = c;

				double CA = b.X - a.X;
				double CB = b.Y - a.Y;
				double CC = c.X - a.X;
				double CD = c.Y - a.Y;
				double CE = CA * (a.X + b.X) + CB * (a.Y + b.Y);
				double CF = CC * (a.X + c.X) + CD * (a.Y + c.Y);
				double CG = 2 * (CA * (c.Y - b.Y) - CB * (c.X - b.X));
				double dx, dy;


				// If the points of the triangle are collinear, then just find the
				// extremes and use the midpoint as the center of the circumcircle.
				this.X = (CD * CE - CB * CF) / CG;
				this.Y = (CA * CF - CC * CE) / CG;
				dx = this.X - a.X;
				dy = this.Y - a.Y;
				this.R = dx * dx + dy * dy;
			}
		}

		/**
		 * Takes a set of {@link Point|points} and the name of a z-value property and
		 * creates a [Triangulated Irregular Network](http://en.wikipedia.org/wiki/Triangulated_irregular_network),
		 * or a TIN for short, returned as a collection of Polygons. These are often used
		 * for developing elevation contour maps or stepped heat visualizations.
		 *
		 * This triangulates the points, as well as adds properties called `a`, `b`,
		 * and `c` representing the value of the given `propertyName` at each of
		 * the points that represent the corners of the triangle.
		 *
		 * @name tin
		 * @param {FeatureCollection<Point>} points input points
		 * @param {String=} z name of the property from which to pull z values
		 * This is optional: if not given, then there will be no extra data added to the derived triangles.
		 * @return {FeatureCollection<Polygon>} TIN output
		 * @example
		 * // generate some random point data
		 * var points = turf.random('points', 30, {
		 *   bbox: [50, 30, 70, 50]
		 * });
		 * //=points
		 * // add a random property to each point between 0 and 9
		 * for (var i = 0; i < points.features.length; i++) {
		 *   points.features[i].properties.z = ~~(Math.random() * 9);
		 * }
		 * var tin = turf.tin(points, 'z')
		 * for (var i = 0; i < tin.features.length; i++) {
		 *   var properties  = tin.features[i].properties;
		 *   // roughly turn the properties of each
		 *   // triangle into a fill color
		 *   // so we can visualize the result
		 *   properties.fill = '#' + properties.a +
		 *     properties.b + properties.c;
		 * }
		 * //=tin
		 */
		public static FeatureCollection Tin(FeatureCollection points, string z = null)
		{
			//break down points
			var vertices = points.Features.Select(x => {
				var point = (Point)x.Geometry;
				var pos = (GeographicPosition)point.Coordinates;
				object zval = null;
				if (z != null) x.Properties.TryGetValue(z, out zval);
				return new Vertix( pos.Longitude, pos.Latitude, zval );
			});

			var triangles = Triangulate(vertices.ToList());

			var features = triangles.Select(x =>
			{
				return new Feature(
					new Polygon(new List<LineString>() {
						new LineString(new List<IPosition>() {
							new GeographicPosition(x.A.Y, x.A.X),
							new GeographicPosition(x.B.Y, x.B.X),
							new GeographicPosition(x.C.Y, x.C.X),
							new GeographicPosition(x.A.Y, x.A.X)
						})
					}),
					new Dictionary<string, object>(){
						{"a", x.A.Z},
						{"b", x.B.Z},
						{"c", x.C.Z}
					}
				);
			}).ToList();
			return new FeatureCollection(features);
		}

		private static void Dedup(List<Vertix> edges)
		{
			var j = edges.Count;
			Vertix a, b, m, n;

			while (j > 0)
			{
				b = edges[--j];
				a = edges[--j];
				var i = j;
				while (i > 0)
				{
					n = edges[--i];
					m = edges[--i];
					if ((a == m && b == n) || (a == n && b == m))
					{
						edges.RemoveRange(j, 2);
						edges.RemoveRange(i, 2);
						j -= 2;
						break;
					}
				}
			}
		}

		private static List<Triangle> Triangulate(List<Vertix> vertices) {
			// Bail if there aren't enough vertices to form any triangles.
			if (vertices.Count() < 3) return new List<Triangle>();

			// Ensure the vertex array is in order of descending X coordinate
			// (which is needed to ensure a subquadratic runtime), and then find
			// the bounding box around the points.
			vertices = vertices.OrderByDescending(x => x.X).ToList();

			var i = vertices.Count - 1;
			var xmin = vertices[i].X;
			var xmax = vertices[0].X;
			var ymin = vertices[i].Y;
			var ymax = ymin;
			var epsilon = 1e-12;

			Vertix a, b, c;
			double A, B, G;

			for (i--;i>=0;i--) {
				if (vertices[i].Y < ymin)
					ymin = vertices[i].Y;
				if (vertices[i].Y > ymax)
					ymax = vertices[i].Y;
			}

			//Find a supertriangle, which is a triangle that surrounds all the
			//vertices. This is used like something of a sentinel value to remove
			//cases in the main algorithm, and is removed before we return any
			// results.

			// Once found, put it in the "open" list. (The "open" list is for
			// triangles who may still need to be considered; the "closed" list is
			// for triangles which do not.)

			var dx = xmax - xmin;
			var dy = ymax - ymin;
			var dmax = (dx > dy) ? dx : dy;
			var xmid = (xmax + xmin) * 0.5;
			var ymid = (ymax + ymin) * 0.5;
			var open = new List<Triangle>() {
				new Triangle(new Vertix(xmid - 20 * dmax, ymid - dmax, null, true),
							 new Vertix(xmid, ymid + 20 * dmax, null, true),
				             new Vertix(xmid + 20 * dmax, ymid - dmax, null, true))
			};
			var closed = new List<Triangle>();
			var edges = new List<Vertix>();
			int j;

			// Incrementally add each vertex to the mesh.
			i = vertices.Count;
			for (i--; i >= 0; i--)
			{
				// For each open triangle, check to see if the current point is
				// inside it's circumcircle. If it is, remove the triangle and add
				// it's edges to an edge list.
				edges = new List<Vertix>();
				j = open.Count;
				for (j--; j >= 0; j--)
				{
					// If this point is to the right of this triangle's circumcircle,
					// then this triangle should never get checked again. Remove it
					// from the open list, add it to the closed list, and skip.
					dx = vertices[i].X - open[j].X;
					if (dx > 0 && dx * dx > open[j].R)
					{
						closed.Add(open[j]);
						open.RemoveAt(j);
						continue;
					}

					// If not, skip this triangle.
					dy = vertices[i].Y - open[j].Y;
					if (dx * dx + dy * dy > open[j].R)
						continue;

					// Remove the triangle and add it's edges to the edge list.
					edges.Add(open[j].A);
					edges.Add(open[j].B);
					edges.Add(open[j].B);
					edges.Add(open[j].C);
					edges.Add(open[j].C);
					edges.Add(open[j].A);
					open.RemoveAt(j);
				}

				// Remove any doubled edges.
				Dedup(edges);

				// Add a new triangle for each edge.
				j = edges.Count;
				while (j > 0)
				{
					b = edges[--j];
					a = edges[--j];
					c = vertices[i];
					// Avoid adding colinear triangles (which have error-prone
					// circumcircles)
					A = b.X - a.X;
					B = b.Y - a.Y;
					G = 2 * (A * (c.Y - b.Y) - B * (c.X - b.X));
					if (Math.Abs(G) > epsilon)
					{
						open.Add(new Triangle(a, b, c));
					}
				}
			}

			// Copy any remaining open triangles to the closed list, and then
			// remove any triangles that share a vertex with the supertriangle.
			closed.AddRange(open);

			i = closed.Count;
			for (i--; i >= 0; i--)
			{
				if (closed[i].A.__S || closed[i].B.__S || closed[i].C.__S)
				{
					closed.RemoveAt(i);
				}
			}

			return closed;
		}
	}
}
