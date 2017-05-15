using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
	class Parent
	{
		public Parent()
		{
			Console.WriteLine("Constructor (PARENT)");
		}

		public virtual void Initialization()
		{
			Console.WriteLine("Initialization (PARENT)");
		}
	}

	class Child : Parent
	{
		public Child()
		{
			Console.WriteLine("Constructor (Child)");
			Initialization();
		}

		public sealed override void Initialization()
		{
//			base.Initialization();
			Console.WriteLine("Initialization (CHILD)");
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
//			Parent parent = new Parent();
			Parent parentChild = new Child();
//			parentChild.Initialization();

			Console.ReadLine();
		}
	}
}
