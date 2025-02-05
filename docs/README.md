# ToDo-API-Server

ToDo-API-Server is an enterprise-ready To-Do application API written in C# [.NET](https://dotnet.microsoft.com/en-us/learn/dotnet/what-is-dotnet) 9 core. It uses [PostgreSql](https://www.postgresql.org/) with [Npgsql](https://github.com/npgsql/npgsql) and [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) for database operations. Combined with [Swagger.io](https://swagger.io/) and [ReDoc](https://github.com/Redocly/redoc) for testing and documentation this To-Do API has it all. It can operate as a standalone application but can also be run from a [Docker](https://www.docker.com/) container. This application API also has rate limiting incorporated currently set to 30 requests a minute. 

There is only one or two things that are missing, only because the time frame for this project was short. Email integration, is very important for a production application, however I do not currently have an email server to route the smtp request through. Because of no email integration 2fa is affected, this I am well aware of. I'm sure if you need email integration that it is an easy addition and would not take long to complete.

## Requirements
The requirements to build or run this project are a follows.
##### For Building from source
Recommended: [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)   
Required: .NET 9.0 SDK - You can get from [here](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) 
##### For Running from Release
Required: .NET Runtime 9.0 - Also available from [here](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) 
##### For Running with Docker
Required: [Docker](https://www.docker.com/)

Note: Assuming you have a [PostgreSql](https://www.postgresql.org/) database server already ready, if not see [here](https://www.devart.com/dbforge/postgresql/how-to-install-postgresql-on-linux/) for linux or [here](https://www.w3schools.com/postgresql/postgresql_install.php) for windows. 

## Building

Clone the project with git using your favorite method e.g. git in a terminal
```bash
git clone https://github.com/thecryptoalien/ToDo-API-Server.git 
```
Build or debug the project using visiual studio, visual studio code, or using .NET cli tools. Below are a couple of examples including building a docker container. 

##### Building in a terminal with .NET cli tool
```bash
cd ToDo-API-Server/ToDo-API-Server
dotnet build 
```
##### Building a docker container
```bash
cd ToDo-API-Server/ToDo-API-Server
dotnet publish --self-contained --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer
```
Note: Docker on linux by default requires elevated privileges e.g. you may need to prepend sudo to the above command. Also, currently only x64 linux containers are supported.

## Running
There are quite a few ways to actually run the server including IIS, Docker, or even just the terminal.
Depending on if you build from source or download a release the methods vary. Here are a couple examples.
### Setting environment variables
For security reasons I like to use environment variables where possible. With .NET they have made it easy for us, see [here](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-9.0#security-and-user-secrets) for more on the way .NET core uses them. 
###### ConnectionStrings__AppDb -  Required if not building from source and using applicationSettings.json 
```bash
EXPORT ConnectionStrings__AppDb="Host=*HOST*; Port=*PORT*; Database=*DB*; Username=*USER*; Password=*PASS*;"
```
###### HostSettings__ApiUrl - Optional if you are hosting with a custom prefix like [here](https://danmanthenomad.com/ToDo-API-Server/) 
```bash
EXPORT HostSettings__ApiUrl="https://yourwebsite/To-Do-API"
```
###### SeedData__AdminEmail & SeedData__AdminPassword - Optional for seeding an inital or additional admin user 
```bash
EXPORT SeedData__AdminEmail="admin@email.com"
EXPORT SeedData__AdminPassword="adminPassword123!"
```
Note: These examples have placeholders like \*HOST*, \*PORT*, \*DB*, and others. Please replace them with the appropriate information for your usage.

### Running the application
If you are on windows or linux and you have built the application or docker container from source or downloaded and extracted one of the release files all the steps below are very similar. 

##### Running standalone application on linux
```bash
./ToDo-API-Server
```
##### Running standalone application on windows
```bash
ToDo-API-Server.exe
```

##### Running a docker container on windows or linux
```bash
docker run -it --rm -p 8080:8080 -e ASPNETCORE_HTTP_PORTS=8080 \
-e ConnectionStrings__AppDb="Host=*HOST*; Port=*PORT*; Database=*DB*; Username=*USER*; Password=*PASS*;" \
todo-api-server:latest
```

## Usage
Using the API is pretty much like any REST API, Authenticate and consume using available endpoints and actions. Authentication is required for the /ToDoEntities endpoint so lets start there. Remember rate limits are currently set to 30 requests per minute.

#### Register and login
This project uses the Microsoft .NET Identity Framework with the identity api endpoints. See [here](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-9.0#the-mapidentityapituser-endpoints) for more details on ASP.NET Core Identity and authorizing api endpoints. 


###### Register/Login JSON object
```json
{
  "email": "email@email.com",
  "password": "Password123!"
}
```
###### Example: Register and login
```python
# imports
import requests

# set base api url
BASE_URL = "http://localhost:8080"  # docker port on localhost

# register and login object
registerAndLogin = {
    "email": "email@email.com",
    "password": "Password123!"
}

# try to register user
print("Trying to Register User...")
registerResponse = requests.post(BASE_URL + "/register", json=registerAndLogin)
print("Status Code", registerResponse.status_code)
# check for failure and print
if (registerResponse.status_code != 200):
    print("JSON Register Response ", registerResponse.json())

# try to login user
print("Trying to Login User...")
loginResponse = requests.post(BASE_URL + "/login", json=registerAndLogin)
print("Status Code", loginResponse.status_code)
print("JSON login Response ", loginResponse.json())
```
###### Valid login response object
```json
{
    "tokenType": "Bearer",
    "accessToken": "CfDJ8HS7l2Rj6dhCr67O3xPPxnvC2pY6GeE_Vp5Dcf6shBdb5xb-NdYCBocV8mi9uI6yWacQV9C0-HE7M4gt...",
    "expiresIn": 3600,
    "refreshToken": "CfDJ8HS7l2Rj6dhCr67O3xPPxnvYf6o5mzhgJYZu34CxtgoE_Rn9Vp5Dcf6shBdb5xb_Vp5Dcf6shBdb5xb..."
}
```
Note: Make sure after the expiry time to refresh your token with the [/refresh](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-9.0#use-the-post-refresh-endpoint) endpoint using your refreshToken.

#### Using the /ToDoEntries endpoint
First things first, we need the correct headers to make a request most importantly "Authorization" here is an example.
###### Headers JSON object with authorization
```json
{
    "Accept": "application/json",
    "Authorization": "Bearer CfDJ8HS7l2Rj6dhCr67O3xPPxntnpPBadZwP3Agw0GKtPOXq26scus_EWpKzMEsrWZ65ncn0x7..."
}
```
###### Example: Calling the /ToDoEntries GET
```python
# set endpoint url
ENDPOINT_URL = BASE_URL + "/ToDoEntries"

# create headers with Authorization
headers = {
    "Accept": "application/json",
    "Authorization": "Bearer " + accessToken
}

# try ToDoEntries GET 
print("Trying ToDoEntries GET...")
getResponse = requests.get(ENDPOINT_URL, headers=headers)
print("Status Code", getResponse.status_code)
print("JSON ToDoEntries GET Response ", getResponse.json())
```
###### ToDoEntry JSON object
```json
{
  "title": "Title of ToDo Entry - Limit of 128 characters",
  "description": "Description of ToDo Entry - Limit of 512 characters",
  "status": 0 
}
```
###### Example: Calling the /ToDoEntries POST
```python
# ToDoEntry test object
toDoEntryObject = {
    "title": "ToDoEntry Test Object",
    "description": "This is a test ToDoEntry from python",
    "status": 0 # 0 = ToDo, 1 = Doing, 2 = Done <- Done requires approval from user with Admin role
}

# try ToDoEntries POST
postResponse = requests.post(ENDPOINT_URL, headers=headers, json=toDoEntryObject)
print("Status Code", postResponse.status_code)
print("JSON ToDoEntries POST Response ", postResponse.json())
```
###### Valid response from /ToDoEntries POST
```json
{
  "id": "9f51c596-94dd-4fe7-b4e8-08b817cf5c11",
  "title": "Title of ToDo Entry - Limit of 128 characters",
  "description": "Description of ToDo Entry - Limit of 512 characters",
  "status": 0,
  "pendingApproval": null,
  "createTime": "2025-02-05T21:47:52.556694Z",
  "updateTime": null,
  "approvedTime": null,
  "createdBy": "e82c302d-1ff8-461c-8f57-e0d8cddfbf69",
  "updatedBy": null,
  "approvedBy": null
}
```
###### Example: Calling the /ToDoEntries/id GET
```python
# create parameter object
toDoEntryId = postResponse.json()["id"]
params = {
    "id": toDoEntryId
}

# try ToDoEntries GET with Id param
getWithIdResponse = requests.get(ENDPOINT_URL + "/" + toDoEntryId, headers=headers, params=params)
print("Status Code", getWithIdResponse.status_code)
print("JSON ToDoEntries GET with Id Response ", getWithIdResponse.json())
```
###### Example: Calling the /ToDoEntries/id PUT
```python
# try ToDoEntries PUT
toDoEntryToUpdate = getWithIdResponse.json()
toDoEntryToUpdate["status"] = 2
putResponse = requests.put(ENDPOINT_URL + "/" + toDoEntryId, headers=headers, params=params, json=toDoEntryToUpdate)
print("Status Code", putResponse.status_code)
```
###### Example: Calling the /ToDoEntries/id,confirm PUT
```python
# create parameter object
confirmParams = {
    "id": toDoEntryId,
    "confirm": True
}

# try ToDoEntries confirm PUT
confirmPutResponse = requests.put(ENDPOINT_URL + "/" + toDoEntryId, headers=headers, params=confirmParams)
print("Status Code", confirmPutResponse.status_code)
```
###### Example: Calling the /ToDoEntries/id DELETE
```python
# try ToDoEntries DELETE
deleteResponse = requests.delete(ENDPOINT_URL + "/" + toDoEntryId, headers=headers, params=params)
print("Status Code", deleteResponse.status_code)
```
Note: In the above examples I used [Python3](https://www.python.org/downloads/) with the [requests](https://pypi.org/project/requests/) library. You can use what ever language or library to interact with this REST api   

## Documentation and testing
Currently there is an instance living on [danmanthenomad.com](https://danmanthenomad.com/ToDo-API-Server/) with a [Swagger UI](https://danmanthenomad.com/ToDo-API-Server/swagger/index.html) and [Api-Docs](https://danmanthenomad.com/ToDo-API-Server/api-docs/index.html). Please explore as you like but don't expect any records or logins to persist, as the database will be truncated regularly. 

## Contributing

Pull requests are welcome from anyone. For major changes, please open an issue first
to discuss what you would like to change.

## License

[MIT](docs/LICENSE.md)