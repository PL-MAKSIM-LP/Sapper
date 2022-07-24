using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sapper.Controllers
{
    public static class MapController
    {
        public const int mapSize = 8;
        public const int cellSize = 40;
        public static int[,] map = new int[mapSize, mapSize];
        public static bool[,] boolMap = new bool[mapSize, mapSize];
        public static Button[,] buttons = new Button[mapSize, mapSize];
        public static Image spriteSet;
        private static bool isFirstTurn;
        private static Point firstCoordinations;
        private static Form form;
        private static int bombCount;
        private static int clickCount = 0;
        private static Image tmpImage = null;

        public static void Init(Form currForm)
        {     
            form = currForm;
            isFirstTurn = true;
            spriteSet = new Bitmap(Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName.ToString(),"Resource/2.jpg"));
            ConfigMapSize(currForm);
            InitMap();
            InitButtons(currForm);          
        }

        private static void ConfigMapSize(Form currForm)
        {
            currForm.StartPosition = FormStartPosition.CenterScreen;
            currForm.Width = mapSize * cellSize + 20;
            currForm.Height = (mapSize + 1) * cellSize;
            currForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
        }

        private static void InitMap()
        {
            for (int i = 0; i < mapSize; i++)
                for (int j = 0; j < mapSize; j++)
                {
                    map[i, j] = 0;
                }
        }

        private static void InitButtons(Form currForm)
        {
            for (int i = 0; i < mapSize; i++)
                for (int j = 0; j < mapSize; j++)
                {
                    Button button = new Button();                   
                    button.Location = new Point(j * cellSize, i * cellSize);
                    button.Size = new Size(cellSize, cellSize);
                    button.Image = GetImage(10);
                    button.FlatAppearance.BorderSize = 0;
                    button.FlatStyle = FlatStyle.Flat;
                    button.MouseUp += new MouseEventHandler(OnButtonPressedMouse);
                    button.MouseHover += new EventHandler(Hover);
                    currForm.Controls.Add(button);
                    buttons[i, j] = button;
                    boolMap[i, j] = false;
                }

        }
        private static void Hover(object sender, System.EventArgs e)
        {
            clickCount = -1;
            Button tmp = sender as Button;

            tmpImage = tmp.Image; 
        }
        private static void OnButtonPressedMouse(object sender, MouseEventArgs e)
        {
            Button pressButton = (Button)sender;
            switch (e.Button.ToString())
            { 
                case "Left": OnLeftButtonPressed(pressButton);  break;
                case "Right": OnRightButtonPressed(pressButton); break;            
            }        
        }
        private static void OnRightButtonPressed(Button pressButton)
        {
            clickCount++;
            clickCount %= 2;
            switch (clickCount)
            {
                case 0: pressButton.Image = GetImage(11); break;
                case 1: pressButton.Image = GetImage(10); break;
            }
        }
        private static void ShowAllBombs(int iBomb, int jBomb)
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (i == iBomb && j == jBomb)
                        continue;
                    if (map[i, j] == -1)
                    {
                        buttons[i, j].Image = GetImage(9);
                    }
                }
            }
        }

        private static void OnLeftButtonPressed(Button pressButton)
        {
            int buttonLocationX = pressButton.Location.X / cellSize;
            int buttonLocationY = pressButton.Location.Y / cellSize;
            if (isFirstTurn)
            {
                firstCoordinations = new Point(buttonLocationX, buttonLocationY); 
                SeedMap();
                CountCellBomb();
                isFirstTurn = false;
            }
            OpenCells(buttonLocationY, buttonLocationX);
            if (map[buttonLocationY, buttonLocationX] == -1)
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"D:\programm\sapper\sapper\Resource\odinochnyiy-vzryiv.wav");
                player.Play();
                pressButton.Image = GetImage(9);
                ShowAllBombs(buttonLocationY, buttonLocationX);
                MessageBox.Show("Boom");
                form.Controls.Clear();
                Init(form);
            }
            else if (CheckWin())
            {
                ShowAllBombs(buttonLocationY, buttonLocationX);
                MessageBox.Show("You Win");
                form.Controls.Clear();
                Init(form);
            }

            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                   buttons[i, j].Enabled = true;
                }
            }
        }

        public static Image GetImage(int pos)
        {
            Image image = new Bitmap(cellSize, cellSize);
            Graphics graphics = Graphics.FromImage(image);
            graphics.DrawImage(spriteSet, new Rectangle(new Point(0, 0), new Size(cellSize, cellSize)), 0 + 32 * pos, 0, 33, 33, GraphicsUnit.Pixel);
           
            return image;
        }

        private static void SeedMap()
        { 
            Random rand = new Random();
            bombCount = rand.Next(5, 10);

            for (int i = 0; i < bombCount; i++)
            {
                int posX = rand.Next(0, mapSize);
                int posY = rand.Next(0, mapSize);

                while (map[posX, posY] == -1 || (Math.Abs(posX - firstCoordinations.X) <= 1 && Math.Abs(posY - firstCoordinations.Y) <= 1))
                {
                     posX = rand.Next(0, mapSize);
                     posY = rand.Next(0, mapSize);
                }

                map[posX, posY] = -1;
            }
        }

        
        private static void CountCellBomb()
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (map[i, j] == -1)
                    {
                        for (int k = i - 1; k < i + 2; k++)
                        {
                            for (int l = j - 1; l < j + 2; l++)
                            {
                                if (!IsInBorder(k, l) || map[k, l] == -1)
                                    continue;
                                map[k, l] = map[k, l] + 1;
                            }
                        }
                    }
                }
            }
        }

        private static bool CheckWin()
        {
            int allCellCount = 0;
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (!boolMap[i, j])
                        allCellCount++;
                }
            }

            if (allCellCount == bombCount)
                return true;
            return false;
        }

        private static bool IsInBorder(int i, int j)
        {
            if (i < 0 || j < 0 || j > mapSize - 1 || i > mapSize - 1)
            {
                return false;
            }
            return true;
        }
        private static void OpenCells(int i, int j)
        {

            OpenCell(i, j);

            if (map[i, j] > 0)
             
                return;

            for (int k = i - 1; k < i + 2; k++)
            {
                for (int l = j - 1; l < j + 2; l++)
                {                   
                    if (!IsInBorder(k, l))
                        continue;
                    if (!buttons[k, l].Enabled)
                        continue;
                    if (map[k, l] == 0)
                        OpenCells(k, l);
                    else if (map[k, l] > 0)
                        OpenCell(k, l);
                }
            }
        }


        private static void OpenCell(int i, int j)
        {
            if (map[i, j] == -1)
                buttons[i, j].Image = GetImage(0);
            else
            {                
                buttons[i, j].Image = GetImage(map[i, j]);
            }
             buttons[i, j].MouseUp -= new MouseEventHandler(OnButtonPressedMouse);
             buttons[i, j].MouseHover -= new EventHandler(Hover);
             buttons[i, j].Enabled = false;

            boolMap[i, j] = true;            
        }
    }   
}
