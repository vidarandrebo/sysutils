#include <stdio.h>

void charcp(FILE *ifp) {
    int c;
    while((c = fgetc(ifp)) != EOF) {
        fputc(c, stdout);
    }
}

int main(int argc, char *argv[]) {
    if (argc == 1) {
        charcp(stdin);    
        return 0;
    }
    else {
        FILE *fp;
        while (--argc > 0) {
            //Moves the pointer +1 before opening the file
            if ((fp = fopen(*++argv, "r")) == NULL) {
                fprintf(stderr, "Can't open %s\n", *argv);
            }
            else {
                charcp(fp);
                fclose(fp);
            }
        }
    }
    return 0;
}
