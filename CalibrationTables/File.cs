using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using DataProcesssingLibrary;

namespace FileLibrary   //работа с файлами
{
    public class DOCFile    //работа с файлами DOC и DOCX
    {
        //делегат для создания событий
        public delegate void DOCFileStateHandler(object sender, FileEventArgs e);

        //события
        protected internal event DOCFileStateHandler FileRead;
        protected internal event DOCFileStateHandler ManyFilesRead;
        protected internal event DOCFileStateHandler FileOpened;
        protected internal event DOCFileStateHandler FileClosed;

        //поля
        protected internal string[] filePath;          //пути к документам
        protected internal string[] fileData;          //данные из документов
        protected internal string[] fileId;             //Id файлов, используем имена файлов без расширения

        private void CallEvent(FileEventArgs e, DOCFileStateHandler handler) //вызов событий
        {
            if (handler != null && e != null) handler(this, e);
        }

        #region вызов событий
        protected virtual void OnManyFilesRead(FileEventArgs e)   //при конструировании класса
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, ManyFilesRead);
            Console.ForegroundColor = cc;
        }

        protected virtual void OnFileRead(FileEventArgs e)   //при чтении файла
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, FileRead);
            Console.ForegroundColor = cc;
        }

        protected virtual void OnFileOpened(FileEventArgs e) //при открытии файла
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, FileOpened);
            Console.ForegroundColor = cc;
        }

        protected virtual void OnFileClosed(FileEventArgs e)    //при закрытии файла
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, FileClosed);
            Console.ForegroundColor = cc;
        }
        #endregion

        #region  конструкторы
        public DOCFile(
            DOCFileStateHandler openFileHandler,
            DOCFileStateHandler readFileHandler,
            DOCFileStateHandler manyFilesReadHandler,
            DOCFileStateHandler closeFileHandler)
        {
            //добавили обработчики событий в стеки
            FileOpened += openFileHandler;
            FileRead += readFileHandler;
            ManyFilesRead += manyFilesReadHandler;
            FileClosed += closeFileHandler;

            //инициализировали поля
            filePath = null;
            fileData = null;
            fileId = null;
        }
        #endregion

        #region  методы private
        private string _readFile(   //чтение заданного документа
            string path,              //путь к документу (для генерации события)
            Word.Document document,     //заданный документ
            bool Quiet = false)
        {
            Word.Range Rng = document.Range();    //берем все содержимое файла
            string res = Rng?.Text;                //и если оно существует, сохраняем в res

            //генерируем событие
            if (!Quiet)
            {
                int pos = path.LastIndexOf('\\') + 1;
                if (res == null)
                    OnFileRead(new FileEventArgs(false, "Не удалось прочитать файл: " + path.Substring(pos)));
                else
                    OnFileRead(new FileEventArgs(true, "Прочитан файл: " + path.Substring(pos)));
            }

            return res;
        }

        private void _closeFile(            //закрытие заданного файла
            string path,                    //путь к документу (для генерации события)
            ref Word.Document document,
            ref Word.Application application,   //экземпляр word
            bool Quiet = false)
        {
            //просто закрыли соответствующий экземпляр Word
            application.Quit();

            //генерируем событие
            if (!Quiet)
            {
                int pos = path.LastIndexOf('\\') + 1;
                OnFileClosed(new FileEventArgs(true, "Закрыт файл: " + path.Substring(pos)));
            }
        }

        private string _chooseFile()   //выбор файла для чтения
        {
            //диалог открытия файла
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Application.StartupPath;
            ofd.Filter = "Документ Word (*.docx, *.doc)|*.docx;*.doc";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;
            ofd.Multiselect = false;

            string result = "";

            if (ofd.ShowDialog() == DialogResult.OK) result = ofd.FileName;    //сохраним путь к выбранному файлу
            else result = null;    //если файл не был выбран

            return result;
        }

        private void _openFile(                 //открытие заданного документа
            string path,                        //путь к документу
            out Word.Application application,   //возвращаемый экземпляр word
            out Word.Document document,         //возвращаемый читаемый документ
            bool Quiet = false)
        {
            //задаем параметры открытия
            object fileName = path;
            object confirmConversions = false;
            object readOnly = Type.Missing;
            object addToRecentFiles = false;
            object passwordDocument = Type.Missing;
            object passwordTemplate = Type.Missing;
            object revert = Type.Missing;
            object writePasswordDocument = Type.Missing;
            object writePasswordTemplate = Type.Missing;
            object format = Type.Missing;
            object encoding = Office.MsoEncoding.msoEncodingAutoDetect;
            object visible = Type.Missing;
            object openConflictDocument = Type.Missing;
            object openAndRepair = Type.Missing;
            object documentDirection = Type.Missing;
            object noEncodingDialog = true;

            application = new Word.Application();    //создаем экземпляр Word

            //открываем в нем документ
            application.Documents.Open(ref fileName,
                ref confirmConversions,
                ref readOnly,
                ref addToRecentFiles,
                ref passwordDocument,
                ref passwordTemplate,
                ref revert,
                ref writePasswordDocument,
                ref writePasswordTemplate,
                ref format,
                ref encoding,
                ref visible,
                ref openAndRepair,
                ref documentDirection,
                ref noEncodingDialog);

            //сохраняем ссылку на открытый документ
            document = new Word.Document();
            document = application.Documents.Application.ActiveDocument;

            //генерируем событие
            if (!Quiet)
            {
                int pos = path.LastIndexOf('\\') + 1;
                OnFileOpened(new FileEventArgs(true, "Открыт файл: " + path.Substring(pos)));
            }
        }
        #endregion

        #region методы protected internal
        protected internal void ReadOneFile()       //чтение одного файла
        {
            string tmpFilePath = "";    //временные путь к файлу

            tmpFilePath = _chooseFile();   //выбор файла
            if (tmpFilePath != null)
            {
                //временные переменные
                Word.Application tmpWordApp;
                Word.Document tmpWordDOC;
                string tmpFileData = "";
                string tmpFileId = "";

                //заполнили ID файла
                int start = tmpFilePath.LastIndexOf('\\') + 1,
                    stop = tmpFilePath.IndexOf('.', start);
                tmpFileId = tmpFilePath.Substring(start, stop - start);

                _openFile(tmpFilePath, out tmpWordApp, out tmpWordDOC);     //откроем
                tmpFileData = _readFile(tmpFilePath, tmpWordDOC);           //прочитаем
                _closeFile(tmpFilePath, ref tmpWordDOC, ref tmpWordApp);    //закроем

                if (tmpFileData.IndexOf(TextPattern.FileIsCorrect) != -1)   //проверяем, что файл нам подходит
                {
                    //---==сохраним данные по документам
                    //путь к файлу
                    Array.Resize(ref filePath, 1);
                    filePath[0] = tmpFilePath;
                    //Id файла
                    Array.Resize(ref fileId, 1);
                    fileId[0] = tmpFileId;
                    //данные из файла
                    Array.Resize(ref fileData, 1);
                    fileData[0] = tmpFileData;
                }
            }
        }

        protected internal void ReadManyFiles(out string FilePath, bool Quiet = false) //пакетное чтение файлов
        {
            bool res = false;   //результат чтения из файла
            int totalFiles = 0, //всего DOC файлов в папке
                    readFiles = 0;  //прочитано подходящих файлов
            FilePath = null;     //папка с файлами

            //диалог выбора папки
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Выберите папку с градуировочными таблицами";
            fbd.ShowNewFolderButton = false;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                FilePath = fbd.SelectedPath;    //сохранили выбранную папку

                //берем список файлов в ней и проверяем каждый
                string[] ListOfFiles = Directory.GetFiles(FilePath);
                foreach (string tmpFilePath in ListOfFiles)
                {
                    FileInfo fi = new FileInfo(tmpFilePath);
                    if (fi.Extension == ".doc" || fi.Extension == ".docx")  //нам нужны только DOC и DOCX
                    {
                        totalFiles++;   //увеличим счетчик документов word

                        //временные переменные
                        Word.Application tmpWordApp;
                        Word.Document tmpWordDOC;
                        string tmpFileData = "";
                        string tmpFileId = "";

                        //заполнили ID файла
                        int start = tmpFilePath.LastIndexOf('\\') + 1,
                            stop = tmpFilePath.IndexOf('.', start);
                        tmpFileId = tmpFilePath.Substring(start, stop - start);

                        _openFile(tmpFilePath, out tmpWordApp, out tmpWordDOC, Quiet);     //откроем
                        tmpFileData = _readFile(tmpFilePath, tmpWordDOC, Quiet);           //прочитаем
                        _closeFile(tmpFilePath, ref tmpWordDOC, ref tmpWordApp, Quiet);                        //закроем

                        if (tmpFileData.IndexOf(TextPattern.FileIsCorrect) != -1)   //проверяем, что файл нам подходит
                        {
                            readFiles++;    //увеличим счетчик прочитанных документов

                            //сохраним данные по документам
                            Array.Resize(ref filePath, readFiles);
                            filePath[readFiles - 1] = tmpFilePath;
                            Array.Resize(ref fileId, readFiles);
                            fileId[readFiles - 1] = tmpFileId;
                            Array.Resize(ref fileData, readFiles);
                            fileData[readFiles - 1] = tmpFileData;
                        }
                    }
                }
            }

            if (readFiles != 0) res = true; //если прочитан хоть один файл

            //генерация событий
            if (!Quiet)
            {
                if (FilePath == null) OnManyFilesRead(new FileEventArgs(false, "Папка не выбрана"));
                else
                {
                    OnManyFilesRead(new FileEventArgs(true, "В папке " + FilePath + " обнаружено " + totalFiles + " документов Word"));
                    if (res) OnManyFilesRead(new FileEventArgs(res, "\tуспешно прочитано " + readFiles + " документов, содержащих градуировочные таблицы"));
                    else OnManyFilesRead(new FileEventArgs(res, "\tдокументов, содержащих градуировочные таблицы не обнаружено"));
                }
            }
        }

        protected internal void ShowFile(int nFile) //отображение содержимого файла
        {
            string str = fileData[nFile].Replace('\r', '\n');   //для отображения в консоли
            Console.WriteLine(str);
        }
        #endregion
    }

    public class TXTFile    //работа с файлами TXT
    {
        //делегат для создания событий
        public delegate void TXTFileStateHandler(object sender, FileEventArgs e);

        //события
        protected internal event TXTFileStateHandler ManyFilesWritten;
        protected internal event TXTFileStateHandler FileWritten;
        protected internal event TXTFileStateHandler FileCreated;
        protected internal event TXTFileStateHandler FileSaved;

        private void CallEvent(FileEventArgs e, TXTFileStateHandler handler) //вызов событий
        {
            if (handler != null && e != null) handler(this, e);
        }

        #region вызов событий
        protected virtual void OnManyFilesWritten(FileEventArgs e)   //при записи в файл
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, ManyFilesWritten);
            Console.ForegroundColor = cc;
        }

        protected virtual void OnFileWritten(FileEventArgs e)   //при записи в файл
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, FileWritten);
            Console.ForegroundColor = cc;
        }

        protected virtual void OnFileCreated(FileEventArgs e) //при создании файла
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, FileCreated);
            Console.ForegroundColor = cc;
        }

        protected virtual void OnFileSaved(FileEventArgs e)    //при сохранении файла
        {
            ConsoleColor cc = Console.ForegroundColor;
            if (e.Success) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
            CallEvent(e, FileSaved);
            Console.ForegroundColor = cc;
        }
        #endregion

        #region конструкторы
        public TXTFile(                         //конструктор для работы с отдельным файлом            
            TXTFileStateHandler createFileHandler,
            TXTFileStateHandler writeFileHandler,
            TXTFileStateHandler manyFilesWriteHandler,
            TXTFileStateHandler saveFileHandler)
        {
            //добавили обработчики событий в стеки
            ManyFilesWritten += manyFilesWriteHandler;
            FileCreated += createFileHandler;
            FileWritten += writeFileHandler;
            FileSaved += saveFileHandler;
        }
        #endregion

        #region  методы private
        private void _writeFile(            //запись в заданный файл указанных данных
            StreamWriter File,              //заданный файл
            string FilePath,                //путь к файлу
            CalibrationTable CT,            //таблица для записи
            bool writeValcomHeader = false, //опция: писать ли в файл заголовок Valcom
            bool Quiet = false)
        {
            int VolumeAccuracy = 3;     //до какой цифры после запятой писать объем

            //определим точность записи объемов
            double tmpVol = CT.Table.ElementAt(0).Value * Math.Pow(10, VolumeAccuracy);
            tmpVol -= Math.Truncate(tmpVol);
            while (tmpVol != 0 && VolumeAccuracy <= 10)
            {
                VolumeAccuracy++;
                tmpVol *= 10;
                tmpVol -= Math.Truncate(tmpVol);
            }

            if (writeValcomHeader)           //запись заголовка Valcom
            {
                File.WriteLine("IndexValue={0}", CT.Table.ElementAt(0).Key);
                File.WriteLine("ScaleValue=1");
                File.Write("StepValue=");
                switch (CT.levelBase)
                {
                    case LevelBase.Millimeters: File.WriteLine("10"); break;
                    case LevelBase.Centimeters: File.WriteLine("1"); break;
                }
            }

            foreach (KeyValuePair<int, double> pair in CT.Table) //для каждой пары из таблицы
            {
                File.Write(pair.Key);                      //пишем ключ
                File.Write(" ");                           //пишем пробел
                File.WriteLine(pair.Value.ToString("F" + VolumeAccuracy)); //пишем значение (три знака после запятой) и переходим на новую строку
            }

            //генерируем событие
            if (!Quiet)
            {
                int pos = FilePath.LastIndexOf('\\') + 1;
                OnFileWritten(new FileEventArgs(true, "Записаны данные в файл: " + FilePath.Substring(pos)));
            }
        }

        private void _saveFile(StreamWriter fileSW, string filePath, bool Quiet = false)   //сохранение и закрытие заданного файла
        {
            //просто закрыли соответствующий файл
            fileSW.Close();

            //генерируем событие
            if (!Quiet)
            {
                int pos = filePath.LastIndexOf('\\') + 1;
                OnFileSaved(new FileEventArgs(true, "Сохранен и закрыт файл: " + filePath.Substring(pos)));
            }
        }

        private string _chooseFile(string fileName, string path = null)   //выбор файла для сохранения
        {
            //диалог сохранения файла
            SaveFileDialog sfd = new SaveFileDialog();
            if (path == null) sfd.InitialDirectory = Application.StartupPath;
            else sfd.InitialDirectory = path;
            sfd.Filter = "Текстовый документ (*.txt)|*.txt";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;
            sfd.AddExtension = true;
            sfd.OverwritePrompt = true;
            sfd.ValidateNames = true;
            sfd.FileName = fileName;

            string filePath;

            if (sfd.ShowDialog() == DialogResult.OK) filePath = sfd.FileName;    //сохраним путь к выбранному файлу
            else filePath = null;    //если файл не был выбран

            return filePath;
        }

        private bool _createFile(out StreamWriter fileSW, string filePath, bool Quiet = false)    //создание заданного файла
        {
            bool res = false;
            string eventString = "";

            //создаем файл
            try
            {
                fileSW = new StreamWriter(filePath, false, Encoding.Default);
                res = true;
                int pos = filePath.LastIndexOf('\\') + 1;
                eventString = "Создан файл " + filePath.Substring(pos);
            }
            catch (Exception)
            {
                fileSW = null;
                res = false;
                eventString = "Создание файла не возможно";
            }

            //генерируем событие
            if (!Quiet) OnFileCreated(new FileEventArgs(res, eventString));

            return res;
        }
        #endregion

        #region методы protected internal
        protected internal void WriteFile(      //запись одного файла
            CalibrationTable CT,                //данные для записи в файл
            string path = null,                 //папка для сохранения файла
            bool writeValcomHeader = false)
        {
            //выделили память
            //filePath = new string[1];
            //fileSW = new StreamWriter[1];
            string filePath;
            StreamWriter fileSW;

            filePath = _chooseFile(CT.CTid, path);       //выбор файла
            if (_createFile(out fileSW, filePath))      //создали
            {
                _writeFile(fileSW, filePath, CT, writeValcomHeader);   //записали
                _saveFile(fileSW, filePath);        //сохранили и закрыли
            }
        }

        protected internal void WriteManyFiles(    //пакетная запись файлов
            CalibrationTable[] CT,                //данные для записи в файл
            string path = null,                 //папка для сохранения файла
            bool writeValcomHeader = false,
            bool Quiet = false)
        {
            int savedFiles = 0; //кол-во записанных файлов

            //выделение памяти
            StreamWriter fileSW;
            string filePath;


            for (int i = 0; i < CT.Length; i++)  //для всех таблиц
            {
                filePath = path + "\\" + CT[i].CTid + ".txt";    //генерируем имя файла
                if (_createFile(out fileSW, filePath, Quiet))          //создадим
                {
                    _writeFile(fileSW, filePath, CT[i], writeValcomHeader, Quiet);    //запишем
                    _saveFile(fileSW, filePath, Quiet);            //сохраним и закроем
                    savedFiles++;
                }
            }

            //генерация событий
            if (!Quiet)
            {
                if (savedFiles > 0) OnManyFilesWritten(new FileEventArgs(true, "Успешно сохранено " + savedFiles + " файлов", 1000));
                else OnManyFilesWritten(new FileEventArgs(false, "Файлы сохранены не были", 1000));
            }
        }
        #endregion
    }

    public class FileEventArgs : EventArgs   //аргументы событий по DOC файлам
    {
        //поля
        public string Message { get; private set; }     //Сообщение        
        public bool Success { get; private set; }       //признак успешности операции
        public int Duration { get; private set; }       //продолжительность отображения сообщения в милисекундах

        //конструкторы
        public FileEventArgs(bool _suc, string _mes, int _dur = 500)
        {
            Success = _suc;
            Message = _mes;
            Duration = _dur;
        }
    }
}