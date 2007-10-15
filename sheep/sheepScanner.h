#ifndef SHEEPSCANNER_H
#define SHEEPSCANNER_H

typedef struct yy_buffer_state * YY_BUFFER_STATE;
YY_BUFFER_STATE yy_scan_string(const char * str);
YY_BUFFER_STATE shp_scan_bytes(const char *bytes, int len);
void yy_delete_buffer(YY_BUFFER_STATE b);
int yylex();
extern char* yytext;

#endif // SHEEPSCANNER_H
