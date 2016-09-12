using System;


namespace SMS_spam
{
    class Program
    {
        static void Main(string[] args)
        {
            string line = Console.ReadLine();
            int cost = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if ((int)line[i] > 96)
                {
                    cost += ((int)line[i] - 97) % 3 + 1;
                }
                else if (line[i] == ' ' || line[i] == '.')
                {
                    cost++;
                }
                else if (line[i] == ',')
                {
                    cost += 2;
                }
                else if (line[i] == '!')
                {
                    cost += 3;
                }
            }
            Console.WriteLine(cost);
        }
    }
}
