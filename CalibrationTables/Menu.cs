using System;

namespace MenuLibrary
{
    public class Menu  //Универсальное меню
    {
        public class DoubleColor
        {
            public ConsoleColor Text { get; private set; }
            public ConsoleColor Background { get; private set; }

            public DoubleColor(ConsoleColor txt = ConsoleColor.Gray, ConsoleColor back = ConsoleColor.Black)
            {
                Text = txt;
                Background = back;
            }
        }

        private const int TitleIndent = 5;

        #region поля
        private string[][] _Items;          //Пункты меню
        private int[] _NumberOfItems;       //Кол-во пунктов меню
        private string[] _MenuTitle;        //Заголовок меню
        private int[] _CurrentItem;         //Текущий (выбранный в данный момент) пункт меню
        private int[] _PrevItem;            //Предыдущий выбранный пункт меню
        private int _NumberOfMenus;         //кол-во доп. меню
        private int _CurrentMenu = 0;       //текущее доп. меню
        private int _PrevMenu = 0;          //предыдущее доп. меню
        private DoubleColor[] _ColorScheme; //цветовые схемы подменю
        private int[] _MenuWidth;           //ширина доп. меню
        private string _MenuInputText;      //текст перед полем ввода основного меню
        private int _InputMaxLength;        //максимальная длина поля ввода (в символах)
        private string _Input;              //поле ввода
        private int[] _FirstItemY;          //номер строки первого пункта меню
        private int _FirstItemVisible;      //номер первого видимого пункта основного меню
        private int _ItemsVisible;          //кол-во отображаемых пунктов основного меню
        private bool _ShowAllItems;         //флаг отображения всех или части пунктов основного меню (true - отобразить все)
        #endregion

        #region свойства
        public string[] MenuTitle
        {
            get { return _MenuTitle; }
            set { _MenuTitle = value; }
        }

        public int[] CurrentItem
        {
            get { return _CurrentItem; }
            set { _CurrentItem = value; }
        }

        public string MenuInputText
        {
            get { return _MenuInputText; }
            set { _MenuInputText = value; }
        }

        public string[][] Items
        {
            get { return _Items; }
            set
            {
                //проверка на адекватность входных данных
                if (_MenuTitle.Length != value.Length)
                {
                    Console.WriteLine("\nОшибка в меню. Несоответствие кол-ва заголовков и доп. меню");
                    Console.ReadKey(true);
                    Environment.Exit(-2);
                }

                //если основное меню до переназначения пунктов было пустым, то будем отображать все пункты меню
                if (_Items[0].Length == 0) _ShowAllItems = true;

                //перевыделение памяти
                Array.Resize(ref _Items, value.Length);
                Array.Resize(ref _NumberOfItems, value.Length);
                Array.Resize(ref _CurrentItem, value.Length);
                Array.Resize(ref _PrevItem, value.Length);
                Array.Resize(ref _MenuWidth, value.Length);
                Array.Resize(ref _FirstItemY, value.Length);

                //заполняем значениями поля
                _Input = "";         //само поле ввода пока пустое

                //определим, показывать ли все пункты основного меню, и сколько их показывать, если нет
                if (_ShowAllItems) _ItemsVisible = value[0].Length; //если показываем все
                else if (_ItemsVisible > value[0].Length)           //если не все, но в переназначенном меню пунктов стало меньше
                {
                    //то покажем их все
                    _ItemsVisible = value[0].Length;
                    _ShowAllItems = true;
                }

                for (int i = 0; i < value.Length; i++)
                {
                    //перевыделим память под пункты меню и сохраним их
                    Array.Resize(ref _Items[i], value[i].Length);
                    value[i].CopyTo(_Items[i], 0);

                    _NumberOfItems[i] = value[i].Length;    //кол-во пунктов меню

                    //определение ширины меню (берем максимальную длину строки)
                    _MenuWidth[i] = TitleIndent + _MenuTitle[i].Length;
                    if (i == 0 && _MenuInputText != null)   //для основного меню учтем поле ввода, если оно есть
                        _MenuWidth[i] =
                            (_MenuWidth[i] < _MenuInputText.Length) ?
                            _MenuInputText.Length :
                            _MenuWidth[i];
                    for (int j = 0; j < _Items[i].Length; j++)
                        _MenuWidth[i] =
                            (_MenuWidth[i] < _Items[i][j].Length) ?
                            _Items[i][j].Length :
                            _MenuWidth[i];

                    //определение размера поля ввода
                    if (i == 0 && _MenuInputText != null)   //если оно есть
                    {
                        _InputMaxLength = _MenuWidth[i] - _MenuInputText.Length - 1;
                        if (_InputMaxLength < 5)    //сделаем так, чтобы поле ввода было не короче 5 символов
                        {
                            _MenuWidth[i] = _MenuInputText.Length + 6;  //если нужно, увеличим ширину меню
                            _InputMaxLength = 5;
                        }
                    }

                    //определим номера строк экрана для первых пунктов каждого меню, отображаемых на экране
                    if (i == 0)     //для основного меню
                        if (_MenuInputText == null) _FirstItemY[i] = 2; //если у основного меню поля ввода нет, то 2 строка
                        else _FirstItemY[i] = 4;                        //а если есть, то 4-я
                    else if (i == 1)   //для первого доп. меню
                        _FirstItemY[i] = _FirstItemY[i - 1] + _ItemsVisible + 4;    //отталкиваемся от данных по основному меню с учетом кол-ва отображаемых пунктов
                    else            //для остальных доп. меню
                        _FirstItemY[i] = _FirstItemY[i - 1] + _Items[i - 1].Length + 4; //отталкиваемся от данных по предыдущим меню
                }

                _FirstItemVisible = 0;  //первый видимый пункт пока первый
            }
        }
        #endregion

        #region конструкторы
        public Menu                             //Конструктор: Создаем меню
            (string[] title,                    //заголовок меню
            string[][] items,                   //элементы меню
            string menuinput=null,              //текст перед полем ввода
            int itemsvisible=0,                 //кол-во отображаемых пунктов основного меню
            DoubleColor[] colorscheme = null)   //цветовая схема меню
        {
            //проверка на адекватность входных данных
            if (title.Length != items.Length)
            {
                Console.WriteLine("\nОшибка в меню. Несоответствие кол-ва заголовков и доп. меню");
                Console.ReadKey(true);
                Environment.Exit(-2);
            }

            //выделение памяти
            _MenuTitle = new string[title.Length];
            _Items = new string[title.Length][];
            _NumberOfItems = new int[title.Length];
            _CurrentItem = new int[title.Length];
            _PrevItem = new int[title.Length];
            _ColorScheme = new DoubleColor[title.Length];
            _MenuWidth = new int[title.Length];
            _FirstItemY = new int[title.Length];

            //сохранение данных
            _NumberOfMenus = title.Length;  //кол-во меню
            title.CopyTo(_MenuTitle, 0);    //сохраним заголовки всех меню
            _MenuInputText = menuinput;     //сохраним подпись у поля ввода
            _Input = "";                    //само поле ввода пока пустое

            //определим, показывать ли все пункты основного меню, и сколько их показывать, если нет
            if (itemsvisible <= 0)
            {
                _ItemsVisible = items[0].Length;
                _ShowAllItems = true;
            }
            else
            {
                _ItemsVisible = itemsvisible;
                _ShowAllItems = false;
            }

            for (int i = 0; i < title.Length; i++)
            {
                //выделим память под пункты меню и сохраним их
                _Items[i] = new string[items[i].Length];
                items[i].CopyTo(_Items[i], 0);

                _NumberOfItems[i] = items[i].Length;    //кол-во пунктов меню

                //сохраним цветовую схему меню
                if (colorscheme != null) _ColorScheme[i] = new DoubleColor(colorscheme[i].Text, colorscheme[i].Background);
                else _ColorScheme[i] = new DoubleColor();

                //определение ширины меню (берем максимальную длину строки)
                _MenuWidth[i] = TitleIndent + _MenuTitle[i].Length;
                if (i == 0 && _MenuInputText != null)   //для основного меню учтем поле ввода, если оно есть
                    _MenuWidth[i] =
                        (_MenuWidth[i] < _MenuInputText.Length) ?
                        _MenuInputText.Length :
                        _MenuWidth[i];
                for (int j = 0; j < _Items[i].Length; j++)
                    _MenuWidth[i] =
                        (_MenuWidth[i] < _Items[i][j].Length) ?
                        _Items[i][j].Length :
                        _MenuWidth[i];

                //определение размера поля ввода
                if (i == 0 && _MenuInputText != null)   //если оно есть
                {
                    _InputMaxLength = _MenuWidth[i] - _MenuInputText.Length - 1;
                    if (_InputMaxLength < 5)    //сделаем так, чтобы поле ввода было не короче 5 символов
                    {
                        _MenuWidth[i] = _MenuInputText.Length + 6;  //если нужно, увеличим ширину меню
                        _InputMaxLength = 5;
                    }
                }

                //определим номера строк экрана для первых пунктов каждого меню, отображаемых на экране
                if (i == 0)     //для основного меню
                    if (_MenuInputText == null) _FirstItemY[i] = 2; //если у основного меню поля ввода нет, то 2 строка
                    else _FirstItemY[i] = 4;                        //а если есть, то 4-я
                else if (i == 1)   //для первого доп. меню
                    _FirstItemY[i] = _FirstItemY[i - 1] + _ItemsVisible + 4;    //отталкиваемся от данных по основному меню с учетом кол-ва отображаемых пунктов
                else            //для остальных доп. меню
                    _FirstItemY[i] = _FirstItemY[i - 1] + _Items[i - 1].Length + 4; //отталкиваемся от данных по предыдущим меню
            }
            
            _FirstItemVisible = 0;  //первый видимый пункт пока первый
        }
        #endregion

        #region методы private
        private void ShowMenu()   //первичный вывод меню на экран
        {
            Console.Clear();    //очистка окна консоли
            Console.CursorVisible = false;  //делаем курсор невидимым

            for (int i = 0; i < _NumberOfMenus; i++)
            {
                Console.ForegroundColor = _ColorScheme[i].Text;    //цвет текста 
                Console.BackgroundColor = _ColorScheme[i].Background;   //фон текста 

                //выводим заголовок
                for (int j = 0; j < TitleIndent; j++)   //отступ
                    Console.Write(" ");
                Console.Write($"{_MenuTitle[i]}");      //заголовок
                for (int j = TitleIndent + _MenuTitle[i].Length; j < _MenuWidth[i]; j++)    //до ширины меню
                    Console.Write(" ");
                Console.WriteLine();

                //пустая строка после заголовка
                for (int j = 0; j < _MenuWidth[i]; j++)
                    Console.Write(" ");
                Console.WriteLine();

                //поле ввода в основном меню
                if (i==0 && _MenuInputText != null) //если оно есть
                {
                    Console.Write($"{_MenuInputText} ");    //текст перед полем ввода
                    for (int j = _MenuInputText.Length + 1; j < _MenuWidth[i]; j++) //до ширины меню
                        Console.Write(" ");
                    Console.WriteLine();

                    //пустая строка после поля ввода
                    for (int j = 0; j < _MenuWidth[i]; j++)
                        Console.Write(" ");
                    Console.WriteLine();
                }

                //пункты меню
                int first, last;    //первый и последний отображаемый пункт меню
                //определим первый и последний отображаемый пункт меню
                first = (i == 0) ? _FirstItemVisible : 0;
                last = (i == 0) ? _FirstItemVisible + _ItemsVisible : _NumberOfItems[i];
                for (int j = first; j < last; j++)
                {   //пробегаем по всем элементам меню
                    if (i==_CurrentMenu && j == _CurrentItem[i])
                    {   //для выбранного пункта меню
                        Console.ForegroundColor = _ColorScheme[i].Background;   //текст
                        Console.BackgroundColor = _ColorScheme[i].Text;    //фон

                        //выводим элемент меню
                        Console.Write(_Items[i][j]);
                        for (int k = _Items[i][j].Length; k < _MenuWidth[i]; k++)
                            Console.Write(" ");
                        Console.WriteLine();

                        Console.ForegroundColor = _ColorScheme[i].Text;    //текст 
                        Console.BackgroundColor = _ColorScheme[i].Background;   //фон 
                    }
                    else
                    {   //для всех остальных пунктов меню
                        Console.Write(_Items[i][j]);
                        for (int k = _Items[i][j].Length; k < _MenuWidth[i]; k++)
                            Console.Write(" ");
                        Console.WriteLine();
                    }
                }

                Console.WriteLine();
                Console.WriteLine();
            }

            Console.SetCursorPosition(0, 0);
        }

        private bool KeyPressed(    //обработка нажатия клавиши
                                    //Возвращаем true, если был нажат <Enter>.
                                    //Возвращаем false в остальных ситуациях.                                     
            ConsoleKeyInfo key) //информация о нажатой клавише
        {
            switch (key.Key)
            {   //выбираем, какая клавиша нажата
                case ConsoleKey.UpArrow:    //стрелка вверх
                    _Input = "";    //очистили поле ввода
                    _PrevMenu = _CurrentMenu;   //сохранили предыдущее меню
                    _PrevItem[_CurrentMenu] = _CurrentItem[_CurrentMenu];   //сохраняем предыдущий выбранный пункт
                    _CurrentItem[_CurrentMenu] =
                        (_CurrentItem[_CurrentMenu] - 1) < 0 ?    //если мы достигли верха меню,
                        (_NumberOfItems[_CurrentMenu] - 1) :    //то текущий выбранный пункт теперь - самый нижний
                        (_CurrentItem[_CurrentMenu] - 1);       //в противном случае прыгаем на один пункт вверх
                    break;

                case ConsoleKey.DownArrow:  //стрелка вниз
                    _Input = "";    //очистили поле ввода
                    _PrevMenu = _CurrentMenu;   //сохранили предыдущее меню
                    _PrevItem[_CurrentMenu] = _CurrentItem[_CurrentMenu];                   //сохраняем предыдущий выбранный пункт
                    _CurrentItem[_CurrentMenu] =
                        (_CurrentItem[_CurrentMenu] + 1) == _NumberOfItems[_CurrentMenu] ?  //если достигли низа меню,
                        0 :                                     //то текущий выбранный пункт теперь - самый верхний
                        (_CurrentItem[_CurrentMenu] + 1);                     //в противном случае прыгаем на один пункт вниз
                    break;

                case ConsoleKey.PageUp:
                    _Input = "";    //очистили поле ввода
                    _PrevMenu = _CurrentMenu;   //сохранили предыдущее меню
                    _PrevItem[_CurrentMenu] = _CurrentItem[_CurrentMenu];                   //сохраняем предыдущий выбранный пункт
                    if (_CurrentMenu == 0)  //для основного меню перейдем на кол-во отображаемых пунктов вверх
                    {
                        _CurrentItem[_CurrentMenu] -= _ItemsVisible;
                        _CurrentItem[_CurrentMenu] = (_CurrentItem[_CurrentMenu] < 0) ?
                            0 :
                            _CurrentItem[_CurrentMenu];
                    }
                    else _CurrentItem[_CurrentMenu] = 0;    //для остальных меню перейдем в начало
                    break;

                case ConsoleKey.PageDown:
                    _Input = "";    //очистили поле ввода
                    _PrevMenu = _CurrentMenu;   //сохранили предыдущее меню
                    _PrevItem[_CurrentMenu] = _CurrentItem[_CurrentMenu];                   //сохраняем предыдущий выбранный пункт
                    if (_CurrentMenu == 0)  //для основного меню перейдем на кол-во отображаемых пунктов вверх
                    {
                        _CurrentItem[_CurrentMenu] += _ItemsVisible;
                        _CurrentItem[_CurrentMenu] = (_CurrentItem[_CurrentMenu] >= _NumberOfItems[_CurrentMenu]) ?
                            (_NumberOfItems[_CurrentMenu] - 1) :
                            _CurrentItem[_CurrentMenu];
                    }
                    else _CurrentItem[_CurrentMenu] = _NumberOfItems[_CurrentMenu] - 1;    //для остальных меню перейдем в конец
                    break;

                case ConsoleKey.Home:
                    _Input = "";    //очистили поле ввода
                    _PrevMenu = _CurrentMenu;   //сохранили предыдущее меню
                    _PrevItem[_CurrentMenu] = _CurrentItem[_CurrentMenu];   //сохраняем предыдущий выбранный пункт
                    _CurrentItem[_CurrentMenu] = 0;     //переходим в начало текущего меню
                    break;

                case ConsoleKey.End:
                    _Input = "";    //очистили поле ввода
                    _PrevMenu = _CurrentMenu;   //сохранили предыдущее меню
                    _PrevItem[_CurrentMenu] = _CurrentItem[_CurrentMenu];   //сохраняем предыдущий выбранный пункт
                    _CurrentItem[_CurrentMenu] = _NumberOfItems[_CurrentMenu] - 1;     //переходим в конец текущего меню
                    break;

                case ConsoleKey.Enter:  //<Enter>, т.е. выбор пункта меню завершен
                    Console.Clear();                //очищаем окно консоли
                    Console.CursorVisible = true;   //теперь курсор видимый
                    Console.ResetColor();           //сбрасываем настройки цвета по-умолчанию
                    return true;                    //возвращаем true

                case ConsoleKey.Tab:    //<Tab>, т.е. перемещение между доп. меню
                    _Input = "";    //очистили поле ввода
                    _PrevItem[_CurrentMenu] = _CurrentItem[_CurrentMenu];
                    _PrevMenu = _CurrentMenu;
                    _CurrentMenu =
                        (_CurrentMenu + 1) == _NumberOfMenus ?
                        0 :
                        (_CurrentMenu + 1);
                    break;

                case ConsoleKey.Escape: //<ESC>, отмена меню
                    Console.Clear();                //очищаем окно консоли
                    Console.CursorVisible = true;   //теперь курсор видимый
                    Console.ResetColor();           //сбрасываем настройки цвета по-умолчанию
                    _CurrentMenu = -1;              //флаг того, что меню просто отменили (т.е. ничего выбрано не было)
                    return true;                    //возвращаем true

                default:
                    if (_CurrentMenu == 0 && _MenuInputText != null)    //если используется поле ввода
                    {
                        if (char.IsLetterOrDigit(key.KeyChar) ||
                            char.IsPunctuation(key.KeyChar) ||
                            key.KeyChar == ' ')                     //учитываем ввод буквы, числа, знаков пунктуации и пробела
                        {
                            _Input += key.KeyChar.ToString();   //добавили новый символ в поле ввода
                            _Input = (_Input.Length > _InputMaxLength) ? "" : _Input;   //если его длина превышена, очистим его

                            int foundItem = FindItem();     //ищем соответствующий полю ввода пункт меню
                            if (foundItem == -1)                //если не нашли
                            {
                                _Input = "";   //очистим поле ввода
                                
                                //и попробуем найти только последний введенный символ
                                _Input += key.KeyChar.ToString();
                                foundItem = FindItem();                 //ищем
                                if (foundItem == -1) _Input = "";       //если и его не нашли, просто очистим поле ввода
                                else                                    //а если нашли, перейдем к найденному пункту
                                {
                                    _PrevMenu = _CurrentMenu;
                                    _PrevItem[0] = _CurrentItem[0];
                                    _CurrentItem[0] = foundItem;
                                }
                            }
                            else                                //если нашли, перейдем к найденному пункту
                            {
                                _PrevMenu = _CurrentMenu;
                                _PrevItem[0] = _CurrentItem[0];
                                _CurrentItem[0] = foundItem;
                            }
                        }
                    }
                    break;
            }

            return false;   //если пришли сюда, то выбор еще не завершен. Возвращаем false.
        }

        private int FindItem()  //ищет номер пункта основного меню по полю ввода
            //поиск работает так: проходим последовательно все пункты меню, как только нашли такой,
            //который начинается с такого же текста, что и в поле ввода, возвращаем его номер.
            //если дошли до конца, но так и не нашли, вернем -1 - флаг того, что подходящего пункта нет
        {
            for (int i = 0; i < _Items[0].Length; i++)
                if (_Items[0][i].IndexOf(_Input) == 0)
                    return i;

            return -1;
        }

        private void RedrawMenu()   //перерисовка меню
        {
            //перерисовка поля ввода
            if (_MenuInputText != null)
            {
                Console.ForegroundColor = _ColorScheme[0].Text;
                Console.BackgroundColor = _ColorScheme[0].Background;
                Console.SetCursorPosition(_MenuInputText.Length + 1, 2);
                Console.Write(_Input);
                for (int i = _Input.Length; i < _InputMaxLength; i++)
                    Console.Write(" ");
            }

            //---==перерисовка пунктов меню
            if (_PrevMenu == 0 &&       //если мы были в основном меню И
                _CurrentMenu == 0       //остались в основном меню И
                && GetFirstVisible())   //основному меню нужна полная перерисовка
                                        //(а также определили первый отображаемый пункт)
            {
                //то полностью перерисуем основное меню
                RedrawFullMenu();
            }
            else                        //в остальных случаях
            {
                //перерисуем только два пункта
                //перерисовка предыдущего пункта
                int cursorY = _FirstItemY[_PrevMenu] + _PrevItem[_PrevMenu];
                cursorY = (_PrevMenu == 0) ? (cursorY - _FirstItemVisible) : cursorY;

                Console.ForegroundColor = _ColorScheme[_PrevMenu].Text;
                Console.BackgroundColor = _ColorScheme[_PrevMenu].Background;
                Console.SetCursorPosition(0, cursorY);
                Console.Write(_Items[_PrevMenu][_PrevItem[_PrevMenu]]);
                for (int i = _Items[_PrevMenu][_PrevItem[_PrevMenu]].Length; i < _MenuWidth[_PrevMenu]; i++)
                    Console.Write(" ");

                //перерисовка текущего пункта (инвертируем цветовую схему)
                cursorY = _FirstItemY[_CurrentMenu] + _CurrentItem[_CurrentMenu];
                cursorY = (_CurrentMenu == 0) ? (cursorY - _FirstItemVisible) : cursorY;

                Console.ForegroundColor = _ColorScheme[_CurrentMenu].Background;
                Console.BackgroundColor = _ColorScheme[_CurrentMenu].Text;
                Console.SetCursorPosition(0, cursorY);
                Console.Write(_Items[_CurrentMenu][_CurrentItem[_CurrentMenu]]);
                for (int i = _Items[_CurrentMenu][_CurrentItem[_CurrentMenu]].Length; i < _MenuWidth[_CurrentMenu]; i++)
                    Console.Write(" ");
            }
        }

        private bool GetFirstVisible()  //определение первого отображаемого пункта основного меню при перерисовке
            //вернет true, если меню нужно двигать,
            //false, если меню не двигается
        {
            if (_CurrentItem[0] >= _FirstItemVisible &&
                _CurrentItem[0] <= (_FirstItemVisible + _ItemsVisible - 1))   //если текущий элемент лежит в отображаемых границах, ничего не меняем
            {
                return false;
            }

            if (_PrevItem[0] < _CurrentItem[0] && _PrevItem[0] != 0)    //если сдвинулись вниз
            {
                //то первый  отображаемый ставим так, чтобы текущий был последним отображаемым
                _FirstItemVisible = _CurrentItem[0] - _ItemsVisible + 1;
            }
            else                                    //если сдвинулись вверх
            {
                if (_NumberOfItems[0] < (_CurrentItem[0] + _ItemsVisible))  //если, когда поставим текущий первым отображаемым, меню "уползет" вверх
                    _FirstItemVisible = _NumberOfItems[0] - _ItemsVisible;  //то первый отображаемый ставим так, чтобы последний пункт меню был в самом низу
                else                                                        //если меню не "уползет"
                    _FirstItemVisible = _CurrentItem[0];                    //то первый отображаемый - текущий
            }

            return true;
        }

        private void RedrawFullMenu()       //полная перерисовка пунктов основного меню
        {
            for (int y = _FirstItemY[0], v = _FirstItemVisible;     //идем одновременно по строкам в окне и отображаемым пунктам
                v < _FirstItemVisible + _ItemsVisible;              //закончим, когда отобразим все пункты
                y++, v++)
            {
                if (_CurrentMenu == 0 && v == _CurrentItem[0])  //рисуем текущий пункт меню
                {
                    Console.ForegroundColor = _ColorScheme[0].Background;
                    Console.BackgroundColor = _ColorScheme[0].Text;
                    Console.SetCursorPosition(0, y);
                    Console.Write(_Items[0][v]);
                    for (int i = _Items[0][v].Length; i < _MenuWidth[0]; i++)
                        Console.Write(" ");
                }
                else        //рисуем остальные пункты меню
                {
                    Console.ForegroundColor = _ColorScheme[0].Text;
                    Console.BackgroundColor = _ColorScheme[0].Background;
                    Console.SetCursorPosition(0, y);
                    Console.Write(_Items[0][v]);
                    for (int i = _Items[0][v].Length; i < _MenuWidth[0]; i++)
                        Console.Write(" ");
                }
            }
        }
        #endregion

        #region методы public
        public void MenuCicle(out int MenuChoice, out int ItemChoice, int currentMenu= 0, int currentItem = 0)      //цикл по меню
                                                                            //Возвращаем номер выбранного пункта меню
        {
            _CurrentMenu = currentMenu;
            _CurrentItem[_CurrentMenu] = currentItem;   //начинаем с заданного пункта
            ShowMenu();         //первоначальное отображение меню

            while (!KeyPressed(Console.ReadKey(true)))
            {//пока не будет нажат <Enter>
                RedrawMenu();   //перерисовываем меню
                Console.ResetColor();
                
            }

            //сюда попадем, когда был нажат <Enter>
            MenuChoice = _CurrentMenu;
            if (MenuChoice != -1) ItemChoice = _CurrentItem[_CurrentMenu];
            else ItemChoice = -1;
        }
        #endregion
    }
}
