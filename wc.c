#include <stdio.h>
#include <stdbool.h>

void wordcount(FILE *ifp, int *cr, int *wd, int *ln, char *argv) {
    int c, word, character, line;
    word = line = character = 0;
    bool previous = false;
    bool valid = false;
    while((c = fgetc(ifp)) != EOF) {
        switch (c) {
            case '\n':
                line++;
                word++;
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
    *cr += character;
    *wd += word;
    *ln += line;
    printf("%d\t%d\t%d\t%s\n", line, word, character, argv);
}

int main(int argc, char *argv[]) {
    int character, word, line;
    word = line = character = 0;
    int *cr = &character;
    int *wd = &word;
    int *ln = &line;
    if (argc == 1) {
        char b;
        char *d = &b;
        wordcount(stdin, cr, wd, ln, d);    
        return 0;
    }
    else {
        int n = 0;
        FILE *fp;
        while (--argc > 0) {
            //Moves the pointer +1 before opening the file
            if ((fp = fopen(*++argv, "r")) == NULL) {
                fprintf(stderr, "Cant open %s\n", *argv);
            }
            else {
                wordcount(fp, cr, wd, ln, *argv);
                fclose(fp);
            }
            n++;
        }
        if (n > 1) {
            printf("%d\t%d\t%d\ttotal\n", line, word, character);
        }
    }
    return 0;
}
