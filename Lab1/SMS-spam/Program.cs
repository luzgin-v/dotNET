using System;


namespace SMS_spam
{
    class Program
    {
        static void Main()
        {
            string line = Console.ReadLine(); //исходный рекламный слоган
            int cost = 0; //стоимость
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] > 96)
                {
                    //расчет стоимости буквенных символов
                    cost += (line[i] - 97) % 3 + 1;
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
