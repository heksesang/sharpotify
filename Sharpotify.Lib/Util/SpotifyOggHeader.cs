using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sharpotify.Util
{
    internal class SpotifyOggHeader
    {
        #region fields

        private int _unknown = 0;
        private readonly int[] _headerTableDec = new int[] {
		        0,    112,    197,    327,    374,    394,    407,    417,
		      425,    433,    439,    444,    449,    454,    458,    462,
		      466,    470,    473,    477,    480,    483,    486,    489,
		      491,    494,    497,    499,    502,    504,    506,    509,
		      511,    513,    515,    517,    519,    521,    523,    525,
		      527,    529,    531,    533,    535,    537,    538,    540,
		      542,    544,    545,    547,    549,    550,    552,    554,
		      555,    557,    558,    560,    562,    563,    565,    566,
		      568,    569,    571,    572,    574,    575,    577,    578,
		      580,    581,    583,    584,    585,    587,    588,    590,
		      591,    593,    594,    595,    597,    598,    599,    601,
		      602,    604,    605,    606,    608,    609,    610,    612,
		      613,    615,    616,    617,    619,    620,    621,    623,
		      624,    625,    627,    628,    629,    631,    632,    634,
		      635,    636,    638,    639,    640,    642,    643,    644,
		      646,    647,    649,    650,    651,    653,    654,    655,
		      657,    658,    660,    661,    662,    664,    665,    667,
		      668,    669,    671,    672,    674,    675,    677,    678,
		      679,    681,    682,    684,    685,    687,    688,    690,
		      691,    693,    694,    696,    697,    699,    700,    702,
		      704,    705,    707,    708,    710,    712,    713,    715,
		      716,    718,    720,    721,    723,    725,    727,    728,
		      730,    732,    734,    735,    737,    739,    741,    743,
		      745,    747,    748,    750,    752,    754,    756,    758,
		      760,    763,    765,    767,    769,    771,    773,    776,
		      778,    780,    782,    785,    787,    790,    792,    795,
		      797,    800,    803,    805,    808,    811,    814,    817,
		      820,    823,    826,    829,    833,    836,    840,    843,
		      847,    851,    855,    859,    863,    868,    872,    877,
		      882,    887,    893,    898,    904,    911,    918,    925,
		      933,    941,    951,    961,    972,    985,   1000,   1017,
		     1039,   1067,   1108,   1183,   1520,   2658,   4666,   8191
	    };

        #endregion


        #region properties

        public int Size { get; private set; }
        public int Samples { get; private set; }
        public Single GainScale { get; private set; }
        public Single GainDb { get; private set; }
        public int[] Table { get; private set; }

        #endregion


        #region methods

        public int GetLength(int sampleRate)
        {
            return this.Samples / sampleRate;
        }


        /* Swap short bytes. */
        private Int16 Swap(Int16 value)
        {
            return (Int16)(((value & 0x00ff) << 8) |
                            ((value & 0xff00) >> 8));
        }

        /* Swap integer bytes. */
        private int Swap(int value)
        {
            byte[] inBytes = IntUtils.ToBytes(value);
            return IntUtils.BytesToInteger(
                new byte[4] { inBytes[3], inBytes[2], inBytes[1], inBytes[0] });
            /*return ((value & 0x000000ff) << 24) |
                    ((value & 0x0000ff00) << 8) |
                    ((value & 0x00ff0000) >> 8) |
                    ((value & 0xff000000) >> 24);*/
        }

        /* Decode Spotify OGG header. */
	    private void Decode(byte[] header)
        {
		    /* Get input steam of bytes. */
            ByteBuffer input = new ByteBuffer(header);
    		
		    /* Skip OGG page header (length is always 0x1C in this case). */
		    input.Position = 0x1C;
    		
		    /* Read Spotify specific data. */
		    if (input.GetByte() == 0x81)
            {
                while (input.Remaining >= 2)
                {
				    int blockSize = this.Swap(input.GetShort());

                    if (input.Remaining >= blockSize && blockSize > 0)
                    {
					    switch (input.GetByte())
                        {
						    /* Table lookup */
						    case 0:
							    if (blockSize == 0x6e)
                                {
								    this.Samples = this.Swap(input.GetInt());
								    this.Size  = this.Swap(input.GetInt());
								    this._unknown = -this._headerTableDec[input.GetByte()];
								    this.Table   = new int[0x64];
    								
								    int ack = this._unknown;
								    int ctr = 0;
    								
								    for (int i = 0; i < 0x64; i++)
                                    {
									    ack += this._headerTableDec[input.GetByte()];
    									
									    this.Table[ctr] = ack;
								    }
							    }
    							
							    break;
						    /* Gain */
						    case 1:
							    if(blockSize > 0x10)
                                {
								    this.GainDb = 1.0f;
    								
								    int value;
    								
								    if ((value = this.Swap(input.GetInt())) != -1)
                                    {
                                        this.GainDb = FloatUtils.CreateFromIntBits(value);
								    }
    								
								    if (this.GainDb < -40.0f)
                                    {
									    this.GainDb = 0.0f;
								    }
    								
								    this.GainScale = this.GainDb * 0.05f;
								    this.GainScale = (Single)Math.Pow(10.0, this.GainScale);
							    }
    							
							    break;
					    }
				    }
			    }
		    }
	    }

        #endregion


        #region construction

        public SpotifyOggHeader(byte[] header)
        {
            /* Try to parse header. If it fails, just set default values. */
            try
            {
                this.Decode(header);
            }
            catch (Exception)
            {
                this.Samples = 0;
                this.Size = 0;
                this.Table = new int[0];
                this.GainScale = 1.0f;
                this.GainDb = 0.0f;
            }
        }

        #endregion
    }
}
