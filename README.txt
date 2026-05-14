Открыть postgrSQL, далее win + r services.mcs там найти постгр и запустить его
Открыть диск c папка projects - далее vendingapi(файл sln) - далее в командной строке написать 1. dotnet build 2. dotnet run - проверить swagger и json
Открыть vendingDesktop - sln файл - данные для входа: email test2@test.com password 123456

Если забудете пароль или нужен новый пользователь:
Можете создать нового пользователя через Swagger (http://localhost:5000/swagger) → POST /api/Auth/register:
JSON
Copy
{
  "fullName": "Новый Пользователь",
  "email": "test3@test.com",
  "password": "123456",
  "role": "Администратор"
}

Открыть vendingWeb - sln файл - открыть http://localhost:5001/
