using System;
using System.IO;
using System.Threading;

namespace wc_class {
    public class Count {
        public object myLock {get;set;}
        public long Words {get;set;}
        public long Lines{get;set;}
        public long Characters{get;set;}
        public bool Previous{get;set;}

        public void wordCount(char[] charArray) {
            foreach (char c in charArray) {
                switch(c) {
                    case '\n':
                        if (Previous == false) {
                            Words++;
                            Previous = true;
                        }
                        Lines++;
                        break;
                    case '\t':
                        if (Previous == false) {
                            Words++;
                            Previous = true;
                        }
                        break;
                    case ' ':
                        if (Previous == false) {
                            Words++;
                            Previous = true;
                        }
                        break;
                    default:
                        Previous = false;
                        break;
                }
                Characters++;
            }
        }
    }

    public class FileData {
        public FileStream fs {get;set;}
        public long numBytesToRead {get;set;}
        public int charArraySize{get;set;}
        public char[] characters{get;set;}

        public void resetArray() {
            if (numBytesToRead > charArraySize) {
                characters = new char[charArraySize];
            }
            else {
                characters = new char[numBytesToRead];
            }
        }

        public void readChar() {
            int currentByte;
            for (int j = 0; j < characters.Length && (currentByte = fs.ReadByte()) != -1; j++) {
                characters[j] = (char)currentByte; 
                numBytesToRead--;
            }
        }
        public void declareArrays() {
            if ((int)fs.Length > (int)Math.Pow(2,16)) {
                charArraySize = (int)Math.Pow(2,16);
            }
            else {
                charArraySize = (int)fs.Length;
            }
            numBytesToRead = (long)fs.Length;
            characters = new char[charArraySize];
        }
    }

    class Program {
        static void initRead(string fileName, Count globalCount) {
            Count localCount = new Count();
            FileData file = new FileData();
            localCount.Previous = false;
            using (file.fs = File.OpenRead(fileName)) {
                file.declareArrays();
                while (file.numBytesToRead > 0) {
                    file.readChar();
                    localCount.wordCount(file.characters);
                    file.resetArray();
                }
            }
            Console.WriteLine("\t{0}\t{1}\t{2}\t{3}",localCount.Lines, localCount.Words, localCount.Characters, fileName);
            lock(globalCount.myLock) {
                globalCount.Words += localCount.Words;
                globalCount.Lines += localCount.Lines;
                globalCount.Characters += localCount.Characters;
            }
        }

        static Count startThreads(string[] args) {
            Count globalCount = new Count();
            globalCount.myLock = new Object();
            Thread[] workerThreads = new Thread[args.Length];
            for (int i = 0; i < args.Length; i++) {
                int y = i;
                workerThreads[i] = new Thread(() => initRead(args[y], globalCount));
                workerThreads[i].IsBackground = true;
                workerThreads[i].Start();
            }
            for (int k = 0; k < args.Length; k++) {
                workerThreads[k].Join();
            }
            return globalCount;
        }
        
        static void Main(string[] args) {
            Count globalCount = startThreads(args);
            if (args.Length > 1) {
                Console.WriteLine("\t{0}\t{1}\t{2}\ttotal",globalCount.Lines, globalCount.Words, globalCount.Characters);
            }
        }
    }
}
