using System;
using System.IO;
using System.Threading;

namespace wc {
    public struct Count {
        public object myLock;
        public long Words;
        public long Lines;
        public long Characters;
        public bool Previous;
    }

    public struct FileData {
        public FileStream fs;
        public long numBytesToRead;
        public int charArraySize;
        public char[] characters;
    }

    class Program {
        static Count wordCount(char[] charArray, Count localCount) {
            foreach (char c in charArray) {
                switch(c) {
                    case '\n':
                        if (localCount.Previous == false) {
                            localCount.Words++;
                            localCount.Previous = true;
                        }
                        localCount.Lines++;
                        break;
                    case '\t':
                        if (localCount.Previous == false) {
                            localCount.Words++;
                            localCount.Previous = true;
                        }
                        break;
                    case ' ':
                        if (localCount.Previous == false) {
                            localCount.Words++;
                            localCount.Previous = true;
                        }
                        break;
                    default:
                        localCount.Previous = false;
                        break;
                }
                localCount.Characters++;
            }
            return localCount;
        }
        static FileData readChar(FileData file) {
            int currentByte;
             for (int j = 0; j < file.characters.Length && (currentByte = file.fs.ReadByte()) != -1; j++) {
                 file.characters[j] = (char)currentByte; 
                 file.numBytesToRead--;
             }
             return file;
        }

        static FileData resetArray(FileData file) {
            if (file.numBytesToRead > file.charArraySize) {
                file.characters = new char[file.charArraySize];
            }
            else {
                file.characters = new char[file.numBytesToRead];
            }
            return file;
        }

        static void initRead(string fileName, ref Count globalCount) {
            Count localCount = new Count();
            FileData file = new FileData();
            localCount.Previous = false;
            using (file.fs = File.OpenRead(fileName)) {
                if ((int)file.fs.Length > (int)Math.Pow(2,16)) {
                    file.charArraySize = (int)Math.Pow(2,16);
                }
                else {
                    file.charArraySize = (int)file.fs.Length;
                }
                file.numBytesToRead = (long)file.fs.Length;
                file.characters = new char[file.charArraySize];
                while (file.numBytesToRead > 0) {
                    file = readChar(file);
                    localCount = wordCount(file.characters, localCount);
                    file = resetArray(file);
                }
            }
            Console.WriteLine("\t{0}\t{1}\t{2}\t{3}",localCount.Lines, localCount.Words, localCount.Characters, fileName);
            lock(globalCount.myLock) {
                globalCount.Words += localCount.Words;
                globalCount.Lines += localCount.Lines;
                globalCount.Characters += localCount.Characters;
            }
        }
        
        static void Main(string[] args) {
            Thread[] workerThreads = new Thread[args.Length];
            Count globalCount = new Count();
            globalCount.myLock = new Object();
            for (int i = 0; i < args.Length; i++) {
                int y = i;
                workerThreads[i] = new Thread(() => initRead(args[y], ref globalCount));
                workerThreads[i].IsBackground = true;
                workerThreads[i].Start();
            }
            for (int k = 0; k < args.Length; k++) {
                workerThreads[k].Join();
            }
            if (args.Length > 1) {
                Console.WriteLine("\t{0}\t{1}\t{2}\ttotal",globalCount.Lines, globalCount.Words, globalCount.Characters);
            }
        }
    }
}
