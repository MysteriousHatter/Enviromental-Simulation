﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnityMeshSimplifier
{
    /// <summary>
    /// A symmetric matrix.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SymmetricMatrix
    {
        #region Fields
        /// <summary>
        /// The m11 component.
        /// </summary>
        public double m0;
        /// <summary>
        /// The m12 component.
        /// </summary>
        public double m1;
        /// <summary>
        /// The m13 component.
        /// </summary>
        public double m2;
        /// <summary>
        /// The m14 component.
        /// </summary>
        public double m3;
        /// <summary>
        /// The m22 component.
        /// </summary>
        public double m4;
        /// <summary>
        /// The m23 component.
        /// </summary>
        public double m5;
        /// <summary>
        /// The m24 component.
        /// </summary>
        public double m6;
        /// <summary>
        /// The m33 component.
        /// </summary>
        public double m7;
        /// <summary>
        /// The m34 component.
        /// </summary>
        public double m8;
        /// <summary>
        /// The m44 component.
        /// </summary>
        public double m9;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the component value with a specific index.
        /// </summary>
        /// <param name="index">The component index.</param>
        /// <returns>The value.</returns>
        public double this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                switch (index)
                {
                    case 0:
                        return m0;
                    case 1:
                        return m1;
                    case 2:
                        return m2;
                    case 3:
                        return m3;
                    case 4:
                        return m4;
                    case 5:
                        return m5;
                    case 6:
                        return m6;
                    case 7:
                        return m7;
                    case 8:
                        return m8;
                    case 9:
                        return m9;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a symmetric matrix with a value in each component.
        /// </summary>
        /// <param name="c">The component value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SymmetricMatrix(double c)
        {
            this.m0 = c;
            this.m1 = c;
            this.m2 = c;
            this.m3 = c;
            this.m4 = c;
            this.m5 = c;
            this.m6 = c;
            this.m7 = c;
            this.m8 = c;
            this.m9 = c;
        }

        /// <summary>
        /// Creates a symmetric matrix.
        /// </summary>
        /// <param name="m0">The m11 component.</param>
        /// <param name="m1">The m12 component.</param>
        /// <param name="m2">The m13 component.</param>
        /// <param name="m3">The m14 component.</param>
        /// <param name="m4">The m22 component.</param>
        /// <param name="m5">The m23 component.</param>
        /// <param name="m6">The m24 component.</param>
        /// <param name="m7">The m33 component.</param>
        /// <param name="m8">The m34 component.</param>
        /// <param name="m9">The m44 component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SymmetricMatrix(double m0, double m1, double m2, double m3,
            double m4, double m5, double m6, double m7, double m8, double m9)
        {
            this.m0 = m0;
            this.m1 = m1;
            this.m2 = m2;
            this.m3 = m3;
            this.m4 = m4;
            this.m5 = m5;
            this.m6 = m6;
            this.m7 = m7;
            this.m8 = m8;
            this.m9 = m9;
        }

        /// <summary>
        /// Creates a symmetric matrix from a plane.
        /// </summary>
        /// <param name="a">The plane x-component.</param>
        /// <param name="b">The plane y-component</param>
        /// <param name="c">The plane z-component</param>
        /// <param name="d">The plane w-component</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SymmetricMatrix(double a, double b, double c, double d)
        {
            this.m0 = a * a;
            this.m1 = a * b;
            this.m2 = a * c;
            this.m3 = a * d;

            this.m4 = b * b;
            this.m5 = b * c;
            this.m6 = b * d;

            this.m7 = c * c;
            this.m8 = c * d;

            this.m9 = d * d;
        }

        /// <summary>
        /// Creates a symmetric matrix from a plane defined by normal n and point p.
        /// Normal n must be normalized before calling the constructor and p is a point in the plane
        /// </summary>
        /// <param name="n">normalized plane normal</param>
        /// <param name="p">a point in the plane (mostly one of its vertices)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SymmetricMatrix(Vector3d n, Vector3d p)
        {
            double d = -(n.x * p.x + n.y * p.y + n.z * p.z);

            this.m0 = n.x * n.x;
            this.m1 = n.x * n.y;
            this.m2 = n.x * n.z;
            this.m3 = n.x * d;

            this.m4 = n.y * n.y;
            this.m5 = n.y * n.z;
            this.m6 = n.y * d;

            this.m7 = n.z * n.z;
            this.m8 = n.z * d;

            this.m9 = d * d;
        }
        #endregion

        #region Operators
        /// <summary>
        /// Adds two matrixes together.
        /// </summary>
        /// <param name="a">The left hand side.</param>
        /// <param name="b">The right hand side.</param>
        /// <returns>The resulting matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SymmetricMatrix operator +(SymmetricMatrix a, SymmetricMatrix b)
        {
            return new SymmetricMatrix(
                a.m0 + b.m0, a.m1 + b.m1, a.m2 + b.m2, a.m3 + b.m3,
                a.m4 + b.m4, a.m5 + b.m5, a.m6 + b.m6,
                a.m7 + b.m7, a.m8 + b.m8,
                a.m9 + b.m9
            );
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Determinant(0, 1, 2, 1, 4, 5, 2, 5, 7)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal double Determinant1()
        {
            double det =
                m0 * m4 * m7 +
                m2 * m1 * m5 +
                m1 * m5 * m2 -
                m2 * m4 * m2 -
                m0 * m5 * m5 -
                m1 * m1 * m7;
            return det;
        }

        /// <summary>
        /// Determinant(1, 2, 3, 4, 5, 6, 5, 7, 8)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal double Determinant2()
        {
            double det =
                m1 * m5 * m8 +
                m3 * m4 * m7 +
                m2 * m6 * m5 -
                m3 * m5 * m5 -
                m1 * m6 * m7 -
                m2 * m4 * m8;
            return det;
        }

        /// <summary>
        /// Determinant(0, 2, 3, 1, 5, 6, 2, 7, 8)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal double Determinant3()
        {
            double det =
                m0 * m5 * m8 +
                m3 * m1 * m7 +
                m2 * m6 * m2 -
                m3 * m5 * m2 -
                m0 * m6 * m7 -
                m2 * m1 * m8;
            return det;
        }

        /// <summary>
        /// Determinant(0, 1, 3, 1, 4, 6, 2, 5, 8)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal double Determinant4()
        {
            double det =
                m0 * m4 * m8 +
                m3 * m1 * m5 +
                m1 * m6 * m2 -
                m3 * m4 * m2 -
                m0 * m6 * m5 -
                m1 * m1 * m8;
            return det;
        }

        /// <summary>
        /// Evaluate the shape of the quadric surface. The shape is not good for findind the optimal vertex location if:
        /// one eigenvalue is zero or the algorithm do not converge
        /// reference material:
        /// https://people.inf.ethz.ch/arbenz/ewp/Lnotes/chapter3.pdf sectionn 3.2.3
        /// https://mathemanu.github.io/conics.pdf for triplet definition
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool ShapeIsGood()
        {
            // characteristic equation is :  L^3 + I*L^2 + J * L - D = 0
            double D = Determinant1();
            double I = m0 + m4 + m7;
            double J = m0 * m7 - m2 * m2 + m4 * m7 - m5 * m5 + m0 * m4 - m1 * m1;
            double[] eigVal = { m0, m4, m7 };
            double[] fVal = { Double.MaxValue, Double.MaxValue, Double.MaxValue }; // for debug
            int[] triplet = { 0, 0, 0 }; // number of {positive eigenvalues, negative eigenvalues, 0 eigenvalues}
            double f, fp, e, g;
            bool isGood = true;
            const double epsilon = 1E-4;
            const int maxIter = 8;
            const double nearZero = 1E-4;

            // Newton method with explicit deflation
            for (int i = 0; i < 3; i++)
            {
                e = eigVal[i];
                int iter = 0;
                f = e * e * e + I * e * e + J * e - D;

                do
                {
                    fp = 3 * e * e + 2 * I * e + J;

                    if (i == 1)
                    {
                        g = e - eigVal[0];
                        fp = (g * fp - f) / (g * g);
                        f = f / g;
                    }
                    else if (i == 2)
                    {
                        g = (e - eigVal[0]) * (e - eigVal[1]);
                        fp = (g * fp - f * (2 * e - eigVal[0] - eigVal[1])) / (g * g);
                        f = f / g;
                    }

                    e = e - f / fp;
                    f = e * e * e + I * e * e + J * e - D;

                } while ((Math.Abs(f) > epsilon) && (iter++ < maxIter));

                eigVal[i] = e;
                fVal[i] = f;

                if ((Math.Abs(f) > epsilon) || (Math.Abs(e) < nearZero)) // conclude to a zero eigenvalue
                {
                    triplet[2]++;
                    //quit early
                    DebugMeshPerf.Data.Triplets[2]++;
                    isGood = false;
                    return isGood;
                }
                else if (e > 0) // positive eigenvalue
                    triplet[0]++;
                else // and negative
                    triplet[1]++;
            }

            if (triplet[2] > 0)
                DebugMeshPerf.Data.Triplets[2]++;
            else if (triplet[1] > 0)
                DebugMeshPerf.Data.Triplets[1]++;
            else
                DebugMeshPerf.Data.Triplets[0]++;

            // analyze the triplet
            isGood = triplet[2] == 0;

            return isGood;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Computes the determinant of this matrix.
        /// </summary>
        /// <param name="a11">The a11 index.</param>
        /// <param name="a12">The a12 index.</param>
        /// <param name="a13">The a13 index.</param>
        /// <param name="a21">The a21 index.</param>
        /// <param name="a22">The a22 index.</param>
        /// <param name="a23">The a23 index.</param>
        /// <param name="a31">The a31 index.</param>
        /// <param name="a32">The a32 index.</param>
        /// <param name="a33">The a33 index.</param>
        /// <returns>The determinant value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Determinant(int a11, int a12, int a13,
            int a21, int a22, int a23,
            int a31, int a32, int a33)
        {
            double det =
                this[a11] * this[a22] * this[a33] +
                this[a13] * this[a21] * this[a32] +
                this[a12] * this[a23] * this[a31] -
                this[a13] * this[a22] * this[a31] -
                this[a11] * this[a23] * this[a32] -
                this[a12] * this[a21] * this[a33];
            return det;
        }

        /// <summary>
        /// Add quadrics matrix of plane P defined by its normal n and a point p in the plane
        /// </summary>
        /// <param name="n"></param>
        /// <param name="p"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ref Vector3d n, ref Vector3d p, double scale = 1.0)
        {
            double d = -Vector3d.Dot(ref n, ref p);

            this.m0 += n.x * n.x * scale;
            this.m1 += n.x * n.y * scale;
            this.m2 += n.x * n.z * scale;
            this.m3 += n.x * d * scale;

            this.m4 += n.y * n.y * scale;
            this.m5 += n.y * n.z * scale;
            this.m6 += n.y * d * scale;

            this.m7 += n.z * n.z * scale;
            this.m8 += n.z * d * scale;

            this.m9 += d * d * scale;
        }
        /// <summary>
        /// Add coefficients of matrix b
        /// </summary>
        /// <param name="b"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(SymmetricMatrix b)
        {
            this.m0 += b.m0; this.m1 += b.m1; this.m2 += b.m2; this.m3 += b.m3;
            this.m4 += b.m4; this.m5 += b.m5; this.m6 += b.m6;
            this.m7 += b.m7; this.m8 += b.m8;
            this.m9 += b.m9;
        }
        /// <summary>
        /// Subtract quadrics matrix of plane P defined by its normal n and a point p in the plane
        /// </summary>
        /// <param name="n"></param>
        /// <param name="p"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subtract(ref Vector3d n, ref Vector3d p)
        {
            double d = -Vector3d.Dot(ref n, ref p);

            this.m0 -= n.x * n.x;
            this.m1 -= n.x * n.y;
            this.m2 -= n.x * n.z;
            this.m3 -= n.x * d;

            this.m4 -= n.y * n.y;
            this.m5 -= n.y * n.z;
            this.m6 -= n.y * d;

            this.m7 -= n.z * n.z;
            this.m8 -= n.z * d;

            this.m9 -= d * d;
        }
        /// <summary>
        /// Subtract coefficients of matrix b
        /// </summary>
        /// <param name="b"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subtract(SymmetricMatrix b)
        {
            this.m0 -= b.m0; this.m1 -= b.m1; this.m2 -= b.m2; this.m3 -= b.m3;
            this.m4 -= b.m4; this.m5 -= b.m5; this.m6 -= b.m6;
            this.m7 -= b.m7; this.m8 -= b.m8;
            this.m9 -= b.m9;
        }

        /// <summary>
        /// Multiply all coefficients by scalar a
        /// </summary>
        /// <param name="c"></param>
        public void Multiply(double a)
        {
            this.m0 *= a; this.m1 *= a; this.m2 *= a; this.m3 *= a;
            this.m4 *= a; this.m5 *= a; this.m6 *= a;
            this.m7 *= a; this.m8 *= a;
            this.m9 *= a;
        }

        /// <summary>
        /// Set coefficients to 0
        /// </summary>
        /// <param name="c"></param>
        public void Clear()
        {
            this.m0 = 0; this.m1 = 0; this.m2 = 0; this.m3 = 0;
            this.m4 = 0; this.m5 = 0; this.m6 = 0;
            this.m7 = 0; this.m8 = 0;
            this.m9 = 0;
        }


        public override string ToString()
        {
            return string.Format("M4X4 [ [({0:F4}),({1:F4}),({2:F4}),({3:F4})] , [({4:F4}),({5:F4}),({6:F4})] , [({7:F4}),({8:F4})] , [({9:F4})]]", m0, m1, m2, m3, m4, m5, m6, m7, m8, m9);
        }
        #endregion
    }
}