using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;
using Aiv.Fast3D;
using OpenTK;

namespace Aiv.Fast3D.Example
{
	class Program
	{
        public static void Main(string[] args)
        {
            Console.WriteLine("=== AivFast2d Example ===");
            Console.WriteLine("Possible examples:");
            Console.WriteLine("[1] Suzanne");
            Console.WriteLine("[2] Orthographic Camera");
            Console.WriteLine("[3] Perspective Camera");
            Console.WriteLine("[4] Skeleton");
            Console.WriteLine("[5] Castle");
            Console.WriteLine();

            int minChoice = 0;
            int maxChoice = 5;
            int choice;
            do
            {
                Console.Write("Pick a number [type 0 to exit]: ");
                string input = Console.ReadLine();

                bool isValidNumber = int.TryParse(input, out choice);
                if (!isValidNumber ||
                    choice < minChoice || choice > maxChoice)
                {
                    Console.WriteLine("Invalid choice!!!");
                }
                else
                {
                    switch (choice)
                    {
                        case 0: break;
                        case 1: SuzanneExample.Run(); break;
                        case 2: OrthographicExample.Run(); break;
                        case 3: PerspectiveExample.Run(); break;
                        case 4: SkeletonExample.Run(); break;
                        case 5: CastleExample.Run(); break;
                    }

                    if (choice == 0) break;
                };
            } while (true);
        }
    }
}
