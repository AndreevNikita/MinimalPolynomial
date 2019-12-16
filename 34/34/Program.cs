using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _34
{
	//Written by Nikita Andreev
	//http://vk.com/214100294
	//Вспомогательные функции
	class Utils {
		//Функция, возвращающая остаток от деления a на b
		public static int clockMod(int a, int b) {
			if(a >= 0)
				return a % b;
			else
				return b > 0 ? a % b + b : a % b;
		}

		public static void swap(ref int a, ref int b) {
			int buffer = a;
			a = b;
			b = buffer;
		}
		public static int gcd(int a, int b) {
			if(a < b)
				swap(ref a, ref b);
			while(b != 0) {
				a %= b;
				swap(ref a, ref b);
			}
			return a;
		}

		//d = GCD(a, b)
		//a*u + b*v = d
		public static int gcdExt(int a, int b, out int u, out int v) {
			if(b == 0) {
				u = 1; v = 0;
				return a;
			}

			int uNext, vNext;
			int d = gcdExt(b, a % b, out uNext, out vNext);
			u = vNext;
			v = uNext - vNext * (a / b);
			return d;
		}

		//Получить такой x, при котором a*x mod m = b
		public static int getMultiplier(int a, int m, int b) {
			int u, v;
			int gcd = gcdExt(a, m, out u, out v);
			if(gcd != 1) {
				return (b % gcd == 0) ? getMultiplier(a / gcd, m / gcd, b / gcd) : -1;
			} else {
				return clockMod(u * b, m);
			}
		}

		public static int reverseNumber(int a, int m) {
			int u, v;
			return gcdExt(a, m, out u, out v) == 1 ? u : -1;
		}

		public static int pow(int x, uint pow) {
			int ret = 1;
			while ( pow != 0 ) {
				if ( (pow & 1) == 1 )
					ret *= x;
				x *= x;
				pow >>= 1;
			}
			return ret;
		}
	}

	class ClockSystem {
		public int M;
		public ClockSystem(int m) {
			M = m;
		}

		public static bool operator==(ClockSystem a, ClockSystem b) {
			return a.M == b.M;
		}

		public static bool operator!=(ClockSystem a, ClockSystem b) {
			return a.M != b.M;
		}

		public static implicit operator ClockSystem(int m) {
			return new ClockSystem(m);
		}

		public ClockPolynomial newClockPolynomial() {
			return new ClockPolynomial(this);
		}

		public int toSystem(int number) {
			return Utils.clockMod(number, M);
		}
	}

	class ClockPolynomial {
		ClockSystem system;
		private List<int> coeffs = new List<int>();
		
		//Свойства полинома
		public int Length { get => coeffs.Count; }
		public int GrandPower { get => Length - 1; }
		public int GrandCoef { get => this[GrandPower]; }
		public bool IsNull { get => Length == 0; }
		public int M { get => system.M; }

		public ClockPolynomial(ClockSystem system) {
			this.system = system;
		}

		public ClockPolynomial(ClockSystem system, params int[] coeffs) : this(system) {
			for(int index = 0; index < coeffs.Length; index++)
				this[index] = coeffs[index];
		}

		public ClockPolynomial(ClockPolynomial src) : this(src.system) {
			for(int index = 0; index < src.Length; index++)
				this[index] = src[index];
		}

		void removeNulls() {

			int removeCount = 0;
			for(int index = coeffs.Count - 1; index >= 0 && coeffs[index] == 0; index--, removeCount++);

			if(removeCount != 0)
				coeffs.RemoveRange(coeffs.Count - removeCount, removeCount);
		}

		public ClockPolynomial clone() { return new ClockPolynomial(this); }

		public int this[int coeffIndex] {
			get => coeffIndex < coeffs.Count ? coeffs[coeffIndex] : 0;
			set {
				if(coeffIndex >= coeffs.Count) {
					if(value == 0)
						return;
					coeffs.AddRange(new int[coeffIndex - coeffs.Count + 1]);
				}
				coeffs[coeffIndex] = Utils.clockMod(value, M);
				removeNulls();
			}
		}

		public static ClockPolynomial operator+(ClockPolynomial a, ClockPolynomial b) {
			ClockPolynomial result = new ClockPolynomial(a.system);
			int resultLength = Math.Max(a.Length, b.Length);
			for(int index = 0; index < resultLength; index++) {
				result[index] = a[index] + b[index];
			}
			return result;
		}

		public static ClockPolynomial operator-(ClockPolynomial a, ClockPolynomial b) {
			ClockPolynomial result = new ClockPolynomial(a.system);
			int resultLength = Math.Max(a.Length, b.Length);
			for(int index = 0; index < resultLength; index++) {
				result[index] = a[index] - b[index];
			}
			return result;
		}

		public static ClockPolynomial operator*(ClockPolynomial a, ClockPolynomial b) {
			ClockPolynomial result = new ClockPolynomial(a.system);
			for(int aIndex = 0; aIndex < a.Length; aIndex++)
				for(int bIndex = 0; bIndex < b.Length; bIndex++) {
					result[aIndex + bIndex] += a[aIndex] * b[bIndex];
				}
			return result;
		}

		public static ClockPolynomial operator*(ClockPolynomial a, int multiplier) {
			ClockPolynomial result = new ClockPolynomial(a.system);
			for(int index = 0; index < a.Length; index++)
				result[index] = a[index] * multiplier;
			return result;
		}

		public static bool operator==(ClockPolynomial a, ClockPolynomial b) {
			if(a.Length != b.Length)
				return false;

			for(int index = 0; index < a.Length; index++)
				if(a[index] != b[index])
					return false;

			return true;
		}

		public static bool operator!=(ClockPolynomial a, ClockPolynomial b) {
			return !(a == b);
		}

		public static bool operator==(ClockPolynomial a, int b) {
			return b == 0 ? a.IsNull : a.Length == 1 && a.GrandCoef == b;
		}

		public static bool operator!=(ClockPolynomial a, int b) {
			return b == 0 ? !a.IsNull : a.Length > 1 || a.GrandCoef != b;
		}

		public static void div(ClockPolynomial a, ClockPolynomial b, out ClockPolynomial q, out ClockPolynomial r) {
			r = a.clone();
			q = new ClockPolynomial(a.system);
			while(r.Length >= b.Length) {
				int multiplierCoeff = Utils.getMultiplier(b.GrandCoef, b.M, r.GrandCoef);
				ClockPolynomial multiplierPolynomial = new ClockPolynomial(a.system);
				multiplierPolynomial[r.GrandPower - b.GrandPower] = multiplierCoeff;
				q += multiplierPolynomial;
				r -= b * multiplierPolynomial;
			}
		}

		public static ClockPolynomial gcdExt(ClockPolynomial a, ClockPolynomial b, out ClockPolynomial u, out ClockPolynomial v) {
			if(b == 0) {
				u = new ClockPolynomial(a.system, 1); v = new ClockPolynomial(a.system, 0);
				return a;
			}

			ClockPolynomial uNext, vNext;
			ClockPolynomial q, r;
			ClockPolynomial.div(a, b, out q, out r);
			ClockPolynomial d = gcdExt(b, r, out uNext, out vNext);
			u = vNext;
			v = uNext - vNext * q;
			return d;
		}

		public static ClockPolynomial gcdExt1(ClockPolynomial a, ClockPolynomial b, out ClockPolynomial u, out ClockPolynomial v) {
			ClockPolynomial d = gcdExt(a, b, out u, out v);
			int dGrandReverse = Utils.reverseNumber(d.GrandCoef, d.M);
			d *= dGrandReverse;
			u *= dGrandReverse;
			v *= dGrandReverse;
			return d;
		}

		public ClockPolynomial negative() {
			return new ClockPolynomial(system, 0) - this;
		}

		public override string ToString() {
			return ToString('x');
		}

		public string ToString(char c) {
			if(IsNull)
				return "0";

			string result = "";
			for(int index = 0; index < Length; index++) {
				int coeff = this[index];
				if(coeff == 0)
					continue;

				string powString = "";
				if(index > 0) {
					powString += c;
					if(index > 1) {
						powString = powString + "^" + index;
					}
				}

				if(coeff > 1 || index == 0)
					powString = coeff + powString;

				result = powString + result;

				if(index != GrandPower) {
					result = " + " + result;
				}
			}
			return result;
		}

		public void print(string name = null) {
			Console.WriteLine((name != null ? name + "(x) = " : "") + this);
		}

		public static int ord(ClockPolynomial a, ClockPolynomial m, bool print = false) {
			int powCounter = 0;
			ClockPolynomial lastResult = new ClockPolynomial(a.system, 1);
			if(print)
				Console.WriteLine("({0})^{1} = {2}", a, 0, 1);
			do { 
				ClockPolynomial q, r;
				ClockPolynomial.div(lastResult * a, m, out q, out r);
				lastResult = r;

				powCounter++;
				if(print)
					Console.WriteLine("({0})^{1} = {2}", a, powCounter, lastResult);
				
			} while(lastResult != 1);
			return powCounter;
		}
	}

	class PowersTable {
		ClockSystem system;
		List<ClockPolynomial> powers = new List<ClockPolynomial>();
		public int ORD { get => powers.Count; }

		public PowersTable(ClockPolynomial a, ClockPolynomial m, bool print = false) {
			system = a.M;
			int powCounter = 0;
			ClockPolynomial lastResult = new ClockPolynomial(a.M, 1);
			if(print)
				Console.WriteLine("({0})^{1} = {2}", a, 0, 1);
			do { 
				powers.Add(lastResult);
				ClockPolynomial q, r;
				ClockPolynomial.div(lastResult * a, m, out q, out r);
				lastResult = r;

				powCounter++;
				if(print)
					Console.WriteLine("({0})^{1} = {2}", a, powCounter, lastResult);
				
			} while(lastResult != 1);
		}

		public ClockPolynomial this[int power] {
			get => powers[Utils.clockMod(power, powers.Count)];
		}
	}

	class Program {

		public static void printList<T>(List<T> list) {
			string line = "";
			for(int index = 0; index < list.Count; index++) {
				line += list[index];
				line += ';';
			}
			Console.WriteLine(line);
		}

		public static List<int> variantsSum(List<int> src, int countInVariant, int startIndex = 0) {
			List<int> result = new List<int>();
			if(countInVariant < 1) {
				result.Add(0);
				return result;
			}

			if(countInVariant == 1) {
				for(int index = startIndex; index < src.Count; index++)
					result.Add(src[index]);
			} else {
				for(int index = startIndex; index < src.Count - countInVariant + 1; index++) {
					//Console.WriteLine("Count: {0}", index);
					List<int> variants = variantsSum(src, countInVariant - 1, index + 1);
					for(int index2 = 0; index2 < variants.Count; index2++) {
						//Console.WriteLine("Variant: {0}", variants[index]);
						result.Add(src[index] + variants[index2]);
					}
				}
			}



			return result;
		}

		public static bool getSign(int xCount, int xIndex) {
			return (xCount - xIndex) % 2 == 0;
		}

		public static void printSumsCount(List<List<int>> coeffs) {
			for(int index = coeffs.Count - 1; index >= 0; index--) {
				Console.Write("(");
				for(int sumIndex = 0; sumIndex < coeffs[index].Count; sumIndex++) {
					Console.Write("a^{0}", coeffs[index][sumIndex]);
					if(sumIndex != coeffs[index].Count - 1)
						Console.Write(" + ");
				}
				
				Console.Write(")x^{0}", index);

				if(index != 0)
					Console.Write(getSign(coeffs.Count, index) ? " + " : " - ");
			}
		}

		public static void printPolynomial(List<ClockPolynomial> coeffs) {
			for(int index = coeffs.Count - 1; index >= 0; index--) {
				Console.Write("({0})x^{1}", coeffs[index].ToString('t'), index);

				if(index != 0)
					Console.Write(" + ");
			}
		}

		public static void printPolynomial(List<ClockPolynomial> coeffs, ClockPolynomial x) {
			for(int index = coeffs.Count - 1; index >= 0; index--) {
				Console.Write("({0})({1})^{2}", coeffs[index], x, index);

				if(index != 0)
					Console.Write(" + ");
			}
		}

		public static void printPolynomial(List<ClockPolynomial> coeffs, List<ClockPolynomial> xs) {
			for(int index = coeffs.Count - 1; index >= 0; index--) {
				Console.Write("({0})({1})", coeffs[index], xs[index]);

				if(index != 0)
					Console.Write(" + ");
			}
		}

		static void Main(string[] args) {
			int aPower = 6;
			int q = 2;
			int m = 4;
			ClockSystem system = new ClockSystem(q);
			ClockPolynomial p = new ClockPolynomial(system, 1, 1, 0, 0, 1);
			p.print("p");

			ClockPolynomial a = new ClockPolynomial(system, 0, 1);
			a.print("a");

			(new ClockPolynomial(system, 0, 1, 1, 1) * new ClockPolynomial(system, 2, 2)).print("s");

			PowersTable aPowers = new PowersTable(a, p, true);

			Console.WriteLine();

			Console.WriteLine("B = a^{0} = {1}", aPower, aPowers[aPower]);
			Console.WriteLine();

			List<int> aRootPowers = new List<int>();
			do {
				int bPow = Utils.pow(q, (uint)aRootPowers.Count + 1);
				int aPow1 = 6 * bPow;
				int aPow =  Utils.clockMod(aPow1, aPowers.ORD);
				Console.WriteLine("B^{0} = a^{1} = a^{2} = {3}", bPow, aPow1, aPow, aPowers[aPow]);
				aRootPowers.Add(aPow);
			} while(aPowers[Utils.pow(q, (uint)aRootPowers.Count)] != aPowers[Utils.pow(q, (uint)0)]);
			aRootPowers.Sort();

			Console.WriteLine();
			Console.Write("M = { ");
			for(int index = 0; index < aRootPowers.Count; index++) {
				Console.Write("a^{0}; ", aRootPowers[index]);
			}
			Console.WriteLine("}");

			Console.Write("m{0}(x) = ", aPower);
			for(int index = 0; index < aRootPowers.Count; index++) {
				Console.Write("(x - a^{0})", aRootPowers[index]);
			}
			Console.Write(" = ");

			List<List<int>> aCoeffs = new List<List<int>>();
			for(int index = 0; index <= aRootPowers.Count; index++) {
				List<int> sums = variantsSum(aRootPowers, aRootPowers.Count - index);
				aCoeffs.Add(sums);
			}
			printSumsCount(aCoeffs);
			Console.Write(" = ");

			for(int index = 0; index < aCoeffs.Count; index++) {
				for(int index2 = 0; index2 < aCoeffs[index].Count; index2++) 
					aCoeffs[index][index2] = Utils.clockMod(aCoeffs[index][index2], aPowers.ORD);
			}
			printSumsCount(aCoeffs);
			Console.Write(" = ");


			List<ClockPolynomial> coeffsPolynomials = new List<ClockPolynomial>();
			for(int index = 0; index < aCoeffs.Count; index++) {
				List<int> aSum = aCoeffs[index];
				ClockPolynomial sumPolynomial = new ClockPolynomial(system);
				for(int index2 = 0; index2 < aSum.Count; index2++) {
					//Console.WriteLine("Sum: {0}", aPowers[index2]);
					if(getSign(aCoeffs.Count + 1, index))
						sumPolynomial += aPowers[aSum[index2]];
					else
						sumPolynomial -= aPowers[aSum[index2]];
				}
				//Console.WriteLine("Add {0}; {1}: {2}", sumPolynomial, index, getSign(aCoeffs.Count + 1, index));
				coeffsPolynomials.Add(sumPolynomial);
			}

			printPolynomial(coeffsPolynomials);

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			Console.Write("Ответ: m{0}(x) = ", aPower);
			printPolynomial(coeffsPolynomials);
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("Проверка: ");
			for(int index = 0; index < aRootPowers.Count; index++) {
				int power = aRootPowers[index];
				Console.Write("m{0}(a^{1}) = ", aPower, power);
				printPolynomial(coeffsPolynomials, aPowers[power]);
				Console.Write(" = ");
				List<ClockPolynomial> powersList = new List<ClockPolynomial>();
				for(int xIndex = 0; xIndex < aCoeffs.Count; xIndex++)
					powersList.Add(aPowers[power * xIndex]);

				//for(int index = 0; index < )
				printPolynomial(coeffsPolynomials, powersList);
				Console.Write(" = ");
				ClockPolynomial result = new ClockPolynomial(system);
				for(int xIndex = 0; xIndex < aCoeffs.Count; xIndex++) {
					result += coeffsPolynomials[xIndex] * powersList[xIndex];
				}
				ClockPolynomial resultQ, resultR;
				ClockPolynomial.div(result, p, out resultQ, out resultR);
				Console.Write(resultR);

				Console.WriteLine();
				Console.WriteLine();
			}
			Console.ReadKey();
		}
	}
}