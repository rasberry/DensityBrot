using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DensityBrot
{
	//https://en.wikipedia.org/wiki/Linear-feedback_shift_register#Galois_LFSRs
	public static class LinearFeedbackShiftRegister
	{
		//sequence by number of bits in the register
		public static IEnumerable<ulong> SequenceBits(ulong initialState, int bitLength, bool repeat = false)
		{
			do {
				ulong lfsr = initialState;
				ulong taps = GetTapConstant(bitLength);

				do {
					ulong lsb = lfsr & 1;
					lfsr >>= 1;
					if (lsb != 0) {
						lfsr ^= taps;
					}
					yield return lfsr;
				} while(lfsr != initialState);
			} while(repeat);
		}

		//sequance by total length (rounded up to nearest bit length)
		public static IEnumerable<ulong> SequenceLength(ulong initialState, ulong length, bool repeat = false)
		{
			//LFSR sequences produce 2^n-1 items (skipping 0) so round up
			int bits = (int)Math.Ceiling(Math.Log(length + 1,2.0));

			do {
				foreach(ulong x in SequenceBits(initialState,bits,false)) {
					//skip numbers above the length since bitLength rounds up
					if (x < length) {
						yield return x;
					}
				}
				//LFSR sequence length is 2^n-1 so we need to produce a '0' to make it 2^n
				yield return 0;
			} while(repeat);
		}

		static ulong GetTapConstant(int bitLength)
		{
			//http://www.xilinx.com/support/documentation/application_notes/xapp052.pdf
			switch(bitLength)
			{
				case 03: return 0b110;
				case 04: return 0b1100;
				case 05: return 0b10100;
				case 06: return 0b110000;
				case 07: return 0b1100000;
				case 08: return 0b10111000;
				case 09: return 0b100010000;
				case 10: return 0b1001000000;
				case 11: return 0b10100000000;
				case 12: return 0b100000101001;
				case 13: return 0b1000000001101;
				case 14: return 0b10000000010101;
				case 15: return 0b110000000000000;
				case 16: return 0b1101000000001000;
				case 17: return 0b10010000000000000;
				case 18: return 0b100000010000000000;
				case 19: return 0b1000000000000100011;
				case 20: return 0b10010000000000000000;
				case 21: return 0b101000000000000000000;
				case 22: return 0b1100000000000000000000;
				case 23: return 0b10000100000000000000000;
				case 24: return 0b111000010000000000000000;
				case 25: return 0b1001000000000000000000000;
				case 26: return 0b10000000000000000000100011;
				case 27: return 0b100000000000000000000010011;
				case 28: return 0b1001000000000000000000000000;
				case 29: return 0b10100000000000000000000000000;
				case 30: return 0b100000000000000000000000101001;
				case 31: return 0b1001000000000000000000000000000;
				case 32: return 0b10000000001000000000000000000011;
				case 33: return 0b100000000000010000000000000000000;
				case 34: return 0b1000000100000000000000000000000011;
				case 35: return 0b10100000000000000000000000000000000;
				case 36: return 0b100000000001000000000000000000000000;
				case 37: return 0b1000000000000000000000000000000011111;
				case 38: return 0b10000000000000000000000000000000110001;
				case 39: return 0b100010000000000000000000000000000000000;
				case 40: return 0b1010000000000000000101000000000000000000;
				case 41: return 0b10010000000000000000000000000000000000000;
				case 42: return 0b110000000000000000000011000000000000000000;
				case 43: return 0b1100011000000000000000000000000000000000000;
				case 44: return 0b11000000000000000000000000110000000000000000;
				case 45: return 0b110110000000000000000000000000000000000000000;
				case 46: return 0b1100000000000000000011000000000000000000000000;
				case 47: return 0b10000100000000000000000000000000000000000000000;
				case 48: return 0b110000000000000000000000000110000000000000000000;
				case 49: return 0b1000000001000000000000000000000000000000000000000;
				case 50: return 0b11000000000000000000000000110000000000000000000000;
				case 51: return 0b110000000000000110000000000000000000000000000000000;
				case 52: return 0b1001000000000000000000000000000000000000000000000000;
				case 53: return 0b11000000000000011000000000000000000000000000000000000;
				case 54: return 0b110000000000000000000000000000000000110000000000000000;
				case 55: return 0b1000000000000000000000001000000000000000000000000000000;
				case 56: return 0b11000000000000000000011000000000000000000000000000000000;
				case 57: return 0b100000010000000000000000000000000000000000000000000000000;
				case 58: return 0b1000000000000000000100000000000000000000000000000000000000;
				case 59: return 0b11000000000000000000011000000000000000000000000000000000000;
				case 60: return 0b110000000000000000000000000000000000000000000000000000000000;
				case 61: return 0b1100000000000001100000000000000000000000000000000000000000000;
				case 62: return 0b11000000000000000000000000000000000000000000000000000000110000;
				case 63: return 0b110000000000000000000000000000000000000000000000000000000000000;
				case 64: return 0b1101100000000000000000000000000000000000000000000000000000000000;
			}
			throw new ArgumentOutOfRangeException();
		}
	}
}
