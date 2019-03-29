using System;
using System.Collections.Generic;
using System.Linq;
using MenuLibrary;

namespace DataProcesssingLibrary
{
    public enum LevelBase   //варианты представления уровня
    {
        Millimeters,    //миллиметры
        Centimeters     //сантиметры
    }

    public enum TableErrors //виды ошибок в таблице
    {
        TableOK,            //ошибок в таблице нет
        UnSorted,           //таблица не отсортирована по уровню
        LevelInconsistency, //непоследовательность уровней (есть соседние уровни, расстояние между которыми не соответствует LevelBase)
        VolumeInconsistency //непоследовательность объемов (в сортированной таблице есть есть объем, который больше предыдущего)
    }

    public class CTEventArgs : EventArgs    //аргументы событий по град. таблицам
    {
        //поля
        public string Message { get; private set; } //сообщение        
        public bool Success { get; private set; }   //признак успеха операции
        public int Duration { get; private set; }       //продолжительность отображения сообщения в милисекундах

        //конструкторы
        public CTEventArgs(bool _suc, string _mes, int _dur = 500)
        {
            Message = _mes;
            Success = _suc;
            Duration = _dur;
        }
    }

    public struct TextPattern  //текстовые паттерны для обработки таблицы, считанной из DOC
    {
        internal const string FileIsCorrect = "Г Р А Д У И Р О В О Ч Н А Я   Т А Б Л И Ц А";
        internal const string TableStart1 = "Э С К И З";
        internal const string TableStart2 = "ЭСКИЗ";
        internal struct Spacers     //разделители пар в таблицах
        {
            internal const char VertLine = '|';
            internal const char Colons = ':';
        }
    }

    public class CalibrationTable   //градуировочная таблица
    {
        //делегат для создания событий
        public delegate void CTStateHandler(object sender, CTEventArgs e);

        //события
        protected internal event CTStateHandler LevelBaseChanged;
        protected internal event CTStateHandler TableSorted;
        protected internal event CTStateHandler TableChecked;
        protected internal event CTStateHandler TableRepaired;
        protected internal event CTStateHandler TableRead;
        protected internal event CTStateHandler ItemAdded;
        protected internal event CTStateHandler ItemChanged;
        protected internal event CTStateHandler ItemDeleted;

        //поля
        protected internal Dictionary<int, double> Table;  //собственно, градуировочная таблица (int Key - уровень, double Value - объем)
        protected internal LevelBase levelBase { get; private set; }  //представление уровня в таблице (миллиметры или сантиметры)
        protected internal string CTid { get; private set; }                       //Id таблицы (применяется имя соответствующего DOC файла)
                
        private void CallEvent(CTEventArgs e, CTStateHandler handler) //вызов событий
        {
            if (handler != null && e != null) handler(this, e);
        }

        #region вызов событий
        protected virtual void OnLevelBaseChanged(CTEventArgs e)    //изменение представления уровня
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, LevelBaseChanged);
            Console.ForegroundColor = cc;
        }

        protected virtual void OnTableSorted(CTEventArgs e)    //сортировка таблицы по возрастанию уровня
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, TableSorted);
            Console.ForegroundColor = cc;
        }

        protected virtual void OnTableChecked(CTEventArgs e)    //проверка таблицы
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, TableChecked);
            Console.ForegroundColor = cc;
        }

        protected virtual void OnTableRepaired(CTEventArgs e)    //исправление таблицы
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, TableRepaired);
            Console.ForegroundColor = cc;
        }

        protected virtual void OnTableRead(CTEventArgs e)    //чтение таблицы
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, TableRead);
            Console.ForegroundColor = cc;
        }

        protected virtual void OnItemAdded(CTEventArgs e)    //добавление пары
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, ItemAdded);
            Console.ForegroundColor = cc;
        }

        protected virtual void OnItemChanged(CTEventArgs e)    //добавление пары
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, ItemChanged);
            Console.ForegroundColor = cc;
        }

        protected virtual void OnItemDeleted(CTEventArgs e)    //удаление пары
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, ItemDeleted);
            Console.ForegroundColor = cc;
        }
        #endregion

        #region конструкторы
        public CalibrationTable(
            string ctid,                            //Id таблицы
            CTStateHandler levelBaseChangeHandler,
            CTStateHandler tableSortHandler,
            CTStateHandler tableCheckHandler,
            CTStateHandler tableRepaireHandler,
            CTStateHandler tableReadHandler,
            CTStateHandler itemAddHandler,
            CTStateHandler itemChangeHandler,
            CTStateHandler itemDeleteHandler)
        {
            //добавили обработчики событий в стеки
            LevelBaseChanged += levelBaseChangeHandler;
            TableSorted += tableSortHandler;
            TableChecked += tableCheckHandler;
            TableRepaired += tableRepaireHandler;
            TableRead += tableReadHandler;
            ItemAdded += itemAddHandler;
            ItemChanged += itemChangeHandler;
            ItemDeleted += itemDeleteHandler;

            //выделили память
            Table = new Dictionary<int, double>();

            //инициализация полей
            CTid = ctid;
            levelBase = LevelBase.Centimeters;
        }
        #endregion

        #region методы private
        private void _sortTable() //сортировка таблицы по увеличению уровня (для внутреннего использования)
        {
            Dictionary<int, double> tempTable = new Dictionary<int, double>();  //временная таблица

            //сортируем
            foreach (KeyValuePair<int, double> kvp in Table.OrderBy(key => key.Key))
                tempTable.Add(kvp.Key, kvp.Value);

            //заново наполняем таблицу уже отсортированными парами
            Table.Clear();
            foreach (KeyValuePair<int, double> kvp in tempTable)
                Table.Add(kvp.Key, kvp.Value);
        }

        private TableErrors _checkTable()   //проверка таблицы на адекватность (внутреннее применение)
        {
            if (Table == null) return TableErrors.TableOK;  //если таблица пуста

            //проверка на отсортированность по уровню
            for (int i = 0; i < Table.Count - 1; i++)
                if (Table.ElementAt(i).Key > Table.ElementAt(i + 1).Key)
                    return TableErrors.UnSorted;

            //проверка уровней на непоследовательность (увеличение всегда идет на одно и то же значение (1 или 10 в зависимости от базы: см или мм))
            int Step = 1;   //шаг уровня для текущего levelBase
            switch (levelBase)
            {
                case LevelBase.Millimeters: Step = 10; break;
                case LevelBase.Centimeters: Step = 1; break;
            }
            for (int i = 0; i < Table.Count - 1; i++)
                if ((Table.ElementAt(i + 1).Key - Table.ElementAt(i).Key) != Step)
                    return TableErrors.LevelInconsistency;

            //проверка объемов на непоследовательность (фактически на отсортированность объемов)
            for (int i = 0; i < Table.Count - 1; i++)
                if (Table.ElementAt(i).Value >= Table.ElementAt(i + 1).Value)
                    return TableErrors.VolumeInconsistency;

            return TableErrors.TableOK;
        }

        private void _repairLevelInconsistency()    //исправление непоследовательности уровней (таблица должна быть отсортирована)
        {
            int Step = 1;   //шаг уровня для текущего levelBase
            switch (levelBase)
            {
                case LevelBase.Millimeters: Step = 10; break;
                case LevelBase.Centimeters: Step = 1; break;
            }

            for (int curIndex = 0; curIndex < Table.Count - 1; curIndex++)
            {
                int delta = Table.ElementAt(curIndex + 1).Key - Table.ElementAt(curIndex).Key;  //расстояние от текущего уровня до следующего
                if (delta == Step) continue;    //если тут не надо исправлять, идем к следующему уровню
                while (delta < Step)   //пока delta меньше шага
                {
                    Table.Remove(Table.ElementAt(curIndex + 1).Key);    //удалили следующий уровень
                    if (curIndex == Table.Count - 1) break;     //если текущий уровень только что стал последним в таблице, закончили
                    delta = Table.ElementAt(curIndex + 1).Key - Table.ElementAt(curIndex).Key;  //пересчитали delta
                }
                if (delta > Step)  //если delta больше шага
                {
                    //рассчитаем значения вновь добавляемого уровня и объема
                    int newLevel = Table.ElementAt(curIndex).Key + Step;
                    double newVolume = (Table.ElementAt(curIndex + 1).Value - Table.ElementAt(curIndex).Value) / delta * Step + Table.ElementAt(curIndex).Value;

                    Table.Add(newLevel, newVolume); //добавим в таблицу

                    _sortTable();   //отсортируем таблицу
                }
            }
        }

        private void _repairVolumeInconsistency()   //исправление непоследовательности объемов (таблица должна быть отсортирована)
        {
            Dictionary<int, double> tempTable = new Dictionary<int, double>();  //временная таблица

            for (int curIndex = 0; curIndex < Table.Count; curIndex++)
            {
                tempTable.Add(Table.ElementAt(curIndex).Key, Table.ElementAt(curIndex).Value);   //скопируем текущую пару во временную таблицу

                int startIndex = curIndex + 1;  //индекс начала последовательности ошибочных объемов
                int stopIndex = 0;              //индекс конца последовательности ошибочных объемов

                //ищем ошибочную последовательность от текущего объема
                for (stopIndex = curIndex + 1; stopIndex < Table.Count; stopIndex++)
                    if (Table.ElementAt(stopIndex).Value > Table.ElementAt(curIndex).Value)
                    {
                        stopIndex--;
                        break;
                    }

                if (stopIndex < startIndex || curIndex == Table.Count - 1) continue;    //ошибочной последовательности от текущего объема не обнаружено
                                                                                        //ИЛИ текущая пара последняя

                //есть ошибочная последовательность - будем аппроксимировать объемы
                double volumeStep = 0.0;    //шаг объема внутри ошибочной последовательности

                if (stopIndex == Table.Count)   //если ошибочная последовательность идет до конца таблицы
                {
                    if (curIndex == 0)              //если текущий объем - первый в таблице, то вся таблица ошибочна
                    {
                        Table.Clear();  //очишаем таблицу
                        return;         //и выходим
                    }
                    else    //если текущий объем не первый, будем аппроксимировать из предыдущих объемов
                    {
                        volumeStep = Table.ElementAt(curIndex).Value - Table.ElementAt(curIndex - 1).Value; //шаг равен расстоянию от текущего до предыдущего
                        stopIndex--;    //нужно для правильной работы следующего for, когда ошибочная последовательность идет до конца таблицы
                    }
                }
                else    //если ошибочная последовательность идет не до конца таблицы, будем аппроксимировать между текущим объемом и следующим не ошибочным
                {
                    volumeStep = (Table.ElementAt(stopIndex + 1).Value - Table.ElementAt(curIndex).Value) / (stopIndex - startIndex + 2);   //шаг - расстояние между текущим и
                                                                                                                                            //первым неошибочным объемами, деленное
                                                                                                                                            //на кол-во промежутков между ними
                                                                                                                                            //(т.е. на кол-во ошибочных объемов плюс один)
                }

                for (int i = startIndex; i <= stopIndex; i++)   //проходим всю ошибочную последовательность
                {
                    int newLevel = Table.ElementAt(i).Key;  //уровень
                    double newVolume = Table.ElementAt(curIndex).Value + volumeStep * (i - startIndex + 1); //новый объем
                    tempTable.Add(newLevel, newVolume); //добавили во временную таблицу
                }
                curIndex = stopIndex;   //текущий объем теперь - последний в ошибочной последовательности
            }

            //перенесем данные из временной таблицы в градуировочную
            Table.Clear();
            foreach (KeyValuePair<int, double> kvp in tempTable)
                Table.Add(kvp.Key, kvp.Value);
        }
        
        private bool _readTable(string Source)  //чтение таблицы из источника (в формате DOC)
        {
            double dblLevel = 0.0,  //считанный из строки уровень, в таблице могут встречаться уровни в double - их нужно будет правильно обработать
                    newVolume = 0.0;    //считанный объем
            int newLevel = 0;       //считанный и обработанный уровень
                        
            if (Source?.IndexOf(TextPattern.FileIsCorrect) == -1 || Source == null) return false;    //если был открыт не файл градуировочной таблицы

            //по паттернам ищем начало градуировочной таблицы в строке
            int start = Source.IndexOf(TextPattern.TableStart1);
            if (start == -1) start = Source.IndexOf(TextPattern.TableStart2);
            if (start == -1) return false;  //если таблицу так и не нашли

            //нашли начало таблицы. исключим все, что идет до нее
            string Src = Source.Substring(start);

            //проверим и при необходимости исправим разделитель целой и дробной частей в соответствии с региональными настройками
            if (!double.TryParse("0.0", out dblLevel)) Src = Src.Replace('.', ',');

            //сделаем в таблице необходимые замены символов
            Src = Src.Replace('\r', '\n');
            Src = Src.Replace(TextPattern.Spacers.VertLine, '\n');
            Src = Src.Replace(TextPattern.Spacers.Colons, '\n');

            //делим на строки по \n, исключаем пустые строки
            //теперь у нас набор строк, в некоторых из которых мусор, а каждая из полезных строк содержит пару уровень-объем
            string[] Strings = Src.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (Strings.Length == 0) return false;  //если нет ни одной строки

            string[][] DataSource = new string[Strings.Length][];   //выделим память

            for (int i = 0; i < Strings.Length; i++)    //смотрим каждую строку
            {
                //делим на подстроки по пробелу, исключаем пустые строки
                //если первые две полученные подстроки представляют из себя числа, то данная подстрока полезна: эти числа как раз уровень и объем
                //остальные варианты строк являются мусором
                DataSource[i] = Strings[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (DataSource[i].Length < 2) continue; //если получили меньше двух подстрок, то эта строка нам точно не подходит - пропускаем

                //читаем уровень и проверяем его на адекватность
                if (double.TryParse(DataSource[i][0], out dblLevel)) //если чтение завершилось удачно, то продолжаем работать с этой парой
                {
                    newLevel = (int)dblLevel;   //откинули дробную часть
                    if ((dblLevel - newLevel) == 0.0)   //если уровень был целым, то он нам подходит
                        if (double.TryParse(DataSource[i][1], out newVolume))   //читаем объем, если удачно, то эта пара нам подходит
                        {
                            if (!Table.ContainsKey(newLevel))   //если такого уровня еще нет в таблице
                                Table.Add(newLevel, newVolume);     //запишем ее в таблицу
                        }

                    //если пара так или иначе нам не подходит, или в данной подстроке затесались не уровень и объем, то пара просто НЕ будет записана в таблицу
                    //в таблице мертвой полости кроме уровня и объема есть еще вместимость на 1мм наполнения, она будет в подстроке третьим числом, но она нам не нужна
                }
            }

            if (Table.Count == 0) return false; //если в результате обработки таблица осталась пуста, то ничего не прочитано
            return true;
        }

        private bool _changeVolume(int level, double volume)   //изменение объема по заданному уровню
        {
            if (Table.ContainsKey(level))       //если данный уровень есть в таблице
            {
                Dictionary<int, double> tempTable = new Dictionary<int, double>();
                for (int i = 0; i < Table.Count; i++)
                {
                    if (Table.ElementAt(i).Key == level) tempTable.Add(level, volume);   //дошли до изменяемого уровня
                    else tempTable.Add(Table.ElementAt(i).Key, Table.ElementAt(i).Value);  //остальные уровни
                }

                Table.Clear();
                for (int i = 0; i < tempTable.Count; i++)
                    Table.Add(tempTable.ElementAt(i).Key, tempTable.ElementAt(i).Value);

                return true;
            }

            return false;
        }
        #endregion

        #region методы public
        public void CopyTo(ref CalibrationTable Dest)   //копирование градуировочной таблицы
        {
            Dest = new CalibrationTable(
                CTid,
                LevelBaseChanged,
                TableSorted,
                TableChecked,
                TableRepaired,
                TableRead,
                ItemAdded,
                ItemChanged,
                ItemDeleted);

            Dest.ChangeLevelBase(levelBase, true);
            for (int i = 0; i < Table.Count; i++)
                Dest.Table.Add(Table.ElementAt(i).Key, Table.ElementAt(i).Value);
        }

        public void ChangeLevelBase(LevelBase newLevelBase, bool Quiet = false) //смена представления уровня
        {
            //для генерации события
            string resMessage = "Представление уровня в таблице " + CTid;
            bool resSuccess = false;
            Dictionary<int, double> tempTable = new Dictionary<int, double>();

            if (newLevelBase != levelBase)  //если новая база отличается от текущей
            {
                resMessage += " успешно изменено с ";// + levelBase.ToString() + " на " + newLevelBase.ToString();
                switch (levelBase)
                {
                    case LevelBase.Millimeters: resMessage += "миллиметров"; break;
                    case LevelBase.Centimeters: resMessage += "сантиметров"; break;
                }
                resMessage += " на ";
                switch (newLevelBase)
                {
                    case LevelBase.Millimeters: resMessage += "миллиметры"; break;
                    case LevelBase.Centimeters: resMessage += "сантиметры"; break;
                }
                resSuccess = true;

                levelBase = newLevelBase;   //сохранили новое представление
                double Multiplier = 1;      //множитель для смены представления

                switch (levelBase)           //выбираем направление изменения
                {
                    case LevelBase.Millimeters: //см меняем на мм
                        Multiplier = 10;        //домножим на 10
                        break;
                    case LevelBase.Centimeters: //мм меняем на см
                        Multiplier = 0.1;       //домножим на 0.1 (разделим на 10)
                        break;
                }
                for (int i = 0; i < Table.Count; i++)
                {
                    int k = Table.ElementAt(i).Key;
                    double v = Table.ElementAt(i).Value;
                    k = (int)(k * Multiplier);
                    tempTable.Add(k, v);
                }
                Table.Clear();
                for (int i = 0; i < tempTable.Count; i++)
                    Table.Add(tempTable.ElementAt(i).Key, tempTable.ElementAt(i).Value);
                _sortTable();
            }
            else
            {
                resMessage += " изменено не было!";
                resSuccess = false;
            }

            //генерация события
            if (!Quiet) OnLevelBaseChanged(new CTEventArgs(resSuccess, resMessage));
        }

        public void SortTable(bool Quiet = false) //сортировка таблицы по увеличению уровня (для внешнего использования)
        {
            _sortTable();   //сортируем таблицу

            //генерация события
            if (!Quiet) OnTableSorted(new CTEventArgs(true, "Таблица " + CTid + " успешно отсортирована"));
        }

        public void ShowTable(bool WaitWhenFinished = true, bool ClearWhenFinished = true)
        {
            string TableCaption = "",   //заголовок таблицы
                TableText = "",         //надпись "таблица пуста"
                TableHead = "";         //шапка таблицы
            int[] PairStart;            //начало каждой пары в строке TableText
            
            //запомним цветовые установки
            ConsoleColor cc = Console.ForegroundColor;
            
            //очистим окно
            Console.Clear();

            //запишем заголовок
            TableCaption += "Градуировочная таблица: " + CTid + " (<Esc> для выхода)\n\n";

            //запишем таблицу
            if (Table == null || Table.Count == 0)
            {
                //если таблица пуста
                PairStart = new int[2];
                PairStart[0] = 0;
                TableText += "Таблица пуста\n";
                PairStart[1] = TableText.Length;
            }
            else
            {
                //если в таблице есть записи

                int VolumeAccuracy = 3;     //до какой цифры после запятой писать объем

                //определим точность записи объемов
                double tmpVol = Table.ElementAt(0).Value * Math.Pow(10, VolumeAccuracy);
                while (tmpVol != 0 && VolumeAccuracy <= 10)
                {
                    VolumeAccuracy++;
                    tmpVol *= 10;
                    tmpVol -= Math.Truncate(tmpVol);
                }

                //пишем шапку
                TableHead += "Уровень, ";
                switch (levelBase)
                {
                    case LevelBase.Millimeters:
                        TableHead += "мм";
                        break;
                    case LevelBase.Centimeters:
                        TableHead += "см";
                        break;
                }
                TableHead += "\tОбъем, куб.м\n\n";

                //пишем саму таблицу
                PairStart = new int[Table.Count + 1];
                PairStart[0] = 0;
                for (int i = 0; i < Table.Count; i++)
                {
                    TableText += Table.ElementAt(i).Key + "\t\t" + Table.ElementAt(i).Value.ToString("F" + VolumeAccuracy) + "\n";
                    PairStart[i + 1] = TableText.Length;
                }
            }

            if (WaitWhenFinished)
            {
                bool WasCursorVisible = Console.CursorVisible;
                Console.CursorVisible = false;

                ConsoleKey ck;
                int firstPairVisible = 0;   //первая отображаемая на экране пара
                int CurrentRowsVisible = Console.WindowHeight - 5;  //кол-во строк таблицы на экране

                do
                {
                    //отобразим таблицу
                    Console.SetCursorPosition(0, 0);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(TableCaption);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(TableHead);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(TableText.Substring(PairStart[firstPairVisible],
                        PairStart[firstPairVisible + CurrentRowsVisible] - PairStart[firstPairVisible]));

                    ck = Console.ReadKey(true).Key;

                    //определим текущее кол-во строк таблицы на экране
                    CurrentRowsVisible = Console.WindowHeight - 5;

                    switch (ck)
                    {
                        case ConsoleKey.UpArrow:    //стрелка вверх, сдвижка на пару вверх
                            firstPairVisible = (--firstPairVisible < 0) ? 0 : firstPairVisible;
                            break;
                        case ConsoleKey.DownArrow:  //стрелка вниз, сдвижка на пару вниз
                            firstPairVisible = (++firstPairVisible > (Table.Count - CurrentRowsVisible)) ?
                                    (Table.Count - CurrentRowsVisible) :
                                    firstPairVisible;
                            break;
                        case ConsoleKey.PageUp: //сдвижка вверх на кол-во отображаемых на экране пар
                            firstPairVisible -= CurrentRowsVisible;
                            firstPairVisible = (firstPairVisible < 0) ? 0 : firstPairVisible;
                            break;
                        case ConsoleKey.PageDown:   //сдвижка вниз на кол-во отображаемых на экране пар
                            firstPairVisible += CurrentRowsVisible;
                            firstPairVisible = (firstPairVisible > (Table.Count - CurrentRowsVisible)) ?
                                (Table.Count - CurrentRowsVisible) :
                                firstPairVisible;
                            break;
                        case ConsoleKey.Home:   //прыгнули в самый верх
                            firstPairVisible = 0;
                            break;
                        case ConsoleKey.End:    //прыгнули в самый низ
                            firstPairVisible = Table.Count - CurrentRowsVisible;
                            break;
                    }
                } while (ck != ConsoleKey.Escape);  //пока не нажмут Esc
                if (ClearWhenFinished) Console.Clear();
                Console.CursorVisible = WasCursorVisible;
            }

            Console.ForegroundColor = cc;
        }

        public void CheckTable() //проверка таблицы на адекватность (доступ к методу извне)
        {
            string _message = "Таблица " + CTid + ": ";
            bool _result = false;

            switch (_checkTable())
            {
                case TableErrors.TableOK:
                    _result = true;
                    _message += "ошибок не обнаружено";
                    break;
                case TableErrors.UnSorted:
                    _message += "требуется сортировка";
                    break;
                case TableErrors.LevelInconsistency:
                    _message += "обнаружена непоследовательность уровней";
                    break;
                case TableErrors.VolumeInconsistency:
                    _message += "обнаружена непоследовательность объемов";
                    break;
            }

            //генерация события
            OnTableChecked(new CTEventArgs(_result, _message));
        }

        public void RepairTable(bool repairVolInconcictensy = false, bool Quiet = false)       //исправление ошибок в таблице
        {
            TableErrors check = _checkTable();  //определение, что требуется исправить
            bool isSmthChanged = false;         //флаг наличия исправлений в таблице

            if (!Quiet) OnTableRepaired(new CTEventArgs(true, "Выполняется исправление таблицы " + CTid));

            do
            {
                switch (check)
                {
                    case TableErrors.TableOK:
                        if (!Quiet) OnTableRepaired(new CTEventArgs(true, "\t\tошибки не обнаружены"));
                        break;
                    case TableErrors.UnSorted:
                        _sortTable();
                        isSmthChanged = true;
                        if (!Quiet) OnTableRepaired(new CTEventArgs(false, "\t\tтаблица отсортирована"));
                        break;
                    case TableErrors.LevelInconsistency:
                        _repairLevelInconsistency();
                        isSmthChanged = true;
                        if (!Quiet) OnTableRepaired(new CTEventArgs(false, "\t\tисправлена непоследовательность уровней"));
                        break;
                    case TableErrors.VolumeInconsistency:
                        if (repairVolInconcictensy)
                        {
                            _repairVolumeInconsistency();
                            isSmthChanged = true;
                            if (!Quiet) OnTableRepaired(new CTEventArgs(false, "\t\tисправлена непоследовательность объемов"));
                        }
                        break;
                }
                check = _checkTable();  //определение, что требуется исправить
                //если исправлять непоследовательность объемов не надо и осталась только эта ошибка, прервем процесс восстановления
                if (check == TableErrors.VolumeInconsistency && !repairVolInconcictensy) break;
            } while (check != TableErrors.TableOK); //пока есть, что исправлять

            if (!Quiet)
            {
                if (isSmthChanged)
                    OnTableRepaired(new CTEventArgs(true, "\tвсе ошибки исправлены"));
                else
                    OnTableRepaired(new CTEventArgs(true, "\tисправление таблицы не требуется"));
            }
        }

        public bool ReadTable(string Source, bool Quiet = false)    //чтение таблицы из строки (взятой из DOC)
        {
            bool res = _readTable(Source); //читаем

            //генерация событий
            if (!Quiet)
            {
                if (res)
                    OnTableRepaired(new CTEventArgs(res, "В таблицу " + CTid + " успешно прочитано " + Table.Count + " записей"));
                else
                    OnTableRepaired(new CTEventArgs(res, "Таблицу " + CTid + " прочитать не удалось"));
            }

            return res;
        }

        public void AddPair(bool Quiet = false)   //добавление пары значений в таблицу
        {
            int newLevel = 0;
            double newVolume = 0.0;

            Console.Clear();
            Console.WriteLine("Добавление новой записи в таблицу " + CTid);
            Console.WriteLine();
            Console.Write("Задайте значение уровня в " + ((levelBase == LevelBase.Centimeters) ? "см: " : "мм: "));
            if (int.TryParse(Console.ReadLine(), out newLevel))  //если ввод корректен
            {
                if (!Table.ContainsKey(newLevel))    //если такого уровня еще нет
                {
                    Console.Write("Задайте значение объема в куб.м (для отмены пустая строка): ");
                    if (double.TryParse(Console.ReadLine(), out newVolume))
                    {
                        Table.Add(newLevel, newVolume);
                        //генерация события
                        if (!Quiet) OnItemAdded(new CTEventArgs(true, "Новая запись успешно добавлена в таблицу", 1000));
                    }
                    else
                    {
                        //генерация события
                        if (!Quiet) OnItemAdded(new CTEventArgs(false, "Добавление записи не произведено", 2000));
                    }
                }
                else    //такой уровень уже есть
                {
                    if (ChangeVolume(newLevel, true)) //изменим его объем
                    {
                        //генерация события
                        if (!Quiet) OnItemAdded(new CTEventArgs(true, "Изменен объем для уровня " + newLevel, 1000));
                    }
                    else
                    {
                        //генерация события
                        if (!Quiet) OnItemAdded(new CTEventArgs(false, "Изменение объема для уровня " + newLevel + " не произведено", 1500));
                    }
                }
            }
            else    //ввод уровня некорректен
            {
                //генерация события
                if (!Quiet) OnItemAdded(new CTEventArgs(false, "Заданное значение уровня не является целым числом", 2000));
            }
        }

        public bool ChangeVolume(int level = -1, bool Quiet = false)  //изменение объема по заданному уровню (доступ извне)
        {
            double newVolume = 0.0;
            bool res = false;

            //отображаем записи таблицы в виде меню
            int MenuChoice = 0,
                ItemChoice = 0;
            int currentItem = (level == -1) ? 0 : level;
            string[][] MenuItems = new string[][] { new string[Table.Count] };

            int VolumeAccuracy = 3;     //до какой цифры после запятой писать объем

            //определим точность записи объемов
            double tmpVol = Table.ElementAt(0).Value * Math.Pow(10, VolumeAccuracy);
            while (tmpVol != 0 && VolumeAccuracy <= 10)
            {
                VolumeAccuracy++;
                tmpVol *= 10;
                tmpVol -= Math.Truncate(tmpVol);
            }

            for (int i = 0; i < Table.Count; i++)
                MenuItems[0][i] = Table.ElementAt(i).Key.ToString() + "\t\t" + Table.ElementAt(i).Value.ToString("F" + VolumeAccuracy);

            Menu M = new MenuLibrary.Menu(
                new string[] { "Таблица " + CTid + ". Выберите запись или нажмите <Esc> для возврата" },
                MenuItems,
                "Поиск уровня:",
                Console.WindowHeight - 5);

            do
            {
                M.MenuCicle(out MenuChoice, out ItemChoice, 0, currentItem);
                switch (MenuChoice)
                {
                    case -1:    //отмена меню
                        return false;

                    case 0:
                        Console.Clear();
                        Console.Write($"Задайте новое значение объема для уровня {Table.ElementAt(ItemChoice).Key} в куб.м (для отмены пустая строка): ");
                        if (double.TryParse(Console.ReadLine(), out newVolume))
                        {
                            res = _changeVolume(Table.ElementAt(ItemChoice).Key, newVolume);
                            //генерация события
                            if (!Quiet) OnItemChanged(new CTEventArgs(true, "Объем для уровня " + Table.ElementAt(ItemChoice).Key + " изменен на " + newVolume, 1000));
                        }
                        else
                        {
                            //генерация события
                            if (!Quiet) OnItemChanged(new CTEventArgs(false, "Изменение таблицы не произведено", 1500));
                            res = false;
                        }
                        break;

                    default:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ошибка в работе меню изменения записи таблицы!!!\n\nНажмите любую клавишу для выхода...");
                        Console.ReadKey(true);
                        Environment.Exit(-1);
                        break;
                }

            } while (!(MenuChoice == -1 || MenuChoice == 0));

            return res;
        }

        public void DeletePair(bool Quiet = false)    //удаление пары из таблицы
        {
            int level = 0;

            //отображаем записи таблицы в виде меню
            int MenuChoice = 0,
                ItemChoice = 0;
            int currentItem = (level == -1) ? 0 : level;
            string[][] MenuItems = new string[][] { new string[Table.Count] };

            int VolumeAccuracy = 3;     //до какой цифры после запятой писать объем

            //определим точность записи объемов
            double tmpVol = Table.ElementAt(0).Value * Math.Pow(10, VolumeAccuracy);
            while (tmpVol != 0 && VolumeAccuracy <= 10)
            {
                VolumeAccuracy++;
                tmpVol *= 10;
                tmpVol -= Math.Truncate(tmpVol);
            }

            for (int i = 0; i < Table.Count; i++)
                MenuItems[0][i] = Table.ElementAt(i).Key.ToString() + "\t\t" + Table.ElementAt(i).Value.ToString("F" + VolumeAccuracy);

            Menu M = new MenuLibrary.Menu(
                new string[] { "Таблица " + CTid + ". Выберите запись или нажмите <Esc> для возврата" },
                MenuItems,
                "Поиск уровня:",
                Console.WindowHeight - 5);

            do
            {
                M.MenuCicle(out MenuChoice, out ItemChoice, 0, currentItem);
                switch (MenuChoice)
                {
                    case -1:    //отмена меню
                        break;

                    case 0:
                        Console.Clear();
                        Console.WriteLine("Удалить запись {0} - {1:F3}?\n<Delete> для подтверждения, любая другая клавиша для выхода.",
                            Table.ElementAt(ItemChoice).Key,
                            Table.ElementAt(ItemChoice).Value);

                        if (Console.ReadKey(true).Key == ConsoleKey.Delete)
                        {
                            if (Table.Remove(Table.ElementAt(ItemChoice).Key))    //если удалено удачно
                            {
                                //генерация события
                                if (!Quiet)
                                {
                                    OnItemDeleted(
                                        new CTEventArgs(
                                            true,

                                            "Запись " + Table.ElementAt(ItemChoice).Key.ToString() +
                                            " - " + Table.ElementAt(ItemChoice).Value.ToString("F3") +
                                            " успешно удалена",

                                            1000));
                                }
                            }
                            else
                            {
                                //генерация события
                                if (!Quiet) OnItemDeleted(new CTEventArgs(false, "Удаление записи из таблицы не произведено", 1500));
                            }
                        }
                        return;

                    default:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ошибка в работе меню удаления записи таблицы!!!\n\nНажмите любую клавишу для выхода...");
                        Console.ReadKey(true);
                        Environment.Exit(-1);
                        break;
                }

            } while (!(MenuChoice == -1 || MenuChoice == 0));

            //генерация события
            if (!Quiet) OnItemDeleted(new CTEventArgs(false, "Удаление записи из таблицы отменено", 1500));
        }
        #endregion
    }
}
