using System;

namespace Teamwork
{
    class Program
    {
        static void Main()
        {
            int n = int.Parse(Console.ReadLine()); //количество чисел
            string[] numbers = Console.ReadLine().Split(' '); // Васины числа
            for (int i = 0, sum = 0; i < n; i++)
            {
                sum++;
                if (i + 1 != n && numbers[i] != numbers[i + 1])
                {
                    Console.Write(sum + " " + numbers[i] + " ");
                    sum = 0;
                }
                else if (i + 1 == n)
                {
                    Console.Write(sum + " " + numbers[i]);
                }
            }
        }
    }
}
