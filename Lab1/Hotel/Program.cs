using System;

namespace Hotel
{
    class Program
    {
        static void Main()
        {
            int n = int.Parse(Console.ReadLine()); //размер квадратной матрицы
            int[,] result = new int[n,n]; // матрица
            int i = 0, j = n - 1; // номер строки и столбца, предустановлена позиция числа 1 в матрице
            int number = 1; // число для записи в ячейку матрицы
            int startPos = j; //стартовая позиция строки/столбца для начала заполнения
            do
            {
                result[i, j] = number;
                number++;
                if (i + 1 == n)
                {
                    // обработка в случае достижения последней строки
                    startPos++;
                    i = startPos;
                    j = 0;
                }
                else if (j + 1 == n)
                {
                    // обработка в случае достижения последнего столбца
                    startPos--;
                    j = startPos;
                    i = 0;
                }
                else 
                {
                    j++;
                    i++;
                }
            } while (number != n*n+1);
            // вывод матрицы на экран
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
