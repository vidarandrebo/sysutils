#include <stdio.h>
#include <stdbool.h>
#include <pthread.h>
#define BUFFERSIZE 65536

pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;

struct Count {
    long int line;
    long int word;
    long int character;
};

struct FileInfo {
    char *filename;
    struct Count *globalCount;
};

void wordcount(FILE *ifp, struct Count *globalCount, char *argv) {
    char buffer [BUFFERSIZE];
    long int word, character, line;
    word = line = character = 0;
    //previous is set to true when the current character was some form of whitespace
    bool previous = false;
    int n;
    while ((n = fread(buffer, sizeof(char), BUFFERSIZE, ifp)) != 0) {
        for (int i = 0; i < n; i++) {
            switch (buffer[i]) {
                case '\n':
                    if (previous == false) {
                        word++;
                        previous = true;
                    }
                    line++;
                    break;
                case '\t':
                    if (previous == false) {
                        word++;
                        previous = true;
                    }
                    break;
                case ' ':
                    if (previous == false) {
                        word++;
                        previous = true;
                    }
                    break;
                default:
                    previous = false;
            }
            character++;
        }
    }
    pthread_mutex_lock(&mutex);
    globalCount->character += character;
    globalCount->word += word;
    globalCount->line += line;
    pthread_mutex_unlock(&mutex);
    if (*argv == 'n') {
        printf("%ld\t%ld\t%ld\t\n", line, word, character);
    }
    else {
        printf("%ld\t%ld\t%ld\t%s\n", line, word, character, argv);
    }
}

void *createThreads(void *inputArgs) {
    struct FileInfo *threadArgs = (struct FileInfo *) inputArgs;
    FILE *fp;
    if ((fp = fopen(threadArgs->filename, "r")) == NULL) {
        fprintf(stderr, "Can't open %s\n", threadArgs->filename);
    }
    else {
        wordcount(fp, threadArgs->globalCount, threadArgs->filename);
        fclose(fp);
    }
    return NULL;
}

int main(int argc, char *argv[]) {
    struct Count globalCount;
    globalCount.word = 0;
    globalCount.character = 0;
    globalCount.line = 0;
    if (argc == 1) {
        char b = 'n';
        char *d = &b;
        wordcount(stdin, &globalCount, d);    
        return 0;
    }
    else {
        int n = 0;
        int numFiles = argc - 1;
        pthread_t threadArray [numFiles];
        struct FileInfo fileInfoArray [numFiles];
        for (int i = 0; i < numFiles; i++) {
            ++argv;
            fileInfoArray[i].filename = *argv;
            fileInfoArray[i].globalCount = &globalCount;
            pthread_create(&threadArray[i], NULL, createThreads, &fileInfoArray[i]);
            n++;
        }
        for (int i = 0; i < numFiles; i++) {
            pthread_join(threadArray[i], NULL);
        }
        if (n > 1) {
            printf("%ld\t%ld\t%ld\ttotal\n", globalCount.line, globalCount.word, globalCount.character);
        }
    }
    return 0;
}
