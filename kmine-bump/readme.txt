�������:

set ����������|�������->���������� [���������]
����������� ���������� ��������� ��������� ��� ��������� �������(�� 
���������).
�������:
ver - ������ ����� ��� exe/dll
datever - ������ ����� �� ������� YYYYMMDDHH
date - ���� ����������� �����
updated - ���� �� ���������� � ������
size - ������ �����

set cat c:\tmp
set src $cat\src
set size->filesize file.txt
set date->filechange file.txt
set ver->version file.exe
update * $src $dst
set updated->needupload


update ����|* �������-�������� �������-��Ȩ����
��������� ����, ���� �� ����������

update file.txt $src\info $dst\info


execute program params
��������� ������� �������

execute 7z a $dst\archive $src


label (�����:)
���������� ����� ��� ��������

lbl1:


remove ���� �������
������� ���� � ��������

remove archive.7z $dst


echo ���������
������� ���������

echo ������


template ������ ��������-����
������ ����, ���������� � ������ ����������

template file.tmpl $out\index.html


if [!] (exists ����)|$���������� stop|(goto �����)
��������� ������� �� ������� ���� ��� ������������� ���������� � ��� 
�������/���������� ������������� ��������� ��� ��������� � �����

if ! exists flag.dat stop
if $updated goto continue