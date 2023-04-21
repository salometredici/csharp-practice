# prueba-csharp

Dejo disponible en la carpeta __*db*__ el script __*restore_script.sql*__ con el cual se puede hacer el restore de la base Postgresql que creé para realizar pruebas


La base tiene un schema llamado __*transactions*__ en el cual están todas las tablas, funciones y procedures utilizados para la resolución

También posee algunos datos precargados para facilitar las pruebas, por ejemplo:

* En la tabla __*users*__ hay 4 registros con sus respectivas contraseñas ya hasheadas
    - La password de los usuarios con ids 7 y 8 es '_password1234_'
* Ambos usuarios poseen dos cuentas de diferente moneda en la tabla __*accounts*__, por lo que sirven para hacer pruebas de transferencias a terceros y cuentas propias
    - UserId 7: Posee las cuentas con ids 9 y 10
    - UserId 8: Posee las cuentas con ids 11 y 12
## API

- #### Retorna todas las transferencias realizadas por el usuario logueado según los filtros enviados

```http
  GET /api/transactions?from={from}&to={to}&sourceAccountId={sourceAccountId}
```
Respuesta:

```
[
  {
    "id": 1,
    "accountFrom": 11,
    "originCurrency": "USD",
    "accountTo": 12,
    "destCurrency": "EUR",
    "amount": 150,
    "date": "2023-04-21T06:23:04.691",
    "description": "transferencia ctas propias"
  }
]
```


- #### Realiza una transferencia entre una cuenta de origen y una cuenta de destino. Puede ser entre cuentas propias o a terceros
__La cuenta de origen debe pertenecer al usuario logueado__


```http
  POST /api/transactions/transfer
```
Request de ejemplo:

```
{
  "accountFrom": 11,
  "accountTo": 12,
  "amount": 150,
  "date": "2023-04-21T09:23:04.691Z",
  "description": "transferencia ctas propias"
}
```
Respuesta:
```
{
  "transactionId": 1,
  "amountDebited": "150 USD",
  "commissionDebited": "0 USD",
  "amountTransferred": "136.85385 EUR"
}
```


- #### Registra a un usuario

```http
  POST /api/users/register
```
Request de ejemplo:

```
{
  "name": "Matt",
  "surname": "Dpe",
  "email": "mattdoe@mail.com",
  "password": "1234"
}
```


- #### Realiza el login de un usuario, retornando el jwtToken

```http
 POST /api/users/login
```
Request de ejemplo:

```
{
  "email": "mattdoe@mail.com",
  "password": "1234"
}
```
Respuesta:

```
{
  "jwtToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEwIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6Ik1hdHQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9zdXJuYW1lIjoiRHBlIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibWF0dGRvZUBtYWlsLmNvbSIsIm5iZiI6MTY4MjA2ODgzMSwiZXhwIjoxNjgyMDcyNDMxLCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjQ0MzExLyIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NDQzMTEvIn0.2_nLiIR1qGal2Yz04Yz59ml8H1p6hGhu-OLd_XpMyr8",
  "tokenExpDate": "2023-04-21T07:20:31.3565057-03:00",
  "id": 10,
  "fullName": "Matt Dpe",
  "email": "mattdoe@mail.com",
  "creationDate": "2023-04-21T06:18:06.813034",
  "lastLoginDate": "2023-04-21T06:20:31.337401-03:00"
}

```

## Notas

#### TransactionsController

Los dos endpoints de este controller requieren el JwtToken que se obtiene en el endpoint __api/users/login__ sea enviado como Authorization

#### Fixer-API

El consumo de esta API externa está limitado a 1 request por minuto, de recibir más requests arrojará la siguiente excepción:

```
Transactions.Domain.HttpException: The operation has been rate-limited and should be retried after 00:00:12.5192287
```

#### Endpoint api/transactions/transfer

El titular de la cuenta de origen __debe__ coincidir con el usuario logueado para poder realizar transferencias, sino arrojará la siguiente excepción:

```
Transactions.Domain.HttpException: Logged user differs from the user making the transfer
```
