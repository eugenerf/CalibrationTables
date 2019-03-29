using System
using System.IO;
using System.Text;
using System.Threading;
using MenuLibrary;
using FileLibrary;
using DataProcesssingLibrary;

namespace CalibrationTables
{
    class Program
    {
        static bool WriteValcomHeader = false;
        static LevelBase DefaultLevelBase = LevelBase.Millimeters;
        static bool ExitApp = false;
        static bool UseLog = false;
        static bool RepairVolumeInconcictensy = false;
        static StreamWriter LogFile;

        [STAThread]
        static void Main(string[] args)
        {
            MainMenu();
            if (UseLog && LogFile != null) LogFile.Close();
        }

        #region меню
        static void MainMenu()  //главное меню
        {
            int MenuChoice = 0,     //выбранное доп. меню
                ItemChoice = 0;     //выбранный пункт  меню
            Menu.DoubleColor[] colorScheme = new Menu.DoubleColor[2];
            colorScheme[0] = new Menu.DoubleColor();
            colorScheme[1] = new Menu.DoubleColor(ConsoleColor.Yellow);

            Menu M = new Menu(
                new string[] { "Градуировочные таблицы", "Опции (<Tab> для перехода)" },
                new string[][] { new string[] { }, new string[] { } },
                null,
                0,
                colorScheme);

            do
            {
                string tmp = "";
                switch (DefaultLevelBase)
                {
                    case LevelBase.Millimeters: tmp = "мм"; break;
                    case LevelBase.Centimeters: tmp = "см"; break;
                }
                M.Items = new string[][] {
                    new string[] {
                    "1. Одна таблица",
                    "2. Много таблиц",
                    "3. Справка",
                    "4. Выход"
                    },
                    new string[] {
                    "База уровней в выходных файлах: " + tmp,
                    "Исправлять непоследовательность объемов: " + ((RepairVolumeInconcictensy)?"да":"нет"),
                    "Записывать заголовок Valcom: " + ((WriteValcomHeader)?"да":"нет"),
                    "Писать log-файл: " + ((UseLog)?"да":"нет")
                    }
                };
                M.MenuCicle(out MenuChoice, out ItemChoice);
                switch (MenuChoice)
                {
                    case -1:    //отмена меню
                        ExitApp = true;
                        break;

                    case 0:
                        switch (ItemChoice)
                        {
                            case 0:     //1. Одна таблица
                                OneTable();
                                break;
                            case 1:     //2. Много таблиц
                                ManyTables();
                                break;
                            case 2:     //3. Справка
                                Console.Clear();
                                Console.WriteLine(Properties.Resources.Справка);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("\n\nДля продолжения нажмите любую клавишу...");
                                Console.SetCursorPosition(0, 0);
                                Console.ReadKey(true);
                                break;
                            case 3:     //4. Выход
                                ExitApp = true;
                                break;
                            default:    //ошибка меню
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка в работе доп. 0 главного меню!!!\n\nНажмите любую клавишу для выхода...");
                                Console.ReadKey(true);
                                Environment.Exit(-1);
                                break;
                        }
                        break;

                    case 1:
                        switch (ItemChoice)
                        {
                            case 0:     //База уровней в выходных файлах:
                                Menu M2 = new Menu(
                                    new string[] { "Задайте базу уровней в выходных файлах" },
                                    new string[][] { new string[] { "сантиметры", "миллиметры" } },
                                    null,
                                    0,
                                    new Menu.DoubleColor[] { new Menu.DoubleColor(ConsoleColor.Yellow) });
                                int MenuChoice2 = 0,
                                    ItemChoice2 = 0;
                                M2.MenuCicle(out MenuChoice2, out ItemChoice2);
                                switch (MenuChoice2)
                                {
                                    case 0:
                                        switch (ItemChoice2)
                                        {
                                            case 0: //сантиметры
                                                DefaultLevelBase = LevelBase.Centimeters;
                                                break;
                                            case 1: //миллиметры
                                                DefaultLevelBase = LevelBase.Millimeters;
                                                break;
                                        }
                                        break;
                                }
                                break;

                            case 1:     //Исправлять непоследовательность объемов
                                Menu M4 = new Menu(
                                    new string[] { "Исправлять непоследовательность объемов?" },
                                    new string[][] { new string[] { "Да", "Нет" } },
                                    null,
                                    0,
                                    new Menu.DoubleColor[] { new Menu.DoubleColor(ConsoleColor.Yellow) });
                                int MenuChoice4 = 0,
                                    ItemChoice4 = 0;
                                M4.MenuCicle(out MenuChoice4, out ItemChoice4);
                                switch (MenuChoice4)
                                {
                                    case 0:
                                        switch (ItemChoice4)
                                        {
                                            case 0: //Да
                                                RepairVolumeInconcictensy = true;
                                                break;
                                            case 1: //Нет
                                                RepairVolumeInconcictensy = false;
                                                break;
                                        }
                                        break;
                                }
                                break;

                            case 2:     //3. Записывать заголовок Valcom:
                                Menu M1 = new Menu(
                                    new string[] { "Записывать в выходные файлы заголовок Valcom?" },
                                    new string[][] { new string[] { "Да", "Нет" } },
                                    null,
                                    0,
                                    new Menu.DoubleColor[] { new Menu.DoubleColor(ConsoleColor.Yellow) });
                                int MenuChoice1 = 0,
                                    ItemChoice1 = 0;
                                M1.MenuCicle(out MenuChoice1, out ItemChoice1);
                                switch (MenuChoice1)
                                {
                                    case 0:
                                        switch (ItemChoice1)
                                        {
                                            case 0: //Да
                                                WriteValcomHeader = true;
                                                break;
                                            case 1: //Нет
                                                WriteValcomHeader = false;
                                                break;
                                        }
                                        break;
                                }
                                break;

                            case 3:     //Писать log-файл
                                Menu M3 = new Menu(
                                    new string[] { "Писать log-файл работы программы?" },
                                    new string[][] { new string[] { "Да", "Нет" } },
                                    null,
                                    0,
                                    new Menu.DoubleColor[] { new Menu.DoubleColor(ConsoleColor.Yellow) });
                                int MenuChoice3 = 0,
                                    ItemChoice3 = 0;
                                M3.MenuCicle(out MenuChoice3, out ItemChoice3);
                                switch (MenuChoice3)
                                {
                                    case 0:
                                        switch (ItemChoice3)
                                        {
                                            case 0: //Да
                                                UseLog = true;

                                                //открываем LOG
                                                string logPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                                                logPath = System.IO.Path.GetFileNameWithoutExtension(logPath);
                                                logPath += ".log";
                                                LogFile = new StreamWriter(logPath, true, Encoding.Default);
                                                break;
                                            case 1: //Нет
                                                UseLog = false;
                                                break;
                                        }
                                        break;
                                }
                                break;

                            default:    //ошибка меню
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка в работе доп. 1 главного меню!!!\n\nНажмите любую клавишу для выхода...");
                                Console.ReadKey(true);
                                ExitApp = true;
                                break;
                        }
                        break;

                    default:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ошибка в работе главного меню!!!\n\nНажмите любую клавишу для выхода...");
                        Console.ReadKey(true);
                        ExitApp = true;
                        break;

                }
            } while (!(MenuChoice == -1 || MenuChoice == 0 && ItemChoice == 3) && !ExitApp);
        }

        static void OneTableMenu(CalibrationTable CT, string path = null, bool ExitIfTrue = true)  //меню работы с одной таблицей
        {
            int MenuChoice = 0,     //выбранное доп. меню
                ItemChoice = 0;     //выбранный пункт  меню

            Menu.DoubleColor[] colorScheme = new Menu.DoubleColor[2];
            colorScheme[0] = new Menu.DoubleColor();
            colorScheme[1] = new Menu.DoubleColor(ConsoleColor.Yellow);

            Menu M = new Menu(
                new string[] { "Таблица: " + CT.CTid, "Опции (<Tab> для перехода)" },
                new string[][] { new string[] { }, new string[] { } },
                null,
                0,
                colorScheme);

            do
            {
                M.Items = new string[][] {
                    new string[]
                    {
                        "1. Отобразить таблицу",
                        "2. Изменить базу уровней на " + ((CT.levelBase==LevelBase.Centimeters)?"мм":"см"),
                        "3. Правка значений в таблице",
                        "4. Проверить таблицу",
                        "5. Исправить таблицу",
                        "6. Сортировать",
                        "7. Сохранить",
                        "8. "+ ((ExitIfTrue)?"Выход":"Назад")
                    },
                    new string[]
                    {
                        "Исправлять непоследовательность объемов: " + ((RepairVolumeInconcictensy)?"да":"нет"),
                        "Записывать заголовок Valcom: " + ((WriteValcomHeader)?"да":"нет")
                    }
                };

                M.MenuCicle(out MenuChoice, out ItemChoice);
                switch (MenuChoice)
                {
                    case -1:    //отмена меню
                        break;

                    case 0:
                        switch (ItemChoice)
                        {
                            case 0:     //1. Отобразить таблицу
                                CT.ShowTable();
                                break;
                            case 1:     //2. Изменить базу уровней
                                switch (CT.levelBase)
                                {
                                    case LevelBase.Millimeters:
                                        CT.ChangeLevelBase(LevelBase.Centimeters);
                                        break;
                                    case LevelBase.Centimeters:
                                        CT.ChangeLevelBase(LevelBase.Millimeters);
                                        break;
                                }
                                break;
                            case 2:     //3. Правка значений в таблице
                                ChangeTableMenu(CT);
                                break;
                            case 3:     //3. Проверить таблицу
                                CT.CheckTable();
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("\n\nДля продолжения нажмите любую клавишу...");
                                Console.ReadKey(true);
                                break;
                            case 4:     //4. Исправить таблицу
                                CT.RepairTable(RepairVolumeInconcictensy);
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("\n\nДля продолжения нажмите любую клавишу...");
                                Console.ReadKey(true);
                                break;
                            case 5:     //5. Сортировать
                                CT.SortTable();
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("\n\nДля продолжения нажмите любую клавишу...");
                                Console.ReadKey(true);
                                break;
                            case 6:     //6. Сохранить
                                TXTFile tf = new TXTFile(
                                    FileEventProcessor,
                                    FileEventProcessor,
                                    FileEventProcessor,
                                    FileEventProcessor);
                                tf.WriteFile(CT, path, WriteValcomHeader);
                                break;
                            case 7:     //7. Выход
                                ExitApp = ExitIfTrue;
                                break;
                            default:    //ошибка меню
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка в работе доп. 0 меню одной таблицы!!!\n\nНажмите любую клавишу для выхода...");
                                Console.ReadKey(true);
                                ExitApp = true;
                                break;
                        }
                        break;

                    case 1:
                        switch (ItemChoice)
                        {
                            case 0:
                                Menu M4 = new Menu(
                                    new string[] { "Исправлять непоследовательность объемов?" },
                                    new string[][] { new string[] { "Да", "Нет" } },
                                    null,
                                    0,
                                    new Menu.DoubleColor[] { new Menu.DoubleColor(ConsoleColor.Yellow) });
                                int MenuChoice4 = 0,
                                    ItemChoice4 = 0;
                                M4.MenuCicle(out MenuChoice4, out ItemChoice4);
                                switch (MenuChoice4)
                                {
                                    case 0:
                                        switch (ItemChoice4)
                                        {
                                            case 0: //Да
                                                RepairVolumeInconcictensy = true;
                                                break;
                                            case 1: //Нет
                                                RepairVolumeInconcictensy = false;
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case 1:
                                Menu M2 = new Menu(
                                            new string[] { "Записывать в выходные файлы заголовок Valcom?" },
                                            new string[][] { new string[] { "Да", "Нет" } },
                                            null,
                                            0,
                                            new Menu.DoubleColor[] { new Menu.DoubleColor(ConsoleColor.Yellow) });
                                int MenuChoice2 = 0,
                                    ItemChoice2 = 0;
                                int curItem2 = (WriteValcomHeader) ? 0 : 1;
                                M2.MenuCicle(out MenuChoice2, out ItemChoice2, 0, curItem2);
                                switch (MenuChoice2)
                                {
                                    case 0:
                                        switch (ItemChoice2)
                                        {
                                            case 0: //Да
                                                WriteValcomHeader = true;
                                                break;
                                            case 1: //Нет
                                                WriteValcomHeader = false;
                                                break;
                                        }
                                        break;

                                    default:
                                        Console.Clear();
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Ошибка в работе меню опций!!!\n\nНажмите любую клавишу для выхода...");
                                        Console.ReadKey(true);
                                        ExitApp = true;
                                        break;
                                }
                                break;
                            default:
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка в работе доп. 1 меню одной таблицы!!!\n\nНажмите любую клавишу для выхода...");
                                Console.ReadKey(true);
                                ExitApp = true;
                                break;
                        }
                        break;

                    default:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ошибка в работе меню одной таблицы!!!\n\nНажмите любую клавишу для выхода...");
                        Console.ReadKey(true);
                        ExitApp = true;
                        break;

                }
            } while (!(MenuChoice == -1 || MenuChoice == 0 && ItemChoice == 7) && !ExitApp);
        }

        static void ManyTablesMenu(CalibrationTable[] CT, string path)  //меню работы с несколькими таблицами
        {
            int MenuChoice = 0,     //выбранное доп. меню
                ItemChoice = 0;     //выбранный пункт  меню
            Menu.DoubleColor[] colorScheme = new Menu.DoubleColor[2];
            colorScheme[0] = new Menu.DoubleColor();
            colorScheme[1] = new Menu.DoubleColor(ConsoleColor.Yellow);

            Menu M = new Menu(
                new string[] { "Несколько градуировочных таблиц", "Опции (<Tab> для перехода)" },
                new string[][] { new string[] { }, new string[] { } },
                null,
                0,
                colorScheme);

            do
            {
                M.Items = new string[][]
                {
                    new string[]
                    {
                        "1. Выбрать таблицу для работы",
                        "2. Изменить везде базу уровней на " + ((DefaultLevelBase==LevelBase.Centimeters)?"мм":"см"),
                        "3. Проверить все таблицы",
                        "4. Исправить все таблицы",
                        "5. Сортировать все таблицы",
                        "6. Сохранить все таблицы",
                        "7. Выход"
                    },
                    new string[]
                    {
                        "Исправлять непоследовательность объемов: " + ((RepairVolumeInconcictensy)?"да":"нет")
                    }
                };
                M.MenuCicle(out MenuChoice, out ItemChoice);
                switch (MenuChoice)
                {
                    case -1:    //отмена меню
                        break;

                    case 0:
                        switch (ItemChoice)
                        {
                            case 0:     //1. Выбрать таблицу для работы
                                ChooseTableMenu(CT, path);
                                break;
                            case 1:     //2. Изменить во всех таблицах базу уровней
                                DefaultLevelBase = (DefaultLevelBase == LevelBase.Centimeters) ? (LevelBase.Millimeters) : (LevelBase.Centimeters);
                                for (int i = 0; i < CT.Length; i++) CT[i].ChangeLevelBase(DefaultLevelBase);
                                break;
                            case 2:     //3. Проверить все таблицы
                                for (int i = 0; i < CT.Length; i++) CT[i].CheckTable();
                                break;
                            case 3:     //4. Исправить все таблицы
                                for (int i = 0; i < CT.Length; i++) CT[i].RepairTable(RepairVolumeInconcictensy);
                                break;
                            case 4:     //5. Сортировать все таблицы
                                for (int i = 0; i < CT.Length; i++) CT[i].SortTable();
                                break;
                            case 5:     //6. Сохранить все таблицы
                                TXTFile tf = new TXTFile(
                                    FileEventProcessor,
                                    FileEventProcessor,
                                    FileEventProcessor,
                                    FileEventProcessor);
                                tf.WriteManyFiles(CT, path, WriteValcomHeader);
                                break;
                            case 6:     //2. Выход
                                ExitApp = true;
                                break;
                            default:    //ошибка меню
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка в работе доп. 0 меню нескольких таблиц!!!\n\nНажмите любую клавишу для выхода...");
                                Console.ReadKey(true);
                                ExitApp = true;
                                break;
                        }
                        break;

                    case 1:
                        Menu M4 = new Menu(
                                    new string[] { "Исправлять непоследовательность объемов?" },
                                    new string[][] { new string[] { "Да", "Нет" } },
                                    null,
                                    0,
                                    new Menu.DoubleColor[] { new Menu.DoubleColor(ConsoleColor.Yellow) });
                        int MenuChoice4 = 0,
                            ItemChoice4 = 0;
                        M4.MenuCicle(out MenuChoice4, out ItemChoice4);
                        switch (MenuChoice4)
                        {
                            case 0:
                                switch (ItemChoice4)
                                {
                                    case 0: //Да
                                        RepairVolumeInconcictensy = true;
                                        break;
                                    case 1: //Нет
                                        RepairVolumeInconcictensy = false;
                                        break;
                                }
                                break;
                        }
                        break;

                    default:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ошибка в работе меню нескольких таблиц!!!\n\nНажмите любую клавишу для выхода...");
                        Console.ReadKey(true);
                        ExitApp = true;
                        break;

                }
            } while (!(MenuChoice == -1 || MenuChoice == 0 && ItemChoice == 6) && !ExitApp);
        }

        static void ChooseTableMenu(CalibrationTable[] CT, string path) //меню выбора таблицы для работы
        {
            int ItemChoice = 0,
                MenuChoice = 0;
            string[][] items = new string[][] { new string[CT.Length + 1] };

            items[0][0] = "1. Назад";
            for (int i = 0; i < CT.Length; i++)
                items[0][i + 1] = (i + 2) + ". " + CT[i].CTid;

            Menu M = new Menu(
                new string[] { "Выберите таблицу для работы" },
                items);
            do
            {
                M.MenuCicle(out MenuChoice, out ItemChoice);
                switch (MenuChoice)
                {
                    case -1:    //отмена меню
                        break;

                    case 0:
                        switch (ItemChoice)
                        {
                            case 0:
                                break;
                            default:
                                if (path == null) OneTableMenu(CT[ItemChoice - 1], null, false);
                                else OneTableMenu(CT[ItemChoice - 1], path, false);
                                break;
                        }
                        break;

                    default:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ошибка в работе меню выбора таблицы!!!\n\nНажмите любую клавишу для выхода...");
                        Console.ReadKey(true);
                        ExitApp = true;
                        break;
                }
            } while (!(MenuChoice == -1 || MenuChoice == 0 && ItemChoice == 0) && !ExitApp);
        }

        static void ChangeTableMenu(CalibrationTable CT)
        {
            int MenuChoice = 0,     //выбранное доп. меню
                ItemChoice = 0;     //выбранный пункт  меню

            Menu M = new Menu(
                new string[] { "Правка таблицы " + CT.CTid },
                new string[][] {
                    new string[] {
                        "1. Добавить запись",
                        "2. Изменить запись",
                        "3. Удалить запись",
                        "4. Назад"
                    }
                });

            do
            {
                M.MenuCicle(out MenuChoice, out ItemChoice);
                switch (MenuChoice)
                {
                    case -1:    //отмена меню
                        break;

                    case 0:
                        switch (ItemChoice)
                        {
                            case 0:     //1. Добавить запись
                                CT.AddPair();
                                break;
                            case 1:     //2. Изменить запись
                                CT.ChangeVolume();
                                break;
                            case 2:     //3. Удалить запись
                                CT.DeletePair();
                                break;
                            case 3:     //4. Назад
                                break;
                            default:    //ошибка меню
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка в работе доп. 0 меню правки таблицы!!!\n\nНажмите любую клавишу для выхода...");
                                Console.ReadKey(true);
                                ExitApp = true;
                                break;
                        }
                        break;

                    default:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ошибка в работе меню правки таблицы!!!\n\nНажмите любую клавишу для выхода...");
                        Console.ReadKey(true);
                        ExitApp = true;
                        break;

                }
            } while (!(MenuChoice == -1 || MenuChoice == 0 && ItemChoice == 3) && !ExitApp);
        }
        #endregion

        #region прочие методы
        static void FileEventProcessor(object o, FileEventArgs e)   //общий обработчик событий по файлам
        {
            Console.WriteLine(e.Message);
            if (UseLog && LogFile != null)
            {
                LogFile.Write(System.DateTime.Now.ToString() + ": ");
                LogFile.WriteLine(e.Message);
            }
            Thread.Sleep(e.Duration);
        }

        static void CTEventProcessor(object o, CTEventArgs e)   //общий обработчик событий по таблицам
        {
            Console.WriteLine(e.Message);
            if (UseLog && LogFile != null)
            {
                LogFile.Write(System.DateTime.Now.ToString() + ": ");
                LogFile.WriteLine(e.Message);
            }
            Thread.Sleep(e.Duration);
        }

        static void OneTable()  //обработка одной таблицы
        {
            Console.Clear();

            //открыли, прочитали DOC файл
            DOCFile doc = new DOCFile(
                FileEventProcessor,
                FileEventProcessor,
                FileEventProcessor,
                FileEventProcessor);

            doc.ReadOneFile();

            if (doc.filePath == null) return;

            CalibrationTable CT = new CalibrationTable( //создаем градуировочную таблицу
                doc.fileId[0],
                CTEventProcessor,
                CTEventProcessor,
                CTEventProcessor,
                CTEventProcessor,
                CTEventProcessor,
                CTEventProcessor,
                CTEventProcessor,
                CTEventProcessor);

            if (CT.ReadTable(doc.fileData[0]))
                CT.RepairTable(RepairVolumeInconcictensy);

            //зададим базу уровней
            CT.ChangeLevelBase(DefaultLevelBase);

            //создали, записали TXT файл
            TXTFile txt = new TXTFile(
                FileEventProcessor,
                FileEventProcessor,
                FileEventProcessor,
                FileEventProcessor);

            txt.WriteFile(CT, doc.filePath[0], WriteValcomHeader);

            OneTableMenu(CT); //вызываем меню для таблицы
        }

        static void ManyTables()  //обработка одной таблицы
        {
            string workPath = "";   //рабочая папка (та, где лежат DOC файлы)

            Console.Clear();

            //открыли, прочитали DOC файлы
            DOCFile doc = new DOCFile(
                FileEventProcessor,
                FileEventProcessor,
                FileEventProcessor,
                FileEventProcessor);

            doc.ReadManyFiles(out workPath);

            if (doc.filePath == null) return;

            CalibrationTable[] CT = new CalibrationTable[0];

            for (int i = 0; i < doc.fileId.Length; i++)
            {
                CalibrationTable tempCT = new CalibrationTable(
                    doc.fileId[i],
                    CTEventProcessor,
                    CTEventProcessor,
                    CTEventProcessor,
                    CTEventProcessor,
                    CTEventProcessor,
                    CTEventProcessor,
                    CTEventProcessor,
                    CTEventProcessor);

                if (tempCT.ReadTable(doc.fileData[i]))
                    tempCT.RepairTable(RepairVolumeInconcictensy);

                if (tempCT.Table.Count == 0) continue;

                Array.Resize(ref CT, CT.Length + 1);
                tempCT.CopyTo(ref CT[CT.Length - 1]);

                CT[CT.Length - 1].ChangeLevelBase(DefaultLevelBase);
            }

            TXTFile txt = new TXTFile(  //создали и записали TXT файлы
                FileEventProcessor,
                FileEventProcessor,
                FileEventProcessor,
                FileEventProcessor);

            txt.WriteManyFiles(CT, workPath, WriteValcomHeader);

            ManyTablesMenu(CT, workPath); //вызываем меню для таблиц
        }
        #endregion
    }
}