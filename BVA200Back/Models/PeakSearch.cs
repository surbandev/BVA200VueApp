using System;
using System.Collections.Generic;
using System.Linq;

namespace BVA200.Models
{
    /// <summary>
    /// Second Difference method used by canberra software
    /// </summary>
    public class CanberraPeakFinder
    {
        const double _startExpectedEnergy = 50f;
       
        /// <summary>
        /// Finds peaks for a given spectrum, assuming expected FWHM of 14 channels
        /// </summary>
        /// <param name="spect">Spectrum used to find peaks</param>
        /// <returns>Collection of peaks found</returns>
        public IEnumerable<Peak> FindPeaks(UInt32[] spect)
        {
            if (spect == null)
                throw new ArgumentNullException("spect");

            var segments = new List<Peak>();
            var goodSegments = new List<Peak>();
            var spectSize = spect.Length;

            try
            {

                var ssi = new double[spectSize];
                var dd = ComputeDdi(spect, _startExpectedEnergy);
                var sd = ComputeSdi(spect, _startExpectedEnergy);

                Peak runningSegment = null;
                for (int i = 0; i < spectSize - 1; i++)
                {
                    // ReSharper disable CompareOfFloatsByEqualityOperator
                    if (dd[i] == 0)
                    {
                        continue;
                    }
                    // ReSharper restore CompareOfFloatsByEqualityOperator
                    ssi[i] = dd[i]/sd[i];

                    if (ssi[i] < 0)
                    {
                        if (runningSegment == null)
                        {
                            runningSegment = new Peak {Start = i};
                        }
                    }
                    else
                    {
                        if (runningSegment != null)
                        {
                            runningSegment.End = i - 1;
                            segments.Add(runningSegment);
                            runningSegment = null;
                        }
                    }
                }

                //Find centroid in segments
                foreach (var s in segments)
                {
                    var sumChanelSSi = 0d;
                    double sumSSi = 0;
                    for (int i = s.Start; i <= s.End; i++)
                    {
                        sumChanelSSi += (i + 1)*ssi[i];
                        sumSSi += ssi[i];
                    }
                    s.Centroid = Math.Round(sumChanelSSi/sumSSi, 2);
                    s.Significance = Math.Round(Math.Abs(ssi[(int) s.Centroid]), 2);
                    s.ExpectedFullWidthHalfMax = Math.Round(ComputeExpectedFullWidthHalfMax((int) s.Centroid), 2);

                    if (s.Significance >= 3)
                    {
                        if (s.Significance < 20)
                        {
                            // we want to check if this segment is of appropriete size
                            // cmp size of segment with the expected FWHM for that region
                            if ((s.End - s.Start) >= (0.5*s.ExpectedFullWidthHalfMax))
                                goodSegments.Add(s);
                        }
                        else if ((s.End - s.Start) >= 3)
                        {
                            goodSegments.Add(s);
                        }
                    }
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            {
                // Log here
            }
            // ReSharper restore EmptyGeneralCatchClause

            return goodSegments;
        }

        /// <summary>
        /// Returns peaks within percentWithinMaxSignificance from highest significance
        /// </summary>
        /// <param name="spect">Spectrum used to search for peaks</param>
        /// <param name="percentWithinMaxSignificance">Percent as int 5 -> 5%, 10 -> 10%, etc</param>
        /// <returns>Returns collection of found peaks</returns>
        public IEnumerable<Peak> FindPeaks(UInt32[] spect, Int32 percentWithinMaxSignificance)
        {
            var peaksOfInterest = Enumerable.Empty<Peak>();
            try
            {
                var pWithinMaxSignificance = (double)percentWithinMaxSignificance / 100.0f;
                var peaks = FindPeaks(spect).ToList();

                if (peaks.Any())
                {
                    var maxSignificance = (from p in peaks select p.Significance).Max();

                    peaksOfInterest = (from p in peaks
                                       where p.Significance > (maxSignificance * pWithinMaxSignificance)
                                       orderby p.Centroid descending
                                       select p);
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            {
                //log here
            }
            // ReSharper restore EmptyGeneralCatchClause

            return peaksOfInterest;
        }

        /// <summary>
        /// Returns found peaks from spectrum within a given percent of max found signficance.
        /// </summary>
        /// <param name="spectrum">Spectrum used to find peaks</param>
        /// <param name="withinMaxSignificancePercent">Percent below the maximum signifance level of found peaks (example. 20, 25, 30, 40)</param>
        /// <param name="numHighSignificancePeaks">Number of highest-significance peaks to return</param>
        /// <returns>Collection of found peaks (with at most numHighSignificancePeaks members) 
        /// below withinMaxSignificancePercent sorted ascendingly</returns>
        public IEnumerable<Peak> FindPeaksSortedByCentroid(UInt32[] spectrum, Int32 withinMaxSignificancePercent, int numHighSignificancePeaks)
        {
            var foundPeaks = new List<Peak>(FindPeaks(spectrum, withinMaxSignificancePercent));

            var peaksOfInterest = (from p in foundPeaks orderby p.Significance descending select p).Take(numHighSignificancePeaks);
            var peaksSortedByCentroid = peaksOfInterest.OrderBy(p => p.Centroid).ToList();
            return peaksSortedByCentroid;
        }

        double[] ComputeDdi(uint[] spect, double expectedEnergy)
        {
            var ddi = new double[spect.Length * 2];
            var s = new double[spect.Length * 2];
            Array.Copy(spect, s, spect.Length);

            var expectedFullWidthHalfMax = ComputeExpectedFullWidthHalfMax(expectedEnergy);
            var coeff = ComputeCoefficients(expectedFullWidthHalfMax);
            var k = coeff.Count();
            var kInitial = k;

            for (int i = kInitial - 1; i < 1023 - kInitial + 1; i++)
            {
                if (i % 100 == 0 && i < 700)
                {
                    coeff = ComputeCoefficients(ComputeExpectedFullWidthHalfMax(i));
                    k = coeff.Count();
                }
                double sum = 0;
                var j = k - 1;
                while (j > 0)
                {
                    sum += checked(((coeff[j] * s[i - j]) + (coeff[j] * s[i + j])));
                    j--;
                }
                sum += coeff[0] * s[i];
                ddi[i] = sum;
            }

            return ddi;
        }
        double[] ComputeSdi(uint[] spect, double expectedEnergy)
        {
            var sdi = new double[spect.Length * 2];
            var s = new double[spect.Length * 2];
            Array.Copy(spect, s, spect.Length);

            var expectedFullWidthHalfMax = ComputeExpectedFullWidthHalfMax(expectedEnergy);
            var coeff = ComputeCoefficients(expectedFullWidthHalfMax);
            var k = coeff.Count();
            var kInitial = k;

            for (int i = kInitial - 1; i < 1023 - kInitial + 1; i++)
            {
                if (i % 100 == 0 && i < 700)
                {
                    coeff = ComputeCoefficients(ComputeExpectedFullWidthHalfMax(i));
                    k = coeff.Count();
                }
                double sum = 0;
                var j = k - 1;
                while (j > 0)
                {
                    sum += checked(((Math.Pow(coeff[j], 2) * s[i - j]) + (Math.Pow(coeff[j], 2) * s[i + j])));
                    j--;
                }
                sum += Math.Pow(coeff[0], 2) * s[i];
                sdi[i] = Math.Sqrt(sum);
            }
            return sdi;
        }

        double ComputeExpectedFullWidthHalfMax(double energy)
        {
            // Canberra software came up with the following function
            // FWHM = 1.069 + 2.137 * Energy^(1/2)
            // and FWHM = 1.114 + 2.228 * Energy^(1/2)
            // we will use the first one just because it gives us a little bit
            // lower results (of course this is very system unique but it should be
            // appropriate for the same family of detectors
            return 1 + 2 * Math.Pow(energy, 0.5);
        }

        List<double> ComputeCoefficients(double expectedFullWidthHalfMax)
        {
            var coefficents = new List<double>();
            var s = (2 * Math.Sqrt(2 * Math.Log(2, Math.E)));
            var cw = expectedFullWidthHalfMax / s;
            double cj = -100;

            //The first cooficient c0 is always -100
            coefficents.Add(cj);
            var j = 1;
            do
            {
                cj = (100 * (Math.Pow(j, 2) - Math.Pow(cw, 2)) / Math.Pow(cw, 2)) * Math.Pow(Math.E, (-1 * (Math.Pow(j, 2) / (2 * Math.Pow(cw, 2)))));

                if (Math.Abs(cj) < 1)
                    break;

                coefficents.Add(cj);
                j++;

            } while (true);

            coefficents[1] -= coefficents.Sum();

            return coefficents;
        }
    }

}
