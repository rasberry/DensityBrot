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
		public static IEnumerable<ulong> SequenceBits(int bitLength, ulong initialState = 31L, bool repeat = false)
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
		public static IEnumerable<ulong> SequenceLength(ulong length, ulong initialState = 31L, bool repeat = false)
		{
			//LFSR sequences produce 2^n-1 items (skipping 0) so round up
			int bits = (int)Math.Ceiling(Math.Log(length + 1,2.0));

			do {
				foreach(ulong x in SequenceBits(bits,initialState,false)) {
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
			throw new ArgumentOutOfRangeException("bad bit length "+bitLength);
		}
	}

	// https://en.wikipedia.org/wiki/Mersenne_Twister
	// http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/VERSIONS/C-LANG/mt19937-64.c
	public class MersenneTwister
	{
		const int
			W = 64,                  //word_size
			N = 312,                 //state_size
			M = 156,                 //shift_size
			R = 31,                  //mask_bits
			U = 29,                  //tempering_u
			S = 17,                  //tempering_s
			T = 37,                  //tempering_t
			L = 43                   //tempering_l
		;

		const ulong
			A = 0xb5026f5aa96619e9,  //xor_mask
			D = 0x5555555555555555,  //tempering_d
			B = 0x71d67fffeda60000,  //tempering_b
			C = 0xfff7eee000000000,  //tempering_c
			F = 6364136223846793005, //initialization_multiplier
			defSeed = 0x5489u        //default_seed
		;

		ulong[] MT = new ulong[312];
		ulong index = N + 1;
		//static ulong lowerMask = (1uL << R) - 1; //R number of binary 1's
		static ulong lowerMask = 0x7FFFFFFF;

		//static ulong upperMask = ~lowerMask & ((1uL << W) - 1);
		static ulong upperMask = 0xFFFFFFFF80000000;

		public MersenneTwister(ulong seed = defSeed)
		{
			userSeed = seed;
		}

		ulong userSeed = defSeed;

		// Initialize the generator from a seed
		void Seed(ulong seed)
		{
			index = N;
			MT[0] = seed;
			for(ulong i=1; i<N; i++) {
				MT[i] = (F * (MT[i-1] ^ (MT[i-1] >> (W-2))) + i);
				//MT[i] = ((1uL << W) - 1) & (F * (MT[i-1] ^ (MT[i-1] >> (W-2))) + i);
			}
		}

	 	// Extract a tempered value based on MT[index]
 		// calling twist() every n numbers
		public ulong Extract()
		{
			if (index >= N) {
				if (index > N) {
					Seed(userSeed);
				}
				Twist();
			}

			ulong y = MT[index];
			y = y ^ ((y >> U) & D);
			y = y ^ ((y << S) & B);
			y = y ^ ((y << T) & C);
			y = y ^ (y >> L);

			index++;
			return ((1uL << W) - 1) & y;
		}

		// Generate the next n values from the series x_i
		void Twist()
		{
			for(ulong i=0; i<N; i++) {
				ulong x = (MT[i] & upperMask) + (MT[(i+1) % N] & lowerMask);
				ulong z = x >> 1;
				if (x % 2 != 0) { // lowest bit of x is 1
					z = z ^ A;
				}
				MT[i] = MT[(i + M) % N] ^ z;
			}
			index = 0;
		}
	}
}

// //The 10000th consecutive invocation of a default-contructed std::mt19937 is required to produce the value 4123659995.
// //The 10000th consecutive invocation of a default-contructed std::mt19937_64 is required to produce the value 9981545732273789042
//
// /*
//    A C-program for MT19937-64 (2004/9/29 version).
//    Coded by Takuji Nishimura and Makoto Matsumoto.
//
//    This is a 64-bit version of Mersenne Twister pseudorandom number
//    generator.
//
//    Before using, initialize the state by using init_genrand64(seed)
//    or init_by_array64(init_key, key_length).
//
//    Copyright (C) 2004, Makoto Matsumoto and Takuji Nishimura,
//    All rights reserved.
//
//    Redistribution and use in source and binary forms, with or without
//    modification, are permitted provided that the following conditions
//    are met:
//
//      1. Redistributions of source code must retain the above copyright
//         notice, this list of conditions and the following disclaimer.
//
//      2. Redistributions in binary form must reproduce the above copyright
//         notice, this list of conditions and the following disclaimer in the
//         documentation and/or other materials provided with the distribution.
//
//      3. The names of its contributors may not be used to endorse or promote
//         products derived from this software without specific prior written
//         permission.
//
//    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//    "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//    LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//    A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
//    CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
//    EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//    PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
//    PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//    LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//    NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//    SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//    References:
//    T. Nishimura, ``Tables of 64-bit Mersenne Twisters''
//      ACM Transactions on Modeling and
//      Computer Simulation 10. (2000) 348--357.
//    M. Matsumoto and T. Nishimura,
//      ``Mersenne Twister: a 623-dimensionally equidistributed
//        uniform pseudorandom number generator''
//      ACM Transactions on Modeling and
//      Computer Simulation 8. (Jan. 1998) 3--30.
//
//    Any feedback is very welcome.
//    http://www.math.hiroshima-u.ac.jp/~m-mat/MT/emt.html
//    email: m-mat @ math.sci.hiroshima-u.ac.jp (remove spaces)
// */
//
//
// #include <stdio.h>
//
// #define NN 312
// #define MM 156
// #define MATRIX_A 0xB5026F5AA96619E9ULL
// #define UM 0xFFFFFFFF80000000ULL /* Most significant 33 bits */
// #define LM 0x7FFFFFFFULL /* Least significant 31 bits */
//
//
// /* The array for the state vector */
// static unsigned long long mt[NN];
// /* mti==NN+1 means mt[NN] is not initialized */
// static int mti=NN+1;
//
// /* initializes mt[NN] with a seed */
// void init_genrand64(unsigned long long seed)
// {
//     mt[0] = seed;
//     for (mti=1; mti<NN; mti++)
//         mt[mti] =  (6364136223846793005ULL * (mt[mti-1] ^ (mt[mti-1] >> 62)) + mti);
// }
//
// /* initialize by an array with array-length */
// /* init_key is the array for initializing keys */
// /* key_length is its length */
// void init_by_array64(unsigned long long init_key[],
// 		     unsigned long long key_length)
// {
//     unsigned long long i, j, k;
//     init_genrand64(19650218ULL);
//     i=1; j=0;
//     k = (NN>key_length ? NN : key_length);
//     for (; k; k--) {
//         mt[i] = (mt[i] ^ ((mt[i-1] ^ (mt[i-1] >> 62)) * 3935559000370003845ULL))
//           + init_key[j] + j; /* non linear */
//         i++; j++;
//         if (i>=NN) { mt[0] = mt[NN-1]; i=1; }
//         if (j>=key_length) j=0;
//     }
//     for (k=NN-1; k; k--) {
//         mt[i] = (mt[i] ^ ((mt[i-1] ^ (mt[i-1] >> 62)) * 2862933555777941757ULL))
//           - i; /* non linear */
//         i++;
//         if (i>=NN) { mt[0] = mt[NN-1]; i=1; }
//     }
//
//     mt[0] = 1ULL << 63; /* MSB is 1; assuring non-zero initial array */
// }
//
// /* generates a random number on [0, 2^64-1]-interval */
// unsigned long long genrand64_int64(void)
// {
//     int i;
//     unsigned long long x;
//     static unsigned long long mag01[2]={0ULL, MATRIX_A};
//
//     if (mti >= NN) { /* generate NN words at one time */
//
//         /* if init_genrand64() has not been called, */
//         /* a default initial seed is used     */
//         if (mti == NN+1)
//             init_genrand64(5489ULL);
//
//         for (i=0;i<NN-MM;i++) {
//             x = (mt[i]&UM)|(mt[i+1]&LM);
//             mt[i] = mt[i+MM] ^ (x>>1) ^ mag01[(int)(x&1ULL)];
//         }
//         for (;i<NN-1;i++) {
//             x = (mt[i]&UM)|(mt[i+1]&LM);
//             mt[i] = mt[i+(MM-NN)] ^ (x>>1) ^ mag01[(int)(x&1ULL)];
//         }
//         x = (mt[NN-1]&UM)|(mt[0]&LM);
//         mt[NN-1] = mt[MM-1] ^ (x>>1) ^ mag01[(int)(x&1ULL)];
//
//         mti = 0;
//     }
//
//     x = mt[mti++];
//
//     x ^= (x >> 29) & 0x5555555555555555ULL;
//     x ^= (x << 17) & 0x71D67FFFEDA60000ULL;
//     x ^= (x << 37) & 0xFFF7EEE000000000ULL;
//     x ^= (x >> 43);
//
//     return x;
// }
//
// /* generates a random number on [0, 2^63-1]-interval */
// long long genrand64_int63(void)
// {
//     return (long long)(genrand64_int64() >> 1);
// }
//
// /* generates a random number on [0,1]-real-interval */
// double genrand64_real1(void)
// {
//     return (genrand64_int64() >> 11) * (1.0/9007199254740991.0);
// }
//
// /* generates a random number on [0,1)-real-interval */
// double genrand64_real2(void)
// {
//     return (genrand64_int64() >> 11) * (1.0/9007199254740992.0);
// }
//
// /* generates a random number on (0,1)-real-interval */
// double genrand64_real3(void)
// {
//     return ((genrand64_int64() >> 12) + 0.5) * (1.0/4503599627370496.0);
// }
//
//
// int main(void)
// {
//     int i;
//     unsigned long long init[4]={0x12345ULL, 0x23456ULL, 0x34567ULL, 0x45678ULL}, length=4;
//     init_by_array64(init, length);
//     printf("1000 outputs of genrand64_int64()\n");
//     for (i=0; i<1000; i++) {
//       printf("%20llu ", genrand64_int64());
//       if (i%5==4) printf("\n");
//     }
//     printf("\n1000 outputs of genrand64_real2()\n");
//     for (i=0; i<1000; i++) {
//       printf("%10.8f ", genrand64_real2());
//       if (i%5==4) printf("\n");
//     }
//     return 0;
// }


