# Madden Companion Export API

Этот репозиторий содержит **Madden Companion Export API** — простой сервис на ASP.NET Core, который принимает POST-запросы из приложения Madden Companion и сохраняет данные в JSON-файлы.

## 📋 Содержание
- [Требования](#требования)
- [Структура репозитория](#структура-репозитория)
- [Установка и запуск](#установка-и-запуск)
  - [1. Клонирование и подготовка](#1-клонирование-и-подготовка)
  - [2. Сборка и публикация](#2-сборка-и-публикация)
  - [3. Настройка systemd](#3-настройка-systemd)
  - [4. Конфигурация Nginx](#4-конфигурация-nginx)
- [Маршруты API](#маршруты-api)
- [Генерация CSV](#генерация-csv)
- [Проверка работоспособности](#проверка-работоспособности)

Требования

Структура репозитория

Установка и запуск

1. Клонирование и подготовка

2. Сборка и публикация

3. Настройка systemd

4. Конфигурация Nginx

Маршруты API

Генерация CSV

Проверка работоспособности

🛠 Требования

Debian 12+ / Ubuntu 20.04+

.NET SDK 8.0

Nginx

systemd

(Опционально) домен или nip.io-домен для HTTPS


## 📁 Структура репозитория
```
/opt/madden_api/
├─ src/                  # Исходники ASP.NET Core
│   └─ Program.cs
├─ publish/              # Папка публикации (dotnet publish)
└─ nginx/
    └─ madden.conf       # Конфиг Nginx для проксирования
```

## 🚀 Установка и запуск

### 1. Клонирование и подготовка
```bash
sudo mkdir -p /opt/madden_api
sudo chown $USER:$USER /opt/madden_api
cd /opt/madden_api
git clone <URL_РЕПОЗИТОРИЯ> src
```

### 2. Сборка и публикация
```bash
cd src
# Убедитесь, что в csproj указан <TargetFramework>net8.0</TargetFramework>
dotnet publish -c Release -o ../publish
```

### 3. Настройка systemd
Создайте файл `/etc/systemd/system/madden.service`:
```ini
[Unit]
Description=Madden Companion Export API
After=network.target

[Service]
WorkingDirectory=/opt/madden_api/publish
ExecStart=/usr/bin/dotnet /opt/madden_api/publish/Madden.Api.dll --urls http://0.0.0.0:5268
Restart=always
User=www-data
Group=www-data

[Install]
WantedBy=multi-user.target
```
```bash
sudo systemctl daemon-reload
sudo systemctl enable --now madden.service
```

### 4. Конфигурация Nginx
Скопируйте `nginx/madden.conf` в `/etc/nginx/sites-available/`, активируйте и перезагрузите:
```bash
sudo ln -sf /opt/madden_api/nginx/madden.conf /etc/nginx/sites-enabled/madden
sudo nginx -t && sudo systemctl reload nginx
```

*В конфиге используется HTTPS для `109-172-37-234.nip.io`. Замените на свой.*

## 🔌 Маршруты API
- `POST /{username}/{platform}/{league}/leagueteams` — сохраняет teams.json
- `POST /{username}/{platform}/{league}/standings` — standings.json
- `POST /{username}/{platform}/{league}/freeagents/roster` — freeagents.json
- `POST /{username}/{platform}/{league}/team/{team}/roster` — roster-{team}.json
- `POST /{username}/{platform}/{league}/week/{stage}/{week}/schedules` — schedules.json
- `POST /{username}/{platform}/{league}/week/{stage}/{week}/{stat}` — stats/{stat}.json

## 🗜 Генерация CSV
- `GET /{username}/{platform}/{league}/csv/teams` — CSV из \`leagueTeamInfoList\` teams.json
- `GET /{username}/{platform}/{league}/csv/freeagents` — CSV из \`rosterInfoList\` freeagents.json

## ✔️ Проверка работоспособности
```bash
curl -i https://<host>/   # Madden Companion Export API is up

POST /{username}/{platform}/{league}/freeagents/roster — freeagents.json

POST /{username}/{platform}/{league}/team/{team}/roster — roster-{team}.json

POST /{username}/{platform}/{league}/week/{stage}/{week}/schedules — schedules.json

POST /{username}/{platform}/{league}/week/{stage}/{week}/{stat} — stats/{stat}.json


# Здоровье сервиса
curl -i https://<host>/     # Madden Companion Export API is up

# Пример POST
curl -i -X POST https://<host>/<user>/pc/4110445/leagueteams \
  -H 'Content-Type: application/json' \
  -d '{"leagueTeamInfoList":[]}';

```
