using System;

namespace Hotel
{
    class Program
    {
        static void Main(string[] args)
        {
            int n = int.Parse(Console.ReadLine());
            int[,] result = new int[n,n];
            int i = 0, j = n-1, number = 1, start_pos = j;
            do
            {
                result[i, j] = number;
                number++;
                Console.WriteLine(i);
                if (j + 1 == n)
                {
                    Console.WriteLine("+"+i);
                    start_pos--;
                    j = start_pos;
                    i = 0;
                }
                else if (i + 1 == n)
                {
                    Console.WriteLine("-"+i);
                    start_pos++;
                    i = start_pos;
                    j = 0;
                }
                else 
                {
                    j++;
                    i++;
                }
            } while (i != n-1 & j != 0);
            for (i = 0; i<n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    Console.Write(result[i,j]+" ");
                }
                Console.Write("\n");
            }
            Console.ReadLine();
        }
    }
}
