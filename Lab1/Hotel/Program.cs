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
                if (i + 1 == n)
                {
                    start_pos++;
                    i = start_pos;
                    j = 0;
                }
                else if (j + 1 == n)
                {
                    start_pos--;
                    j = start_pos;
                    i = 0;
                }
                else 
                {
                    j++;
                    i++;
                }
            } while (number!=n*n+1);
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
