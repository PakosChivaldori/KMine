Команды:

set ПЕРЕМЕННАЯ|ФУНКЦИЯ->ПЕРЕМЕННАЯ [ВЫРАЖЕНИЕ]
присваивает переменной результат выражения или результат функции(от 
выражения).
Функции:
ver - версия файла для exe/dll
datever - версия файла по шаблону YYYYMMDDHH
date - дата модификации файла
updated - было ли обновление в файлах
size - размер файла

set cat c:\tmp
set src $cat\src
set size->filesize file.txt
set date->filechange file.txt
set ver->version file.exe
update * $src $dst
set updated->needupload


update ФАЙЛ|* КАТАЛОГ-ИСТОЧНИК КАТАЛОГ-ПРИЁМНИК
Обновляет файл, если он отличается

update file.txt $src\info $dst\info


execute program params
запускает внешний процесс

execute 7z a $dst\archive $src


label (МЕТКА:)
Определяет метку для перехода

lbl1:


remove ФАЙЛ КАТАЛОГ
Удаляет файл в каталоге

remove archive.7z $dst


echo СООБЩЕНИЕ
Выводит сообщение

echo Превед


template ШАБЛОН ВЫХОДНОЙ-ФАЙЛ
Создаёт файл, подставляя в шаблон переменные

template file.tmpl $out\index.html


if [!] (exists ФАЙЛ)|$ПЕРЕМЕННАЯ stop|(goto МЕТКА)
проверяет условие на наличие файл или заполненность переменной и при 
наличии/отсутствии останавливает программу или переходит к метке

if ! exists flag.dat stop
if $updated goto continue