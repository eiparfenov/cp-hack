# Backend для международного хакатона Цифровой Прорыв

Сервис предоставляет функционал для обработки фотографий животных из фотоловушек. Является связующим звеном между frontend и ml service.

## Использованный стек технологий
* фреймворк: asp net core;
* база данных - postgres;

## Конфигурация
Конфигурацию проекта следует осуществлять при помощи переменных окружения:

* S3Options__ServiceUrl - url к сервису s3;
* S3Options__BucketName - имя бакета;
* S3Options__AccessKeyId - id ключа доступа s3;
* S3Options__SecretAccessKey - секретный ключ доступа s3;
* ConnectionStrings__PostgresDb - строка подключения к postgres;
* ConnectionStrings__MlService - домен сервиса машинного обучения;

## Запуск
Есть два варианта для локального запуска сервиса:

1. Склонировать репозиторий и запустить в режиме разработки:
```bash
git clone https://github.com/eiparfenov/cp-hack.git
cd Server
dotnet run
```
Использовать docker образ из публичного репозитория:
```bash
docker run eiparfenov/cp-hack
```
!!! При запуске из контенера не забыть сконфигурировать проект через переменные окружения (параметр -e)

## Остальные проекты подключены как submodules