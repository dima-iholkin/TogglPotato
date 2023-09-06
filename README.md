# <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Flag_of_Ukraine.svg/1920px-Flag_of_Ukraine.svg.png" width="32" alt="Ukrainian flag"> TogglPotato utility for Toggl users

This is a small utility to organize your Toggl daily time entries over the daily 24 hour period.

<img src="/docs/assets/before_and_after.png" width="300" title="before and after using TogglPotato utility" />

* It supports Daylight saving time (DST), so it should work correctly even when the calendar day is not straight 24 hours.

## Usage

This is a server ASP.NET Core WebAPI application, so first you should start the server app and second make an HTTP request to it.

### HTTP request template

```http
POST /api/v1/organize_daily_time_entries HTTP/1.1
Host: example.com
Content-Type: application/json

{
    "togglApiKey": "your Toggl API token goes here",
    "date": "2023-08-30"
}
```

### How to get your Toggl API Token

Please do not give away your Toggl API token to untrustworthy parties, as it would give them full access to your Toggl account.

<img src="/docs/assets/TogglApiToken.png" title="instruction to get your Toggl API Token" />

## Hosting

1. ```git clone``` the repo.
2. ```dotnet run``` it or use VS Code or VS 2022 to start it.
3. Use Postman, cURL or any other HTTP client to make a request using the provided above HTTP request template.

At the moment the app requires **.NET 8 Preview 7**.

## License information

* **[MIT License](http://opensource.org/licenses/mit-license.php)**
* Copyright © 2023 <a href="https://github.com/dima-iholkin" target="_blank">Dima Iholkin</a>.